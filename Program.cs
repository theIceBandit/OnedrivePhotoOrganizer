using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;

namespace OnedrivePhotoOrganizer
{
    /// <summary>
    /// Represents the onedrive photo organizer application.
    /// </summary>
    public static class Program
    {
        private static IConfigurationRoot config;
        [STAThread]
        public static async Task Main(string[] args)
        {
            LoadConfiguration();
            var settings = new AppSettings();
            config.Bind(settings);

            var currentItem = string.Empty;
            try
            {
                var authHelper = new AuthenticationHelper(settings.ApplicationId, settings.RedirectUri);
                var graphClient = authHelper.GetGraphClient();

                var onedriveClient = new OnedriveHelper(graphClient, settings);
                var (children, totalItems) = await onedriveClient.GetPhotosAsync();

                if (totalItems == 0)
                {
                    Console.WriteLine("No items in photos folder!");
                    return;
                }

                var doneCount = 0;
                

                do
                {
                    foreach (var child in children)
                    {
                        currentItem = child.Name;
                        var newFolder = GetNewFolderName(child);
                        var folderRef = await onedriveClient.CreateFolderHiearchyAsync(newFolder);
                        await onedriveClient.MoveItemAsync(child, folderRef);

                        doneCount++;
                        Console.Write($"Done {doneCount} of {totalItems}...            \r");
                    }

                    (children, _) = await onedriveClient.GetPhotosAsync();
                } while (children != null && children.Count > 0 && doneCount < totalItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine(" ");
                Console.WriteLine(currentItem);
                Console.WriteLine(ex.ToString());
            }
        }

        private static void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("settings.json");
            config = builder.Build();
        }

        private static string GetNewFolderName(DriveItem item)
        {
            var fileType = GetFileType(item);

            var rootFolder=string.Empty;
            if (fileType is FileType.Photo or FileType.Unknown)
            {
                rootFolder = "/Camera";
            }
            if (fileType == FileType.Video)
            {
                rootFolder = "/Camera - Video";
            }

            // Take created date by default
            var dateFolder = item.CreatedDateTime.Value.LocalDateTime.ToString("yyyy/MM/dd");

            // Photo will be populated for both photos and videos
            if (item.Photo != null && item.Photo.TakenDateTime.HasValue)
            {
                // Take taken date
                dateFolder = item.Photo.TakenDateTime.Value.LocalDateTime.ToString("yyyy/MM/dd");
            }

            return $"{rootFolder}/{dateFolder}";
        }

        private static FileType GetFileType(DriveItem item)
        {
            var mimeType = item.File.MimeType.ToLower();
            var typeOfFile = mimeType.Substring(0, mimeType.IndexOf('/'));
            switch (typeOfFile)
            {
                case "image":
                    return FileType.Photo;
                case "video":
                    return FileType.Video;
                default:
                    return FileType.Unknown;
            }
        }
    }
}
