using System.IO;
using System.Text.RegularExpressions;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// An episode file's metadata (e.g. an anime or show episode)
    /// </summary>
    internal class EpisodeFile : MediaFile
    {
        /// <summary>
        /// This regex represents the naming format that Sonarr uses for series (anime/show) folders:
        /// {Series TitleYear} {tvdb-{TvdbId}}
        /// Example: "Clarkson's Farm (2021) {tvdb-378165}"
        /// </summary>
        private static readonly Regex showFolderRegex = new Regex(@"^(?<Title>.*?)\s\((?<Year>\d{4})\)\s\{(?<TVDBID>tvdb-\d+)\}$");

        /// <summary>
        /// This regex represents the naming format that Sonarr uses for TV show episode filenames:
        /// {Series TitleYear} - S{season:00}E{episode:00} - {Episode CleanTitle} [{Custom Formats }{Quality Title}]{[MediaInfo VideoDynamicRangeType]}{[Mediainfo AudioCodec}{ Mediainfo AudioChannels]}{[MediaInfo VideoCodec]}{-Release Group}
        /// Example: "Gen V (2023) - S01E06 - Jumanji [AMZN WEBDL-720p][EAC3 5.1][h264]-NTb"
        /// </summary>
        private static readonly Regex showEpRegex = new Regex(@"
                    ^(?<Title>.+?)\s*(?:\((?<ReleaseYear>\d{4})\))?\s*-\s*
                    S(?<SeasonNum>\d{2})E(?<EpisodeNum>\d{2})\s*-\s*
                    (?<EpisodeTitle>.+?)\s*
                    (?:\[(?<CustomFormats>[^]\[]*?)\s*(?<QualityTitle>[^]\[]*)\])?\s*
                    (?:\[(?<VideoDynamicRange>HDR|SDR|Dolby Vision|HLG)\])?\s*
                    (?:\[(?<VideoBitDepth>\d+)bit\])?\s*
                    (?:\[(?<VideoCodec>x264|x265|AV1|VP9|H\.264|H\.265)\])?\s*
                    (?:\[(?<AudioCodec>[^\]\s]+)\s+(?<AudioChannels>[\d.]+)\])?\s*
                    (?:\[(?<AudioLanguages>[^\]]+)\])?\s*
                    (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// The kind of show season (either "Regular" or "Special")
        /// </summary>
        public string SeasonType { get; set; }

        /// <summary>
        /// The season number (e.g. 00, 01 etc.)
        /// </summary>
        public string SeasonNum { get; set; }

        /// <summary>
        /// The episode number (e.g. 00, 01 etc.)
        /// </summary>
        public string EpisodeNum { get; set; }

        /// <summary>
        /// The episode's title
        /// </summary>
        public string EpisodeTitle { get; set; }

        /// <summary>
        /// Create an episode file
        /// </summary>
        /// <param name="mirrorFilePath"></param>
        public EpisodeFile(string mirrorFilePath) : base(mirrorFilePath)
        {
            CheckType("Show");
        }

        /// <summary>
        /// Initialise fields using the episode's folder name
        /// </summary>
        public override void InitialiseFieldsUsingMediaFolderName()
        {
            // Try to apply show folder regex to media folder name (e.g. "Clarkson's Farm (2021) {tvdb-378165}")
            Match mediaFolderMatch = showFolderRegex.Match(MediaFolderName);

            // If regex matched media folder name
            if (mediaFolderMatch.Success)
            {
                // Extract each part
                Title = mediaFolderMatch.Groups["Title"].Value;
                ReleaseYear = mediaFolderMatch.Groups["Year"].Value;

                // Handle database ID (TVDB)
                string[] databaseIDParts = mediaFolderMatch.Groups["TVDBID"].Value.Split('-');
                string databaseIDType = databaseIDParts[0];
                string databaseIDValue = databaseIDParts[1];
                if (databaseIDType.Equals("tvdb"))
                {
                    DatabaseLink = $"https://www.thetvdb.com/dereferrer/series/{databaseIDValue}";
                }
                else if (databaseIDType.Equals("tmdb"))
                {
                    DatabaseLink = $"https://www.themoviedb.org/movie/{databaseIDValue}";
                }
                else
                {
                    Prog.PrintErrMsg($"Unknown database ID type encountered: {databaseIDType}");
                }
            }
            else
            {
                Prog.PrintErrMsg($"Could not parse media folder: {MediaFolderName}");
            }

            // Get folder path
            string folderPath = Path.GetDirectoryName(RelativeFilePath);

            // Split the relative path by backslashes to extract path parts
            string[] folderPathParts = folderPath.Split('\\');

            // Get season/specials folder (e.g. "Season 01", "Specials")
            string seasonFolder = folderPathParts[3];
            if (seasonFolder.Contains("Season"))
            {
                SeasonType = "Regular";
                SeasonNum = seasonFolder.Split(' ')[1];
            }
            else if (seasonFolder.Equals("Specials"))
            {
                SeasonType = "Special";
                SeasonNum = "00";
            }
            else
            {
                Prog.PrintErrMsg($"Unknown season type encountered: {seasonFolder}");
            }
        }

        /// <summary>
        /// Initialise fields using the episode's filename
        /// </summary>
        public override void InitialiseFieldsUsingMediaFileName()
        {
            // Try applying show regex to media's filename
            Match showMatch = showEpRegex.Match(MediaFileName);
            if (showMatch.Success)
            {
                // Compare title, year and season in folder name vs file name
                CheckMismatch(showMatch, "Title", Title);
                CheckMismatch(showMatch, "ReleaseYear", ReleaseYear);
                CheckMismatch(showMatch, "SeasonNum", SeasonNum);

                // Set common properties
                CustomFormat = GetGroupValue(showMatch, "CustomFormat");
                QualityTitle = GetGroupValue(showMatch, "QualityTitle");
                VideoDynamicRange = GetGroupValue(showMatch, "VideoDynamicRange");
                VideoCodec = GetGroupValue(showMatch, "VideoCodec");
                AudioCodec = GetGroupValue(showMatch, "AudioCodec");
                AudioChannels = GetGroupValue(showMatch, "AudioChannels");
                ReleaseGroup = GetGroupValue(showMatch, "ReleaseGroup");

                // Set show-specific properties
                EpisodeNum = GetGroupValue(showMatch, "EpisodeNum");
                EpisodeTitle = GetGroupValue(showMatch, "EpisodeTitle");
            }
            else
            {
                Prog.PrintErrMsg($"Could not parse show: {RelativeFilePath}");
            }
        }

        /// <summary>
        /// Add fields specific to episodes to the XML document
        /// </summary>
        public override void AddSpecificFieldsToXMLDoc()
        {
            SetElementValue("SeasonType", SeasonType);
            SetElementValue("SeasonNum", SeasonNum);
            SetElementValue("EpisodeNum", EpisodeNum);
            SetElementValue("EpisodeTitle", EpisodeTitle);
        }

        /// <summary>
        /// Initialise fields specific to episodes using the XML document
        /// </summary>
        public override void InitialiseSpecificFieldsUsingXML()
        {
            SeasonType = GetElementValue("SeasonType");
            SeasonNum = GetElementValue("SeasonNum");
            EpisodeNum = GetElementValue("EpisodeNum");
            EpisodeTitle = GetElementValue("EpisodeTitle");
        }

        /// <summary>
        /// Get fields specific to episodes as a string
        /// </summary>
        public override string GetSpecificPropString()
        {
            string episodeProps = "";
            episodeProps += $"SeasonType: {SeasonType ?? "NULL"}\n";
            episodeProps += $"SeasonNum: {SeasonNum ?? "NULL"}\n";
            episodeProps += $"EpisodeNum: {EpisodeNum ?? "NULL"}\n";
            episodeProps += $"EpisodeTitle: {EpisodeTitle ?? "NULL"}\n";
            return episodeProps;
        }
    }
}
