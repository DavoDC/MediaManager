//using System.Text.RegularExpressions;

//namespace MediaManager.Code.Modules
//{
//    /// <summary>
//    /// An episode file's metadata (e.g. an anime or show episode)
//    /// </summary>
//    internal class EpisodeFile : MediaFile
//    {
//        /// <summary>
//        /// This regex represents the naming format that Sonarr uses for series (anime/show) folders:
//        /// {Series TitleYear} {tvdb-{TvdbId}}
//        /// Example: "Clarkson's Farm (2021) {tvdb-378165}"
//        /// </summary>
//        private static readonly Regex showFolderRegex = new Regex(@"^(?<Title>.*?)\s\((?<Year>\d{4})\)\s\{(?<TVDBID>tvdb-\d+)\}$");

//        /// <summary>
//        /// This regex represents the naming format that Sonarr uses for TV show episode filenames:
//        /// {Series TitleYear} - S{season:00}E{episode:00} - {Episode CleanTitle} [{Custom Formats }{Quality Title}]{[MediaInfo VideoDynamicRangeType]}{[Mediainfo AudioCodec}{ Mediainfo AudioChannels]}{[MediaInfo VideoCodec]}{-Release Group}
//        /// Example: "Gen V (2023) - S01E06 - Jumanji [AMZN WEBDL-720p][EAC3 5.1][h264]-NTb"
//        /// </summary>
//        private static readonly Regex showEpRegex = new Regex(@"
//                    ^(?<Title>.+?)\s*(?:\((?<ReleaseYear>\d{4})\))?\s*-\s*
//                    S(?<SeasonNum>\d{2})E(?<EpisodeNum>\d{2})\s*-\s*
//                    (?<EpisodeTitle>.+?)\s*
//                    (?:\[(?<CustomFormats>[^]\[]*?)\s*(?<QualityTitle>[^]\[]*)\])?\s*
//                    (?:\[(?<VideoDynamicRange>HDR|SDR|Dolby Vision|HLG)\])?\s*
//                    (?:\[(?<VideoBitDepth>\d+)bit\])?\s*
//                    (?:\[(?<VideoCodec>x264|x265|AV1|VP9|H\.264|H\.265)\])?\s*
//                    (?:\[(?<AudioCodec>[^\]\s]+)\s+(?<AudioChannels>[\d.]+)\])?\s*
//                    (?:\[(?<AudioLanguages>[^\]]+)\])?\s*
//                    (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace);

//        // The kind of show season (either "Regular" or "Special")
//        public string SeasonType { get; set; }

//        // The season number (e.g. 00, 01 etc.)
//        public string SeasonNum { get; set; }

//        // The episode number (e.g. 00, 01 etc.)
//        public string EpisodeNum { get; set; }

//        // The episode's title
//        public string EpisodeTitle { get; set; }
//    }
//}
