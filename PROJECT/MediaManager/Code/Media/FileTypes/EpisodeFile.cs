namespace MediaManager.Code.Modules
{
    /// <summary>
    /// An episode file's metadata (e.g. an anime or show episode)
    /// </summary>
    internal class EpisodeFile : MediaFile
    {
        // The kind of show season (either "Regular" or "Special")
        public string SeasonType { get; set; }

        // The season number (e.g. 00, 01 etc.)
        public string SeasonNum { get; set; }

        // The episode number (e.g. 00, 01 etc.)
        public string EpisodeNum { get; set; }

        // The episode's title
        public string EpisodeTitle { get; set; }
    }
}
