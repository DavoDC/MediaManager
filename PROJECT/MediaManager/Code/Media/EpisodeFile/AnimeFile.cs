using System.Text.RegularExpressions;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// An anime episode's metadata
    /// </summary>
    internal class AnimeFile : EpisodeFile
    {
        /// <summary>
        /// This regex represents the naming format that Sonarr uses for anime episode filenames:
        /// {Series TitleYear} - S{season:00}E{episode:00} - {absolute:000} - {Episode CleanTitle} [{Custom Formats}{Quality Title}]{[MediaInfo VideoDynamicRangeType]}[{MediaInfo VideoBitDepth}bit]{[MediaInfo VideoCodec]}[{Mediainfo AudioCodec} { Mediainfo AudioChannels}]{MediaInfo AudioLanguages}{-Release Group}
        /// Example: "DAN DA DAN (2024) - S01E11 - 011 - First Love [WEBDL-1080p][8bit][x264][AAC 2.0][JA+EN]-MALD"
        /// </summary>
        private static readonly Regex animeEpRegex = new Regex(@"
                        ^(?<Title>.+?)\s*(?:\((?<ReleaseYear>\d{4})\))?\s*-\s*
                        S(?<SeasonNum>\d{2})E(?<EpisodeNum>\d{2})\s*-\s*
                        (?<AbsoluteEpisode>\d{3})\s*-\s*
                        (?<EpisodeTitle>.+?)\s*
                        (?:\[(?<CustomFormats>[^]\[]*?)\s*(?<QualityTitle>[^]\[]*)\])?\s*
                        (?:\[(?<VideoDynamicRange>HDR|SDR|Dolby Vision|HLG)\])?\s*
                        (?:\[(?<VideoBitDepth>\d+)bit\])?\s*
                        (?:\[(?<VideoCodec>x264|x265|AV1|VP9|h264|h265)\])?\s*
                        (?:\[(?<AudioCodec>[^\]\s]+)\s+(?<AudioChannels>[\d.]+)\])?\s*
                        (?:\[(?<AudioLanguages>[^\]]+)\])?\s*
                        (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// The absolute episode number (e.g. 00, 01 etc.)
        /// </summary>
        public string AbsEpisodeNum { get; set; }

        /// <summary>
        /// The video's bit depth
        /// </summary>
        public string VideoBitDepth { get; set; }

        /// <summary>
        /// The audio languages available
        /// </summary>
        public string AudioLanguages { get; set; }

        /// <summary>
        /// Create an anime file
        /// </summary>
        /// <param name="mirrorFilePath"></param>
        public AnimeFile(string mirrorFilePath) : base(mirrorFilePath)
        {
        }

        /// <returns>The expected media type for anime files</returns>
        public override string GetExpectedType()
        {
            return "Anime";
        }

        /// <summary>
        /// Set anime specific properties using filename
        /// </summary>
        public override void InitialiseMediaTypeSpecificFieldsUsingMediaFileName()
        {
            // Try applying anime regex to media's filename
            Match animeMatch = animeEpRegex.Match(MediaFileName);
            if (animeMatch.Success)
            {
                // Set anime specific properties
                SetAnimeSpecificPropertiesFromFilename(animeMatch);

                // Fix title previously set
                // - The show regex used initially includes the absolute episode number in the title
                // - The title from the anime regex will not include this extra info
                EpisodeTitle = GetGroupValue(animeMatch, "EpisodeTitle");
            }
            else if (SeasonType.Equals("Special"))
            {
                // Else if anime parsing failed, and the file is a special,
                // try parsing anime special using the show episode regex
                Match animeSpecialMatch = showEpRegex.Match(MediaFileName);
                if (animeSpecialMatch.Success)
                {
                    // Set anime specific properties
                    SetAnimeSpecificPropertiesFromFilename(animeSpecialMatch);
                }
            }
            else
            {
                Prog.PrintErrMsg($"Could not parse anime: {RelativeFilePath}");
            }
        }

        /// <summary>
        /// Set anime specific properties extracted from the anime file's filename
        /// </summary>
        /// <param name="match">The regex match object</param>
        private void SetAnimeSpecificPropertiesFromFilename(Match match)
        {
            AbsEpisodeNum = GetGroupValue(match, "AbsoluteEpisode");
            VideoBitDepth = GetGroupValue(match, "VideoBitDepth");
            AudioLanguages = GetGroupValue(match, "AudioLanguages");
        }

        /// <summary>
        /// Add fields specific to anime to the XML document
        /// </summary>
        public override void AddMediaTypeSpecificFieldsToXMLDoc()
        {
            SetElementValue("AbsEpisodeNum", AbsEpisodeNum);
            SetElementValue("VideoBitDepth", VideoBitDepth);
            SetElementValue("AudioLanguages", AudioLanguages);
        }

        /// <summary>
        /// Initialise fields specific to anime using the XML document
        /// </summary>
        public override void InitialiseMediaTypeSpecificFieldsUsingXML()
        {
            AbsEpisodeNum = GetElementValue("AbsEpisodeNum");
            VideoBitDepth = GetElementValue("VideoBitDepth");
            AudioLanguages = GetElementValue("AudioLanguages");
        }

        /// <summary>
        /// Get fields specific to anime as a string
        /// </summary>
        public override string GetMediaTypeSpecificPropString()
        {
            string animeProps = "";
            animeProps += $"AbsEpisodeNum: {AbsEpisodeNum ?? "NULL"}\n";
            animeProps += $"VideoBitDepth: {VideoBitDepth ?? "NULL"}\n";
            animeProps += $"AudioLanguages: {AudioLanguages ?? "NULL"}\n";
            return animeProps;
        }
    }
}
