namespace OnedrivePhotoOrganizer
{
    /// <summary>
    /// Represents the application settings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the application id.
        /// </summary>
        public string ApplicationId { get; set; }
        
        /// <summary>
        /// Gets or sets the application redirect uri.
        /// </summary>
        public string RedirectUri { get; set; }
        
        /// <summary>
        /// Gets or sets the photos folder path in Onedrive.
        /// </summary>
        public string PhotosFolderPath { get; set; }
    }
}
