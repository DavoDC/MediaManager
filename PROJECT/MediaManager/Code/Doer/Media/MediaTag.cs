using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// An media file as a "tag" containing metadata.
    /// </summary>
    internal class MediaTag : MediaFile
    {
        // Regex for anime/show folders (e.g. "A Certain Magical Index (2008) {tvdb-83322}")
        Regex showFolderRegex = new Regex(@"^(?<Title>.*?)\s\((?<Year>\d{4})\)\s\{(?<TVDBID>tvdb-\d+)\}$");

        // Regex for anime episodes (e.g. "A Certain Magical Index (2008) - S01E01 - 001 - Academy City [HDTV-720p][8bit][x264][AAC 2.0][JA]")
        Regex animeEpRegex = new Regex(@"
                ^(?<Title>.+?)\s*-\s*
                S(?<Season>\d{2})E(?<Episode>\d{2})\s*-\s*
                (?<AbsoluteEpisode>\d{3})\s*-\s*
                (?<EpisodeTitle>.+?)\s*
                (?:\[(?<CustomFormats>[^\]]+)\])?\s*
                (?:\[(?<QualityTitle>[^\]]+)\])?\s*
                (?:\[(?<VideoDynamicRange>[^\]]+)\])?\s*
                (?:\[(?<VideoBitDepth>\d+)bit\])?\s*
                (?:\[(?<VideoCodec>[^\]]+)\])?\s*
                (?:\[(?<AudioCodec>[^\] ]+)\s*(?<AudioChannels>[\d.]+)\])?\s*
                (?:\[(?<AudioLanguages>[^\]]+)\])?\s*
                (?:-(?<ReleaseGroup>[^\]]+))?$
            ", RegexOptions.IgnorePatternWhitespace);

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

                //Console.WriteLine("Fin\n");

                //throw new NotImplementedException();

                // Overwrite mirror file contents with XML content
                //MediaXML xmlFileOut = new MediaXML(mirrorFilePath, this);
            }
            else
            {
                // Else if the mirror file is a valid XML file

                // Read in XML data
                // MediaXML xmlFileIn = new MediaXML(mirrorFilePath);

                //    // Set properties using XML file data
                //    //Title = xmlFileIn.Title;
            }
        }

        private void ProcessFolderPath()
        {
            // Get full relative path but without filename
            string folderPath = Path.GetDirectoryName(RelPath);  // e.g. "C:\Anime\A Certain Magical Index(2008) { tvdb - 83322}\Season 01"

            // Split the relative path by backslashes to extract path parts
            string[] folderPathParts = folderPath.Split('\\');

            // Extract and save the media's type
            Type = folderPathParts[1].Replace("s", "");  // e.g. Anime/Movies/Shows

            // Get media folder name parts
            string mediaFolderName = folderPathParts[2];  // e.g. A folder name (e.g. "A Certain Magical Index (2008) {tvdb-83322}", "8 Mile (2002) {tmdb-65}", etc.)

            // Match the input string
            Match mediaFolderMatch = showFolderRegex.Match(mediaFolderName);

            if (mediaFolderMatch.Success)
            {
                // Extract each part
                string title = mediaFolderMatch.Groups["Title"].Value;
                string year = mediaFolderMatch.Groups["Year"].Value;
                string tvdbID = mediaFolderMatch.Groups["TVDBID"].Value;

                // Get ID parts 
                string[] idParts = tvdbID.Split('-');

                // Output the parts
                //Console.WriteLine($"Title: {title}");
                //Console.WriteLine($"Year: {year}");
                //Console.WriteLine($"TVDB ID: https://thetvdb.com/search?query={idParts[1]}");
            }
            else
            {
                Console.WriteLine("No match found.");
            }


            // Get season folder or movie filename
            string part2 = folderPathParts[3];  // e.g. A season folder (e.g. "Season 01") or movie file (e.g "8 Mile (2002) {tmdb-65} [Bluray-720p][EAC3 5.1][x264]-playHD")
            //Console.WriteLine("Season: " + part2);
        }

        private void ProcessFileName()
        {
            // Process filename
            string filename = Path.GetFileNameWithoutExtension(RelPath);

            Match match = animeEpRegex.Match(filename);

            if (match.Success)
            {
                // Extract filename parts and assign them to variables with "Unknown" for missing data
                string title = string.IsNullOrEmpty(match.Groups["Title"].Value) ? "Unknown" : match.Groups["Title"].Value;
                string season = string.IsNullOrEmpty(match.Groups["Season"].Value) ? "Unknown" : match.Groups["Season"].Value;
                string episode = string.IsNullOrEmpty(match.Groups["Episode"].Value) ? "Unknown" : match.Groups["Episode"].Value;
                string absoluteEpisode = string.IsNullOrEmpty(match.Groups["AbsoluteEpisode"].Value) ? "Unknown" : match.Groups["AbsoluteEpisode"].Value;
                string episodeTitle = string.IsNullOrEmpty(match.Groups["EpisodeTitle"].Value) ? "Unknown" : match.Groups["EpisodeTitle"].Value;
                string customFormats = string.IsNullOrEmpty(match.Groups["CustomFormats"].Value) ? "Unknown" : match.Groups["CustomFormats"].Value;
                string qualityTitle = string.IsNullOrEmpty(match.Groups["QualityTitle"].Value) ? "Unknown" : match.Groups["QualityTitle"].Value;
                string videoDynamicRange = string.IsNullOrEmpty(match.Groups["VideoDynamicRange"].Value) ? "Unknown" : match.Groups["VideoDynamicRange"].Value;
                string videoBitDepth = string.IsNullOrEmpty(match.Groups["VideoBitDepth"].Value) ? "Unknown" : match.Groups["VideoBitDepth"].Value; // Note: Almost always Unknown
                string videoCodec = string.IsNullOrEmpty(match.Groups["VideoCodec"].Value) ? "Unknown" : match.Groups["VideoCodec"].Value;
                string audioCodec = string.IsNullOrEmpty(match.Groups["AudioCodec"].Value) ? "Unknown" : match.Groups["AudioCodec"].Value; // Note: Almost always Unknown
                string audioChannels = string.IsNullOrEmpty(match.Groups["AudioChannels"].Value) ? "Unknown" : match.Groups["AudioChannels"].Value; // Note: Almost always Unknown
                string audioLanguages = string.IsNullOrEmpty(match.Groups["AudioLanguages"].Value) ? "Unknown" : match.Groups["AudioLanguages"].Value;
                string releaseGroup = string.IsNullOrEmpty(match.Groups["ReleaseGroup"].Value) ? "Unknown" : match.Groups["ReleaseGroup"].Value;

                // Output the filename variables
                //Console.WriteLine("Filename: " + filename);
                //Console.WriteLine($"- Series Title: {title}");
                //Console.WriteLine($"- Season: {season}");
                //Console.WriteLine($"- Episode: {episode}");
                //Console.WriteLine($"- Absolute Episode: {absoluteEpisode}");
                //Console.WriteLine($"- Episode Title: {episodeTitle}");
                //Console.WriteLine($"- Custom Formats: {customFormats}");
                //Console.WriteLine($"- Quality Title: {qualityTitle}");
                //Console.WriteLine($"- Video Dynamic Range: {videoDynamicRange}");
                //Console.WriteLine($"- Video Bit Depth: {videoBitDepth}"); 
                //Console.WriteLine($"- Video Codec: {videoCodec}");
                //Console.WriteLine($"- Audio Codec: {audioCodec}");
                //Console.WriteLine($"- Audio Channels: {audioChannels}");
                //Console.WriteLine($"- Audio Languages: {audioLanguages}");
                //Console.WriteLine($"- Release Group: {releaseGroup}");
                //Console.WriteLine("");
            }
            else
            {
                Console.WriteLine("No match found.");
                Console.WriteLine("Relpath: " + RelPath);
                Console.WriteLine("Filename: " + filename);
                Console.WriteLine("");
            }


            // Extract extension from real file path
            //string extension = Path.GetExtension(realFilePath);
        }
    }
}
