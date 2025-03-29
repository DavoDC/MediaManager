using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using File = System.IO.File;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// A media file's metadata
    /// </summary>
    internal class MediaFile
    {
        /// <summary>
        /// Properties initialised in base class
        /// </summary>

        // The media file's relative path within the library folder
        public string RelPath { get; set; }

        /// <summary>
        /// Whether the file needs to be converted to valid XML.
        /// If true, then the file only contains a single, valid real file path.
        /// </summary>
        public bool ConvertToValidXML { get; set; }

        // The file extension of this media file
        public string Extension { get; set; }

        // The type of media this file represents (e.g. a movie, show or anime)
        public string Type { get; set; }

        // The name of the folder holding this media
        public string MediaFolderName { get; set; }

        // The filename of this media file (with no extension)
        public string MediaFileName { get; set; }

        // The XML document object for this media file
        public XmlDocument XMLDoc { get; set; }

        // The root element of the XML document object for this media file
        public XmlElement XMLRootElement { get; set; }

        /// <summary>
        /// Properties initialised in derived class based on folder name
        /// </summary>

        // The title of the media item
        public string Title { get; set; }

        // The year the media item was released
        public string ReleaseYear { get; set; }

        // A link to this media item in a media database
        public string DatabaseLink { get; set; }

        /// <summary>
        /// Properties initialised in derived class based on file name
        /// </summary>

        // The custom format of the file
        public string CustomFormat { get; set; }

        // The quality title
        public string QualityTitle { get; set; }

        // The video dynamic range
        public string VideoDynamicRange { get; set; }

        // The video codec
        public string VideoCodec { get; set; }

        // The audio codec
        public string AudioCodec { get; set; }

        // The audio channels
        public string AudioChannels { get; set; }

        // The release group
        public string ReleaseGroup { get; set; }

        /// <summary>
        /// Construct a media file
        /// </summary>
        /// <param name="mirrorFilePath">The mirror file path</param>
        public MediaFile(string mirrorFilePath)
        {
            // Initialise relative path to file
            int relStartPos = mirrorFilePath.LastIndexOf(Prog.MirrorFolderName);
            RelPath = mirrorFilePath.Remove(0, relStartPos + Prog.MirrorFolderName.Length);

            // Initialise XML document object
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement rootElement = xmlDoc.CreateElement("Media");
            xmlDoc.AppendChild(rootElement);

            // Get file contents of mirror file
            string[] fileContents = File.ReadAllLines(mirrorFilePath);
            bool mirrorFileContainsOnePath = fileContents.Length == 1;

            // Check validity of real file path
            string realFilePath = Reflector.FixLongPath(fileContents[0]);
            bool mirrorFilePathIsValid = File.Exists(realFilePath);

            // If the mirror file contains a single, valid path
            if (mirrorFileContainsOnePath && mirrorFilePathIsValid)
            {
                // Then the file needs to be converted to a valid XML file
                ConvertToValidXML = true;

                // Extract media extension from real file path
                Extension = Path.GetExtension(realFilePath);

                // Get full relative path but without filename
                // e.g. "\Anime\A Certain Magical Index (2008) {tvdb-83322}\Season 01"
                // or "\Movies\8 Mile (2002) {tmdb-65}\"
                string folderPath = Path.GetDirectoryName(RelPath);

                // Split the relative path by backslashes to extract path parts
                string[] folderPathParts = folderPath.Split('\\');

                // Extract and save the media's type
                string rawMediaType = folderPathParts[1]; // e.g. Anime/Movies/Shows
                Type = rawMediaType.Replace("s", "");

                // Get media folder name
                // e.g. "A Certain Magical Index (2008) {tvdb-83322}", "8 Mile (2002) {tmdb-65}", etc.
                MediaFolderName = folderPathParts[2];

                // Get media file name without extension
                MediaFileName = Path.GetFileNameWithoutExtension(RelPath);


                // Overwrite mirror file contents with XML content
                //MediaXML xmlFileOut = new MediaXML(mirrorFilePath, this);
                // SOME XML ACTIONS SHOULD BE HERE - common

                // Set XML elements to metadata values for common properties
                //SetElementValue("Type", tag.Type);
                //SetElementValue("Title", tag.Title);
                //SetElementValue("ReleaseYear", tag.ReleaseYear);
                //SetElementValue("DatabaseLink", tag.DatabaseLink);
                //SetElementValue("Extension", tag.Extension);
                //SetElementValue("CustomFormats", tag.CustomFormats);
                //SetElementValue("QualityTitle", tag.QualityTitle);
                //SetElementValue("VideoDynamicRange", tag.VideoDynamicRange);
                //SetElementValue("VideoCodec", tag.VideoCodec);
                //SetElementValue("AudioCodec", tag.AudioCodec);
                //SetElementValue("AudioChannels", tag.AudioChannels);
                //SetElementValue("ReleaseGroup", tag.ReleaseGroup);

                //                    // If the media type is show or anime, add show/anime-specific properties
                //                    if (tag.Type == "Show" || tag.Type == "Anime")
                //                    {
                //                        SetElementValue("SeasonType", tag.SeasonType);
                //                        SetElementValue("SeasonNum", tag.SeasonNum);
                //                        SetElementValue("EpisodeNum", tag.EpisodeNum);
                //                        SetElementValue("EpisodeTitle", tag.EpisodeTitle);
                //                    }

                //                    // If the media type is anime, add anime-specific properties
                //                    if (tag.Type == "Anime")
                //                    {
                //                        SetElementValue("AbsEpisodeNum", tag.AbsEpisodeNum);
                //                        SetElementValue("VideoBitDepth", tag.VideoBitDepth);
                //                        SetElementValue("AudioLanguages", tag.AudioLanguages);
                //                    }

                //                    // If the media type is movie, add movie-specific properties
                //                    if (tag.Type == "Movie")
                //                    {
                //                        SetElementValue("Edition", tag.Edition);
                //                        SetElementValue("ThreeDInfo", tag.ThreeDInfo);
                //                    }

                //                    // Save file
                //                    xmlDoc.Save(mirrorFilePath);
            }
            else
            {
                // TODO In children
                //                // Else if the mirror file is a valid XML file
                //                // Read in XML data
                //                MediaXML xmlFileIn = new MediaXML(mirrorFilePath);
                //                // Set tag properties using XML file data
                //                Title = xmlFileIn.GetElementValue("Title");
                // SOME XML ACTIONS SHOULD BE HERE - common

                //                    // If no tag, LOAD EXISTING XML FILE

                //                    // Open the file using FileStream to avoid URI parsing issues for long paths
                //                    using (FileStream fs = new FileStream(mirrorFilePath, FileMode.Open, FileAccess.Read))
                //                    {
                //                        using (XmlReader reader = XmlReader.Create(fs))
                //                        {
                //                            xmlDoc.Load(reader);
                //                        }
                //                    }
                //                    rootElement = xmlDoc.DocumentElement;

                //                    // Read data from XML and set common properties
                //                    Title = GetElementValue("Title");
                //                    Type = GetElementValue("Type");
                //                    ReleaseYear = GetElementValue("ReleaseYear");
                //                    DatabaseLink = GetElementValue("DatabaseLink");
                //                    Extension = GetElementValue("Extension");
                //                    CustomFormats = GetElementValue("CustomFormats");
                //                    QualityTitle = GetElementValue("QualityTitle");
                //                    VideoDynamicRange = GetElementValue("VideoDynamicRange");
                //                    VideoCodec = GetElementValue("VideoCodec");
                //                    AudioCodec = GetElementValue("AudioCodec");
                //                    AudioChannels = GetElementValue("AudioChannels");
                //                    ReleaseGroup = GetElementValue("ReleaseGroup");

                //                    // If the media type is show or anime, retrieve show/anime-specific properties
                //                    if (Type.Equals("Show") || Type.Equals("Anime"))
                //                    {
                //                        SeasonType = GetElementValue("SeasonType");
                //                        SeasonNum = GetElementValue("SeasonNum");
                //                        EpisodeNum = GetElementValue("EpisodeNum");
                //                        EpisodeTitle = GetElementValue("EpisodeTitle");
                //                    }

                //                    // If the media type is anime, retrieve anime-specific properties
                //                    if (Type.Equals("Anime"))
                //                    {
                //                        AbsEpisodeNum = GetElementValue("AbsEpisodeNum");
                //                        VideoBitDepth = GetElementValue("VideoBitDepth");
                //                        AudioLanguages = GetElementValue("AudioLanguages");
                //                    }

                //                    // If the media type is movie, retrieve movie-specific properties
                //                    if (Type.Equals("Movie"))
                //                    {
                //                        Edition = GetElementValue("Edition");
                //                        ThreeDInfo = GetElementValue("ThreeDInfo");
                //                    }
                //                }
            }


        }

        /// <summary>
        /// Compares a specific group value from the filename match with the corresponding expected value 
        /// and prints an error message if they don't match.
        /// </summary>
        /// <param name="animeMatch">The match object containing the filename data.</param>
        /// <param name="groupName">The name of the group to retrieve from the match.</param>
        /// <param name="expectedValue">The expected value to compare against.</param>
        public void CheckMismatch(Match animeMatch, string groupName, string expectedValue)
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

        /// <summary>
        /// Retrieves the value of a specified group from a regex match, or returns a default value if the group is empty or null.
        /// </summary>
        /// <param name="match">The regex match object containing the groups to extract data from.</param>
        /// <param name="groupName">The name of the group whose value is to be retrieved.</param>
        /// <param name="defaultValue">The default value to return if the group value is empty or null (default is "Unknown").</param>
        /// <returns>The value of the specified group, or the default value if the group is empty or null.</returns>
        public static string GetGroupValue(Match match, string groupName, string defaultValue = "Unknown")
        {
            return string.IsNullOrEmpty(match.Groups[groupName].Value) ? defaultValue : match.Groups[groupName].Value;
        }

        /// <summary>
        /// Sets the value of the specified XML element.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="elementValue">The value to set for the XML element.</param>
        private void SetElementValue(string elementName, string elementValue)
        {
            var existingElement = GetXmlElement(elementName);

            // If element exists, set its value
            if (existingElement != null)
            {
                existingElement.InnerText = elementValue;
                return;
            }

            // If element does not exist, create a new one
            var newElement = XMLDoc.CreateElement(elementName);
            newElement.InnerText = elementValue;
            XMLRootElement.AppendChild(newElement);
        }


        /// <summary>
        /// Gets the value of the specified XML element.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <returns>The value of the XML element, or an empty string if the element is not found.</returns>
        public string GetElementValue(string elementName)
        {
            var existingElement = GetXmlElement(elementName);
            return existingElement?.InnerText ?? string.Empty;
        }

        /// <summary>
        /// Gets the specified XML element from the root element.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <returns>The XML element, or null if not found.</returns>
        private XmlElement GetXmlElement(string elementName)
        {
            return XMLRootElement.SelectSingleNode(elementName) as XmlElement;
        }

        /// <returns>A string representation of all file properties.</returns>
        public string ToAllPropertiesString()
        {
            return $"RelPath: {RelPath ?? "NULL"}\n" +
                   $"Type: {Type ?? "NULL"}\n" +
                   $"Title: {Title ?? "NULL"}\n" +
                   $"ReleaseYear: {ReleaseYear ?? "NULL"}\n" +
                   $"DatabaseLink: {DatabaseLink ?? "NULL"}\n" +
                   $"Extension: {Extension ?? "NULL"}\n" +
                   $"CustomFormats: {CustomFormat ?? "NULL"}\n" +
                   $"QualityTitle: {QualityTitle ?? "NULL"}\n" +
                   $"VideoDynamicRange: {VideoDynamicRange ?? "NULL"}\n" +
                   $"VideoCodec: {VideoCodec ?? "NULL"}\n" +
                   $"AudioCodec: {AudioCodec ?? "NULL"}\n" +
                   $"AudioChannels: {AudioChannels ?? "NULL"}\n" +
                   $"ReleaseGroup: {ReleaseGroup ?? "NULL"}";
        }

        /// <summary>
        /// Prints all properties of this file.
        /// </summary>
        public void PrintAllProperties()
        {
            Console.WriteLine(ToAllPropertiesString());
        }
    }
}