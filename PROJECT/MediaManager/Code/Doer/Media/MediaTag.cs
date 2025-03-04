using System;
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
        /// <summary>
        /// This regex represents the naming format that Sonarr uses for series (anime/show) folders:
        /// {Series TitleYear} {tvdb-{TvdbId}}
        /// Example: "Clarkson's Farm (2021) {tvdb-378165}"
        /// </summary>
        Regex showFolderRegex = new Regex(@"^(?<Title>.*?)\s\((?<Year>\d{4})\)\s\{(?<TVDBID>tvdb-\d+)\}$");

        /// <summary>
        /// This regex represents the naming format that Radarr uses for movie folders:
        /// {Movie CleanTitle} ({Release Year}) {tmdb-{TmdbId}}
        /// Example: "Shrek the Third (2007) {tmdb-810}"
        /// </summary>
        Regex movieFolderRegex = new Regex(@"^(?<Title>.*?)\s\((?<Year>\d{4})\)\s\{(?<TMDBID>tmdb-\d+)\}$");

        /// <summary>
        /// This regex represents the naming format that Sonarr uses for anime episode filenames:
        /// {Series TitleYear} - S{season:00}E{episode:00} - {absolute:000} - {Episode CleanTitle} [{Custom Formats }{Quality Title}]{[MediaInfo VideoDynamicRangeType]}[{MediaInfo VideoBitDepth}bit]{[MediaInfo VideoCodec]}[{Mediainfo AudioCodec} { Mediainfo AudioChannels}]{MediaInfo AudioLanguages}{-Release Group}
        /// Example: "DAN DA DAN (2024) - S01E11 - 011 - First Love [WEBDL-1080p][8bit][x264][AAC 2.0][JA+EN]-MALD"
        /// </summary>
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
                (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// This regex represents the naming format that Sonarr uses for TV show episode filenames:
        /// {Series TitleYear} - S{season:00}E{episode:00} - {Episode CleanTitle} [{Custom Formats }{Quality Title}]{[MediaInfo VideoDynamicRangeType]}{[Mediainfo AudioCodec}{ Mediainfo AudioChannels]}{[MediaInfo VideoCodec]}{-Release Group}
        /// Example: "Gen V (2023) - S01E06 - Jumanji [AMZN WEBDL-720p][EAC3 5.1][h264]-NTb"
        /// </summary>
        Regex showEpRegex = new Regex(@"
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
        /// This regex represents the naming format that Radarr uses for movies:
        /// {Movie CleanTitle} {(Release Year)} {tmdb-{TmdbId}} {edition-{Edition Tags}} {[Custom Formats]}{[Quality Title]}{[MediaInfo 3D]}{[MediaInfo VideoDynamicRangeType]}{[Mediainfo AudioCodec}{ Mediainfo AudioChannels]}{[Mediainfo VideoCodec]}{-Release Group}
        /// Example: "Doctor Strange in the Multiverse of Madness (2022) {tmdb-453395} {edition-Imax} [DSNP IMAX Enhanced][WEBDL-720p][EAC3 Atmos 5.1][h264]-playWEB"
        /// </summary>
        Regex movieRegex = new Regex(@"
            ^(?<Title>.+?)\s*\((?<ReleaseYear>\d{4})\)\s*
            \{(?<DBID>tmdb-\d+)\}\s*
            (?:\{edition-(?<Edition>[^}]+)\}\s*)?
            (?:\[(?<CustomFormats>[^]\[]+)\])?
            (?:\[(?<QualityTitle>[^]\[]+)\])?
            (?:\[(?<VideoDynamicRange>HDR|SDR|Dolby Vision|HLG)\])?  
            (?:\[(?<ThreeD>3D)\])?  
            (?:\[(?<VideoBitDepth>\d+)bit\])? 
            (?:\[(?<VideoCodec>x264|x265|AV1|VP9|H\.264|H\.265|ProRes)\])?  
            (?:\[(?<AudioCodec>[^\]\s]+)\s+(?<AudioChannels>[\d.]+)\])?
            (?:\[(?<AudioLanguages>[^\]]+)\])?
            (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);



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
                MediaXML xmlFileIn = new MediaXML(mirrorFilePath);

                // Set tag properties using XML file data
                Title = xmlFileIn.GetElementValue("Title");
                ReleaseYear = xmlFileIn.GetElementValue("ReleaseYear");
                DatabaseLink = xmlFileIn.GetElementValue("DatabaseLink");
                Extension = xmlFileIn.GetElementValue("Extension");
                Edition = xmlFileIn.GetElementValue("Edition");
                CustomFormats = xmlFileIn.GetElementValue("CustomFormats");
                QualityTitle = xmlFileIn.GetElementValue("QualityTitle");
                VideoDynamicRange = xmlFileIn.GetElementValue("VideoDynamicRange");
                ThreeDInfo = xmlFileIn.GetElementValue("ThreeD");
                VideoCodec = xmlFileIn.GetElementValue("VideoCodec");
                AudioCodec = xmlFileIn.GetElementValue("AudioCodec");
                AudioChannels = xmlFileIn.GetElementValue("AudioChannels");
                AudioLanguages = xmlFileIn.GetElementValue("AudioLanguages");
                ReleaseGroup = xmlFileIn.GetElementValue("ReleaseGroup");
                SeasonType = xmlFileIn.GetElementValue("SeasonType");
                SeasonNum = xmlFileIn.GetElementValue("SeasonNum");
                EpisodeNum = xmlFileIn.GetElementValue("EpisodeNum");
                EpisodeTitle = xmlFileIn.GetElementValue("EpisodeTitle");
                AbsEpisodeNum = xmlFileIn.GetElementValue("AbsEpisodeNum");
                VideoBitDepth = xmlFileIn.GetElementValue("VideoBitDepth");
            }
        }

        private void ProcessFolderPath()
        {
            // Get full relative path but without filename
            // e.g. "\Anime\A Certain Magical Index (2008) {tvdb-83322}\Season 01"
            // or "\Movies\8 Mile (2002) {tmdb-65}\"
            string folderPath = Path.GetDirectoryName(RelPath);

            // Split the relative path by backslashes to extract path parts
            string[] folderPathParts = folderPath.Split('\\');

            // Extract and save the media's type
            string rawMediaType = folderPathParts[1];
            Type = rawMediaType.Replace("s", "");  // e.g. Anime/Movies/Shows

            // Get media folder (e.g. "A Certain Magical Index (2008) {tvdb-83322}", "8 Mile (2002) {tmdb-65}", etc.)
            string mediaFolderName = folderPathParts[2];

            // Handle Anime/Show Folders
            if (rawMediaType.Equals(Prog.AnimeFolderName) || rawMediaType.Equals(Prog.ShowFolderName))
            {
                // Process anime/show folder using the showFolderRegex
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
            // Handle Movie Folders (TMDB)
            else if (rawMediaType.Equals(Prog.MovieFolderName))
            {
                // Process movie folder using the movieFolderRegex
                Match mediaFolderMatch = movieFolderRegex.Match(mediaFolderName);

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
                    Prog.PrintErrMsg($"Could not parse media folder: {mediaFolderName}");
                }
            }
            else
            {
                Prog.PrintErrMsg($"Unknown media type encountered: {rawMediaType}");
            }
        }

        private void ProcessFileName()
        {
            // Get file name without extension
            string filename = Path.GetFileNameWithoutExtension(RelPath);
            if (Prog.AnimeFolderName.Contains(Type))
            {
                // Extract anime info using the anime regex
                Match animeMatch = animeEpRegex.Match(filename);
                if (animeMatch.Success)
                {
                    ExtractAnimeInfo(animeMatch);
                }
                else
                {
                    Prog.PrintErrMsg($"Could not parse anime: {RelPath}");
                }
            }
            else if (Prog.ShowFolderName.Contains(Type))
            {
                // Extract show info using the show regex
                Match showMatch = showEpRegex.Match(filename);
                if (showMatch.Success)
                {
                    ExtractShowInfo(showMatch);
                }
                else
                {
                    Prog.PrintErrMsg($"Could not parse show: {RelPath}");
                }
            }
            else if (Prog.MovieFolderName.Contains(Type))
            {
                // Extract movie info using the movie regex
                Match movieMatch = movieRegex.Match(filename);
                if (movieMatch.Success)
                {
                    ExtractMovieInfo(movieMatch);
                }
                else
                {
                    Prog.PrintErrMsg($"Could not parse movie: {filename}");
                    throw new Exception("Fix parsing");
                }
            }
            else
            {
                Prog.PrintErrMsg($"Unexpected media type: {RelPath}");
            }
        }

        private void ExtractAnimeInfo(Match animeMatch)
        {
            CheckMismatch(animeMatch, "Title", Title);
            CheckMismatch(animeMatch, "ReleaseYear", ReleaseYear);
            CheckMismatch(animeMatch, "SeasonNum", SeasonNum);

            EpisodeNum = GetGroupValue(animeMatch, "EpisodeNum");
            AbsEpisodeNum = GetGroupValue(animeMatch, "AbsoluteEpisode");
            EpisodeTitle = GetGroupValue(animeMatch, "EpisodeTitle");
            CustomFormats = GetGroupValue(animeMatch, "CustomFormats");
            QualityTitle = GetGroupValue(animeMatch, "QualityTitle");
            VideoDynamicRange = GetGroupValue(animeMatch, "VideoDynamicRange");
            VideoBitDepth = GetGroupValue(animeMatch, "VideoBitDepth");
            VideoCodec = GetGroupValue(animeMatch, "VideoCodec");
            AudioCodec = GetGroupValue(animeMatch, "AudioCodec");
            AudioChannels = GetGroupValue(animeMatch, "AudioChannels");
            AudioLanguages = GetGroupValue(animeMatch, "AudioLanguages");
            ReleaseGroup = GetGroupValue(animeMatch, "ReleaseGroup");
        }

        private void ExtractShowInfo(Match showMatch)
        {
            CheckMismatch(showMatch, "Title", Title);
            CheckMismatch(showMatch, "ReleaseYear", ReleaseYear);
            CheckMismatch(showMatch, "SeasonNum", SeasonNum);

            EpisodeNum = GetGroupValue(showMatch, "EpisodeNum");
            EpisodeTitle = GetGroupValue(showMatch, "EpisodeTitle");
            CustomFormats = GetGroupValue(showMatch, "CustomFormats");
            QualityTitle = GetGroupValue(showMatch, "QualityTitle");
            VideoDynamicRange = GetGroupValue(showMatch, "VideoDynamicRange");
            VideoBitDepth = GetGroupValue(showMatch, "VideoBitDepth");
            VideoCodec = GetGroupValue(showMatch, "VideoCodec");
            AudioCodec = GetGroupValue(showMatch, "AudioCodec");
            AudioChannels = GetGroupValue(showMatch, "AudioChannels");
            AudioLanguages = GetGroupValue(showMatch, "AudioLanguages");
            ReleaseGroup = GetGroupValue(showMatch, "ReleaseGroup");
        }

        private void ExtractMovieInfo(Match movieMatch)
        {
            CheckMismatch(movieMatch, "Title", Title);
            CheckMismatch(movieMatch, "ReleaseYear", ReleaseYear);

            CustomFormats = GetGroupValue(movieMatch, "Edition");
            CustomFormats = GetGroupValue(movieMatch, "CustomFormats");
            QualityTitle = GetGroupValue(movieMatch, "QualityTitle");
            VideoDynamicRange = GetGroupValue(movieMatch, "VideoDynamicRange");
            ThreeDInfo = GetGroupValue(movieMatch, "ThreeD");
            VideoBitDepth = GetGroupValue(movieMatch, "VideoBitDepth");
            VideoCodec = GetGroupValue(movieMatch, "VideoCodec");
            AudioCodec = GetGroupValue(movieMatch, "AudioCodec");
            AudioChannels = GetGroupValue(movieMatch, "AudioChannels");
            AudioLanguages = GetGroupValue(movieMatch, "AudioLanguages");
            ReleaseGroup = GetGroupValue(movieMatch, "ReleaseGroup");
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
            // Get value from filename, sanitised
            string filenameVal = Reflector.SanitiseFilename(GetGroupValue(animeMatch, groupName));

            // Get expected value (from folder), sanitised
            expectedValue = Reflector.SanitiseFilename(expectedValue);

            // If the filename value doesn't match the folder value, print an error message
            if (!filenameVal.Equals(expectedValue))
            {
                // Print an error message
                Prog.PrintErrMsg($"Folder and episode '{groupName}' mismatch: {RelPath}");
            }
        }
    }
}
