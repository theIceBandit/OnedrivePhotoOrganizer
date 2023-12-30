using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnedrivePhotoOrganizer
{
    /// <summary>
    /// Represents the ondrive helper.
    /// </summary>
    internal class OnedriveHelper
    {
        private readonly GraphServiceClient client;
        private readonly AppSettings settings;

        public OnedriveHelper(GraphServiceClient client, AppSettings settings)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client), "The graph client cannot be null.");
            this.settings = settings;
        }

        /// <summary>
        /// Gets the list of photos.
        /// </summary>
        /// <returns>A list of photos in the onedrive folder.</returns>
        public async Task<(List<DriveItem>, int?)> GetPhotosAsync()
        {
            var photosFolder = await client.Drive.Root
                .ItemWithPath(settings.PhotosFolderPath)
                .Request()
                .Expand("children")
                .GetAsync();

            return (photosFolder.Children.Where(c => c.Folder == null).ToList(), photosFolder.Folder.ChildCount);
        }

        /// <summary>
        /// Creates the required folder hierarchy.
        /// </summary>
        /// <param name="folderPath">The folder path under which to create the hierarchy.</param>
        /// <returns>The folder drive.</returns>
        public async Task<DriveItem> CreateFolderHiearchyAsync(string folderPath)
        {
            var folderToCreate = new DriveItem { Folder = new Folder() };
            var folder = await client.Drive.Root
                .ItemWithPath(folderPath)
                .Request()
                .UpdateAsync(folderToCreate);

            return folder;
        }

        /// <summary>
        /// Moves the item to the specified folder path.
        /// </summary>
        /// <param name="item">The item to move.</param>
        /// <param name="newFolderPath">The new folder path.</param>
        /// <returns>The item that was moved.</returns>
        public async Task<DriveItem> MoveItemAsync(DriveItem item, DriveItem newFolderPath)
        {
            var newItem = new DriveItem
            {
                ParentReference = new ItemReference
                {
                    Id = newFolderPath.Id,
                },
                Name = item.Name,
            };

            DriveItem updatedItem = null;
            var tryCount = 1;
            var isSuccess = false;
            do
            {
                try
                {
                    updatedItem = await client.Drive.Items[item.Id]
                    .Request()
                    .UpdateAsync(newItem);

                    isSuccess = true;
                }
                catch (Exception)
                {
                    tryCount++;

                    if (tryCount > 3)
                    {
                        throw;
                    }

                    await Task.Delay(3000);
                }
            } while (tryCount <= 3 && !isSuccess);
            

            return updatedItem;
        }
    }
}
