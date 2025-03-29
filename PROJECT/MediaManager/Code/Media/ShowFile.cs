namespace MediaManager.Code.Modules
{
    /// <summary>
    /// A show episode's metadata
    /// </summary>
    internal class ShowFile : EpisodeFile
    {
        /// <summary>
        /// Create a show episode file
        /// </summary>
        /// <param name="mirrorFilePath"></param>
        public ShowFile(string mirrorFilePath) : base(mirrorFilePath)
        {
        }

        /// <returns>The expected media type for show files</returns>
        public override string GetExpectedType()
        {
            return "Show";
        }
    }
}
