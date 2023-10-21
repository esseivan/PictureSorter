namespace PictureSorter
{
    /// <summary>
    /// Contains the settings of the application
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Number of images cached once shown to the user.
        /// </summary>
        public int CachedImagesCount { get; set; } = 5;

        /// <summary>
        /// The save file that contains the progress for the selected folder.
        /// Each of those files will be in their corresponding folders, hidden.
        /// </summary>
        public string AppSaveFileName { get; set; } = "pictureSorter.pssave";

        /// <summary>
        /// Which extension are compatible (shown to the user)
        /// </summary>
        public string[] FileExtensionsFilter { get; set; } =
            new string[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" };
    }
}
