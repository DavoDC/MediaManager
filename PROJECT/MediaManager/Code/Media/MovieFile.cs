using System.Text.RegularExpressions;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// A movie file's metadata
    /// </summary>
    internal class MovieFile : MediaFile
    {
        /// <summary>
        /// This regex represents the naming format that Radarr uses for movies:
        /// {Movie CleanTitle} {(Release Year)} {tmdb-{TmdbId}} - {edition-{Edition Tags}} {[MediaInfo 3D]}{[Custom Formats]}{[Quality Title]}{[Mediainfo AudioCodec}{ Mediainfo AudioChannels]}{[MediaInfo VideoDynamicRangeType]}{[Mediainfo VideoCodec]}{-Release Group}
        /// Example: "Doctor Strange in the Multiverse of Madness (2022) {tmdb-453395} - {edition-IMAX Enhanced} [DSNP IMAX Enhanced][WEBDL-1080p][EAC3 Atmos 5.1][h264]-FLUX"
        /// </summary>
        private static readonly Regex movieRegex = new Regex(@"
                    ^(?<Title>.+?)\s*\((?<ReleaseYear>\d{4})\)\s*
                    \{(?<DBID>tmdb-\d+)\}\s*-\s*
                    (?:\{edition-(?<Edition>[^}]+)\}\s*)?
                    (?:\[(?<ThreeD>3D)\])?
                    (?:\[(?<CustomFormat>(?!DVD\b)(?!\w+-\d{3,4}p)[^\]]+)\])?
                    (?:\[(?<QualityTitle>(?!EAC3 5\.1)[^\]]+)\])?
                    (?:\[(?<AudioCodec>EAC3|[^\]\s]+(?:\s+[^\]\s]+)*)\s+(?<AudioChannels>[\d.]+)\])?
                    (?:\[(?<VideoDynamicRange>HDR|SDR|HLG)\])?
                    (?:\[(?<VideoCodec>x264|x265|h264|h265|AVC)\])?
                    (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace);

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

        /// <returns>The database used for movies</returns>
        public override string GetExpectedDatabaseIDType()
        {
            return "tmdb";
        }

        /// <returns>A link to a movie in the movie database, given its ID</returns>
        public override string GetDatabaseLink(string id)
        {
            return $"https://www.themoviedb.org/movie/{id}";
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
                AudioCodec = GetGroupValue(movieMatch, "AudioCodec");
                AudioChannels = GetGroupValue(movieMatch, "AudioChannels");
                VideoCodec = GetGroupValue(movieMatch, "VideoCodec");
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