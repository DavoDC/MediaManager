namespace MediaManager.Code.Modules
{
    /// <summary>
    /// An anime episode's metadata
    /// </summary>
    internal class AnimeFile : EpisodeFile
    {
        // The absolute episode number (e.g. 00, 01 etc.)
        public string AbsEpisodeNum { get; set; }

        // The video's bit depth
        public string VideoBitDepth { get; set; }

        // The audio languages available
        public string AudioLanguages { get; set; }
    }
}
