using System.IO;
using System.Text.RegularExpressions;
using File = System.IO.File;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// An media file as a "tag" containing metadata.
    /// </summary>
    internal class MediaTag : MediaFile
    {
        // Regex for anime/show folders (e.g. "A Certain Magical Index (2008) {tvdb-83322}")
        Regex showFolderRegex = new Regex(@"^(?<Title>.*?)\s\((?<Year>\d{4})\)\s\{(?<TVDBID>tvdb-\d+)\}$");

        // Anime episode format in Sonarr
        // {Series TitleYear} - S{season:00}E{episode:00} - {absolute:000} - {Episode CleanTitle} [{Custom Formats }{Quality Title}]{[MediaInfo VideoDynamicRangeType]}[{MediaInfo VideoBitDepth}bit]
        // {[MediaInfo VideoCodec]}[{Mediainfo AudioCodec} { Mediainfo AudioChannels}]{MediaInfo AudioLanguages}{-Release Group}
        // Regex for anime episodes (e.g. "A Certain Magical Index (2008) - S01E01 - 001 - Academy City [HDTV-720p][8bit][x264][AAC 2.0][JA]")
        Regex animeEpRegex = new Regex(@"
                ^(?<Title>.+?)\s*(?:\((?<ReleaseYear>\d{4})\))?\s*-\s*
                S(?<SeasonNum>\d{2})E(?<EpisodeNum>\d{2})\s*-\s*
                (?<AbsoluteEpisode>\d{3})\s*-\s*
                (?<EpisodeTitle>.+?)\s*
                (?:\[(?<CustomFormats>[^]\[]*?)\s*(?<QualityTitle>[^]\[]*)\])?\s*
                (?:\[(?<VideoDynamicRange>HDR|SDR|Dolby Vision|HLG)\])?\s*
                (?:\[(?<VideoBitDepth>\d+)bit\])?\s*
                (?:\[(?<VideoCodec>x264|x265|AV1|VP9|H\.264|H\.265)\])?\s*
                (?:\[(?<AudioCodec>[^\]\s]+)\s+(?<AudioChannels>[\d.]+)\])?\s*
                (?:\[(?<AudioLanguages>[^\]]+)\])?\s*
                (?:-(?<ReleaseGroup>[^\]]+))?$
            ", RegexOptions.IgnorePatternWhitespace);

        // Show episode format in Sonarr
        // {Series TitleYear} - S{season:00}E{episode:00} - {Episode CleanTitle} [{Custom Formats }{Quality Title}]{[MediaInfo VideoDynamicRangeType]}{[Mediainfo AudioCodec}{ Mediainfo AudioChannels]}{[MediaInfo VideoCodec]}{-Release Group}
        // TODO?

        // Movie format in Radarr
        // {Movie CleanTitle} {(Release Year)} {tmdb-{TmdbId}} {edition-{Edition Tags}} {[Custom Formats]}{[Quality Title]}{[MediaInfo 3D]}{[MediaInfo VideoDynamicRangeType]}{[Mediainfo AudioCodec}{ Mediainfo AudioChannels]}{[Mediainfo VideoCodec]}{-Release Group}
        // TODO?

        /// <summary>
        /// Construct a tag
        /// </summary>
        /// <param name="mirrorFilePath">The mirror file path</param>
        public MediaTag(string mirrorFilePath)
        {
            // Initialise relative path (always)
            int relStartPos = mirrorFilePath.LastIndexOf(Prog.MirrorFolderName);
            RelPath = mirrorFilePath.Remove(0, relStartPos + Prog.MirrorFolderName.Length);

            // Get file contents of mirror file (should be one real file path)
            string[] fileContents = File.ReadAllLines(mirrorFilePath);
            bool mirrorFileContainsOnePath = fileContents.Length == 1;
            
            // Check validity of real file path
            string realFilePath = Reflector.FixLongPath(fileContents[0]);
            bool mirrorFilePathIsValid = File.Exists(realFilePath);

            // If the mirror contains a single, valid path
            if (mirrorFileContainsOnePath && mirrorFilePathIsValid)
            {
                // Initialise fields using folder path and filename
                ProcessFolderPath();
                ProcessFileName();

                // Extract media extension from real file path
                Extension = Path.GetExtension(realFilePath);

                // Overwrite mirror file contents with XML content
                MediaXML xmlFileOut = new MediaXML(mirrorFilePath, this);
            }
            else
            {
                // Else if the mirror file is a valid XML file

                // Read in XML data
                // MediaXML xmlFileIn = new MediaXML(mirrorFilePath);


                // Set tag properties using XML file data
                // TODO

                // EXAMPLES
                //    Title = xmlFileIn.Title;
                //    Artists = xmlFileIn.Artists;
                //    Album = xmlFileIn.Album;
                //    Year = xmlFileIn.Year;
                //    TrackNumber = xmlFileIn.TrackNumber;
                //    Genres = xmlFileIn.Genres;
                //    Length = xmlFileIn.Length;
                //    AlbumCoverCount = xmlFileIn.AlbumCoverCount;
                //    Compilation = xmlFileIn.Compilation;
            }
        }

        private void ProcessFolderPath()
        {
            // Get full relative path but without filename
            string folderPath = Path.GetDirectoryName(RelPath);  // e.g. "C:\Anime\A Certain Magical Index(2008) { tvdb - 83322}\Season 01"

            // Split the relative path by backslashes to extract path parts
            string[] folderPathParts = folderPath.Split('\\');

            // Extract and save the media's type
            string rawMediaType = folderPathParts[1];
            Type = rawMediaType.Replace("s", "");  // e.g. Anime/Movies/Shows

            // Process media folder
            string mediaFolderName = folderPathParts[2];  // e.g. A folder name (e.g. "A Certain Magical Index (2008) {tvdb-83322}", "8 Mile (2002) {tmdb-65}", etc.)
            Match mediaFolderMatch = showFolderRegex.Match(mediaFolderName);
            if (mediaFolderMatch.Success)
            {
                // Extract each part
                Title = mediaFolderMatch.Groups["Title"].Value;
                ReleaseYear = mediaFolderMatch.Groups["Year"].Value;

                // Handle database ID
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
                Prog.PrintErrMsg($"Could not parse media folder: {mediaFolderName}");
            }

            // Get season/specials folder or movie filename
            // e.g. A season folder (e.g. "Season 01") or movie file (e.g "8 Mile (2002) {tmdb-65} [Bluray-720p][EAC3 5.1][x264]-playHD")
            string seasonFolderOrMovieFile = folderPathParts[3];

            // If this an anime or show
            if(rawMediaType.Equals(Prog.AnimeFolderName) || rawMediaType.Equals(Prog.ShowFolderName))
            {
                // This is a kind of season folder
                if (seasonFolderOrMovieFile.Contains("Season"))
                {
                    SeasonType = "Regular";
                    SeasonNum = seasonFolderOrMovieFile.Split(' ')[1];
                }
                else if (seasonFolderOrMovieFile.Equals("Specials"))
                {
                    SeasonType = "Special";
                    SeasonNum = "00";
                }
                else
                {
                    Prog.PrintErrMsg($"Unknown season type encountered: {seasonFolderOrMovieFile}");
                }
            }
        }

        private void ProcessFileName()
        {
            // Get file name without extension
            string filename = Path.GetFileNameWithoutExtension(RelPath);

            // If this is an anime
            if (Type.Equals(Prog.AnimeFolderName))
            {
                // Try to extract anime info
                Match animeMatch = animeEpRegex.Match(filename);
                if (animeMatch.Success)
                {
                    // Ensure episode show title matches one from folder found earlier
                    CheckMismatch(animeMatch, "Title", Title);

                    // Ensure episode release year matches one from folder found earlier
                    CheckMismatch(animeMatch, "ReleaseYear", ReleaseYear);

                    // Ensure episode season num matches one from folder found earlier
                    CheckMismatch(animeMatch, "SeasonNum", SeasonNum);

                    // Save fields
                    EpisodeNum = GetGroupValue(animeMatch, "Episode");
                    AbsEpisodeNum = GetGroupValue(animeMatch, "AbsoluteEpisode");
                    EpisodeTitle = GetGroupValue(animeMatch, "EpisodeTitle");
                    CustomFormats = GetGroupValue(animeMatch, "CustomFormats"); // Note: Almost always Unknown
                    QualityTitle = GetGroupValue(animeMatch, "QualityTitle");
                    VideoDynamicRange = GetGroupValue(animeMatch, "VideoDynamicRange"); // Note: Almost always Unknown
                    VideoBitDepth = GetGroupValue(animeMatch, "VideoBitDepth");
                    VideoCodec = GetGroupValue(animeMatch, "VideoCodec");
                    AudioCodec = GetGroupValue(animeMatch, "AudioCodec");
                    AudioChannels = GetGroupValue(animeMatch, "AudioChannels");
                    AudioLanguages = GetGroupValue(animeMatch, "AudioLanguages");
                    ReleaseGroup = GetGroupValue(animeMatch, "ReleaseGroup");             
                }
                else
                {
                    Prog.PrintErrMsg($"Could not parse anime: {RelPath}");
                }
            }
            else if (Prog.ShowFolderName.Contains(Type))
            {
                // Else if this is a show
                // TODO
            }
            else if (Prog.MovieFolderName.Contains(Type))
            {
                // Else if this is a movie
                // TODO
            }
            else
            {
                Prog.PrintErrMsg($"Unexpected media type: {RelPath}");
            }
        }

        /// <summary>
        /// Retrieves the value of a specified group from a regex match, or returns a default value if the group is empty or null.
        /// </summary>
        /// <param name="match">The regex match object containing the groups to extract data from.</param>
        /// <param name="groupName">The name of the group whose value is to be retrieved.</param>
        /// <param name="defaultValue">The default value to return if the group value is empty or null (default is "Unknown").</param>
        /// <returns>The value of the specified group, or the default value if the group is empty or null.</returns>
        public string GetGroupValue(Match match, string groupName, string defaultValue = "Unknown")
        {
            return string.IsNullOrEmpty(match.Groups[groupName].Value) ? defaultValue : match.Groups[groupName].Value;
        }

        /// <summary>
        /// Compares a specific group value from the filename match with the corresponding expected value 
        /// and prints an error message if they don't match.
        /// </summary>
        /// <param name="animeMatch">The match object containing the filename data.</param>
        /// <param name="groupName">The name of the group to retrieve from the match.</param>
        /// <param name="expectedValue">The expected value to compare against.</param>
        private void CheckMismatch(Match animeMatch, string groupName, string expectedValue)
        {
            // If the value doesn't match the expected value, print an error message
            if (!GetGroupValue(animeMatch, groupName).Equals(expectedValue))
            {
                // Print an error message
                Prog.PrintErrMsg($"Folder and episode '{groupName}' mismatch: {RelPath}");
            }
        }
    }
}
