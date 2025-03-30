using System.Text.RegularExpressions;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// A movie file's metadata
    /// </summary>
    internal class MovieFile : MediaFile
    {
        /// <summary>
        /// This regex represents the naming format that Radarr uses for movie folders:
        /// {Movie CleanTitle} ({Release Year}) {tmdb-{TmdbId}}
        /// Example: "Shrek the Third (2007) {tmdb-810}"
        /// </summary>
        private static readonly Regex movieFolderRegex = new Regex(@"^(?<Title>.*?)\s\((?<Year>\d{4})\)\s\{(?<TMDBID>tmdb-\d+)\}$");

        /// <summary>
        /// This regex represents the naming format that Radarr uses for movies:
        /// {Movie CleanTitle} {(Release Year)} {tmdb-{TmdbId}} {edition-{Edition Tags}} {[Custom Formats]}{[Quality Title]}{[MediaInfo 3D]}{[MediaInfo VideoDynamicRangeType]}{[Mediainfo AudioCodec}{ Mediainfo AudioChannels]}{[Mediainfo VideoCodec]}{-Release Group}
        /// Example: "Doctor Strange in the Multiverse of Madness (2022) {tmdb-453395} {edition-Imax} [DSNP IMAX Enhanced][WEBDL-720p][EAC3 Atmos 5.1][h264]-playWEB"
        /// </summary>
        private static readonly Regex movieRegex = new Regex(@"
                    ^(?<Title>.+?)\s*\((?<ReleaseYear>\d{4})\)\s*
                    \{(?<DBID>tmdb-\d+)\}\s*
                    (?:\{edition-(?<Edition>[^}]+)\}\s*)?
                    (?:\[(?<CustomFormat>[^\]]+)\])?
                    (?:\[(?<QualityTitle>[^\]]+)\])?
                    (?:\[(?<VideoDynamicRange>HDR|SDR|Dolby Vision|HLG)\])?
                    (?:\[(?<ThreeD>3D)\])?
                    (?:\[(?<VideoBitDepth>\d+)bit\])?
                    (?:\[(?<VideoCodec>x264|x265|AV1|VP9|h264|h265|ProRes)\])?
                    (?:\[(?<AudioCodec>[^\]\s]+(?:\s+[^\]\s]+)*)\s+(?<AudioChannels>[\d.]+)\])?
                    (?:\[(?<AudioLanguages>[^\]]+)\])?
                    (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

        /// <summary>
        /// The movie's edition/cut information (e.g. Directors Cut)
        /// </summary>
        public string Edition { get; set; }

        /// <summary>
        /// The movie's 3D info
        /// </summary>
        public string ThreeDInfo { get; set; }

        /// <summary>
        /// Construct a movie file
        /// </summary>
        /// <param name="mirrorFilePath"></param>
        public MovieFile(string mirrorFilePath) : base(mirrorFilePath)
        {
            CheckType("Movie");
        }

        /// <summary>
        /// Initialise fields by parsing the movie's folder name
        /// </summary>
        public override void InitialiseFieldsUsingMediaFolderName()
        {
            // Try to apply movie folder regex to media folder name (e.g. "8 Mile (2002) {tmdb-65}")
            Match mediaFolderMatch = movieFolderRegex.Match(MediaFolderName);

            // If regex matched media folder name
            if (mediaFolderMatch.Success)
            {
                // Extract each part
                Title = mediaFolderMatch.Groups["Title"].Value;
                ReleaseYear = mediaFolderMatch.Groups["Year"].Value;

                // Handle database ID (TMDB)
                string[] databaseIDParts = mediaFolderMatch.Groups["TMDBID"].Value.Split('-');
                string databaseIDType = databaseIDParts[0];
                string databaseIDValue = databaseIDParts[1];
                if (databaseIDType.Equals("tmdb"))
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
                // Else if the regex did not match the format of the media's folder name
                Prog.PrintErrMsg($"Could not parse media folder: {MediaFolderName}");
            }
        }

        /// <summary>
        /// Initialise fields using the movie's filename
        /// </summary>
        public override void InitialiseFieldsUsingMediaFileName()
        {
            // Try applying movie file regex to media's filename
            Match movieMatch = movieRegex.Match(MediaFileName);

            // If regex matched media filename
            if (movieMatch.Success)
            {
                // Compare title and year in folder name vs file name
                CheckMismatch(movieMatch, "Title", Title);
                CheckMismatch(movieMatch, "ReleaseYear", ReleaseYear);

                // Set common properties
                CustomFormat = GetGroupValue(movieMatch, "CustomFormat");
                QualityTitle = GetGroupValue(movieMatch, "QualityTitle");
                VideoDynamicRange = GetGroupValue(movieMatch, "VideoDynamicRange");
                VideoCodec = GetGroupValue(movieMatch, "VideoCodec");
                AudioCodec = GetGroupValue(movieMatch, "AudioCodec");
                AudioChannels = GetGroupValue(movieMatch, "AudioChannels");
                ReleaseGroup = GetGroupValue(movieMatch, "ReleaseGroup");

                // Set movie-specific properties
                Edition = GetGroupValue(movieMatch, "Edition");
                ThreeDInfo = GetGroupValue(movieMatch, "ThreeD");
            }
            else
            {
                // Else if the regex did not match the format of the media's filename
                Prog.PrintErrMsg($"Could not parse movie: {MediaFileName}");
            }
        }

        /// <summary>
        /// Add fields specific to movies to the XML document
        /// </summary>
        public override void AddSpecificFieldsToXMLDoc()
        {
            SetElementValue("Edition", Edition);
            SetElementValue("ThreeDInfo", ThreeDInfo);
        }

        /// <summary>
        /// Initialise fields specific to movies using the XML document
        /// </summary>
        public override void InitialiseSpecificFieldsUsingXML()
        {
            Edition = GetElementValue("Edition");
            ThreeDInfo = GetElementValue("ThreeDInfo");
        }

        /// <summary>
        /// Get fields specific to movies as a string
        /// </summary>
        public override string GetSpecificPropString()
        {
            string movieProps = "";
            movieProps += $"Edition: {Edition ?? "NULL"}\n";
            movieProps += $"ThreeDInfo: {ThreeDInfo ?? "NULL"}\n";
            return movieProps;
        }
    }
}