//using System.Text.RegularExpressions;

//namespace MediaManager.Code.Modules
//{
//    /// <summary>
//    /// An anime episode's metadata
//    /// </summary>
//    internal class AnimeFile : EpisodeFile
//    {
//        /// <summary>
//        /// This regex represents the naming format that Sonarr uses for anime episode filenames:
//        /// {Series TitleYear} - S{season:00}E{episode:00} - {absolute:000} - {Episode CleanTitle} [{Custom Formats }{Quality Title}]{[MediaInfo VideoDynamicRangeType]}[{MediaInfo VideoBitDepth}bit]{[MediaInfo VideoCodec]}[{Mediainfo AudioCodec} { Mediainfo AudioChannels}]{MediaInfo AudioLanguages}{-Release Group}
//        /// Example: "DAN DA DAN (2024) - S01E11 - 011 - First Love [WEBDL-1080p][8bit][x264][AAC 2.0][JA+EN]-MALD"
//        /// </summary>
//        private static readonly Regex animeEpRegex = new Regex(@"
//                        ^(?<Title>.+?)\s*(?:\((?<ReleaseYear>\d{4})\))?\s*-\s*
//                        S(?<SeasonNum>\d{2})E(?<EpisodeNum>\d{2})\s*-\s*
//                        (?<AbsoluteEpisode>\d{3})\s*-\s*
//                        (?<EpisodeTitle>.+?)\s*
//                        (?:\[(?<CustomFormats>[^]\[]*?)\s*(?<QualityTitle>[^]\[]*)\])?\s*
//                        (?:\[(?<VideoDynamicRange>HDR|SDR|Dolby Vision|HLG)\])?\s*
//                        (?:\[(?<VideoBitDepth>\d+)bit\])?\s*
//                        (?:\[(?<VideoCodec>x264|x265|AV1|VP9|H\.264|H\.265)\])?\s*
//                        (?:\[(?<AudioCodec>[^\]\s]+)\s+(?<AudioChannels>[\d.]+)\])?\s*
//                        (?:\[(?<AudioLanguages>[^\]]+)\])?\s*
//                        (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace);


//        // The absolute episode number (e.g. 00, 01 etc.)
//        public string AbsEpisodeNum { get; set; }

//        // The video's bit depth
//        public string VideoBitDepth { get; set; }

//        // The audio languages available
//        public string AudioLanguages { get; set; }
//    }
//}
