using System.IO;
using System.Text.RegularExpressions;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// An episode file's metadata (e.g. an anime or show episode)
    /// </summary>
    internal abstract class EpisodeFile : MediaFile
    {
        /// <summary>
        /// This regex represents the naming format that Sonarr uses for TV show episode filenames:
        /// {Series TitleYear} - S{season:00}E{episode:00} - {Episode CleanTitle:90} {[Custom Formats]}{[Quality Title]}{[Mediainfo AudioCodec}{ Mediainfo AudioChannels]}{[MediaInfo VideoDynamicRangeType]}{[Mediainfo VideoCodec]}{-Release Group}
        /// Example: "Gen V (2023) - S01E06 - Jumanji [AMZN][WEBDL-720p][EAC3 5.1][h264]-NTb"
        /// </summary>
        protected static readonly Regex showEpRegex = new Regex(@"
                    ^(?<Title>.+?)\s*(?:\((?<ReleaseYear>\d{4})\))?\s*-\s*
                    S(?<SeasonNum>\d{2})E(?<EpisodeNum>\d{2})\s*-\s*
                    (?<EpisodeTitle>.+?)\s*
                    (?:\[(?<CustomFormat>(?!EAC3|AC3|AAC|DTS)[^\]]+)\])?
                    (?:\[(?<QualityTitle>(?!EAC3|AC3|AAC|DTS)[^\]]+)\])?
                    (?:\[(?<AudioCodec>[^\]\s]+(?:\s+[^\]\s]+)*)\s+(?<AudioChannels>[\d.]+)\])?
                    (?:\[(?<VideoDynamicRange>HDR|SDR|HLG)\])?
                    (?:\[(?<VideoBitDepth>\d+)bit\])?
                    (?:\[(?<VideoCodec>x264|x265|h264|h265|MPEG2)\])?
                    (?:\[(?<AudioLanguages>[^\]]+)\])?
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
            CheckType(GetExpectedType());
        }

        /// <returns>A concise string representing this episode file</returns>
        public override string ToString()
        {
            return $"{base.ToString()}: {GetSeasonEpStr()}, {EpisodeTitle}";
        }

        /// <returns>The database used for shows</returns>
        public override string GetExpectedDatabaseIDType()
        {
            return "tvdb";
        }

        /// <returns>A link to a show in the show database, given its ID</returns>
        /// Example: https://www.thetvdb.com/dereferrer/series/248035
        /// Alternate: https://www.thetvdb.com/?tab=series&id=248035
        public override string GetDatabaseLink(string id)
        {
            return $"https://www.thetvdb.com/dereferrer/series/{id}";
        }

        /// <summary>
        /// Initialise fields using the episode's folder name
        /// </summary>
        public override void InitialiseFieldsUsingMediaFolderName()
        {
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
                AudioCodec = GetGroupValue(showMatch, "AudioCodec");
                AudioChannels = GetGroupValue(showMatch, "AudioChannels");
                VideoCodec = GetGroupValue(showMatch, "VideoCodec");
                ReleaseGroup = GetGroupValue(showMatch, "ReleaseGroup");

                // Set episode-specific properties
                EpisodeNum = GetGroupValue(showMatch, "EpisodeNum");
                EpisodeTitle = GetGroupValue(showMatch, "EpisodeTitle");

                // Set media-type-specific properties using filename
                InitialiseMediaTypeSpecificFieldsUsingMediaFileName();
            }
            else
            {
                Prog.PrintErrMsg($"Could not parse show: {RelativeFilePath}");
            }
        }

        /// <summary>
        /// Set media-type-specific properties using filename
        /// </summary>
        public virtual void InitialiseMediaTypeSpecificFieldsUsingMediaFileName()
        {
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

            AddMediaTypeSpecificFieldsToXMLDoc();
        }

        /// <summary>
        /// Add fields specific to an episodic media-type to the XML document
        /// </summary>
        public virtual void AddMediaTypeSpecificFieldsToXMLDoc()
        {
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

            InitialiseMediaTypeSpecificFieldsUsingXML();
        }

        /// <summary>
        /// Initialise fields specific to an episodic media-type using the XML document
        /// </summary>
        public virtual void InitialiseMediaTypeSpecificFieldsUsingXML()
        {
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
            episodeProps += GetMediaTypeSpecificPropString();
            return episodeProps;
        }

        /// <summary>
        /// Get fields specific to an episodic media-type as a string
        /// </summary>
        public virtual string GetMediaTypeSpecificPropString()
        {
            return "";
        }

        /// <summary>
        /// Returns the expected media type as a string
        /// </summary>
        public abstract string GetExpectedType();

        /// <returns>Season-episode code as a string (e.g. S02E06)</returns>
        public string GetSeasonEpStr()
        {
            return $"S{SeasonNum}E{EpisodeNum}";
        }
    }
}
