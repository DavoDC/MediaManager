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
    internal abstract class MediaFile
    {
        /// <summary>
        /// This regex represents the naming format that the "arr" programs use for media folders.
        /// - Sonarr names anime/show folders like so: {Series TitleYear} {tvdb-{TvdbId}}
        ///     - Example: "Clarkson's Farm (2021) {tvdb-378165}"
        /// - Radarr names movie folders like so: {Movie CleanTitle} ({Release Year}) {tmdb-{TmdbId}}
        ///     - Example: "Shrek the Third (2007) {tmdb-810}"
        /// </summary>
        private static readonly Regex folderRegex = new Regex(@"^(?<Title>.*?)\s\((?<Year>\d{4})\)\s\{(?<ID>(?:tvdb|tmdb)-\d+)\}$");

        /// <summary>
        /// This regex represents how concise quality titles are named (e.g. WEBRip-720p, Bluray-1080p etc.)
        /// </summary>
        private static readonly Regex conciseQualityTitleRegex = new Regex(@"\w+-\d{3,4}p", RegexOptions.Compiled);

        /// <summary>
        /// Properties initialised in base class
        /// </summary>

        // The path to the mirror file of the media file 
        public string MirrorFilePath { get; set; }

        // The media file's relative path within the library folder
        public string RelativeFilePath { get; set; }

        // The path to the real media file
        public string RealFilePath { get; set; }

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

        // The audio codec
        public string AudioCodec { get; set; }

        // The audio channels
        public string AudioChannels { get; set; }

        // The video codec
        public string VideoCodec { get; set; }

        // The release group
        public string ReleaseGroup { get; set; }

        /// <summary>
        /// Construct a media file
        /// </summary>
        /// <param name="mirrorFilePath">The mirror file path</param>
        public MediaFile(string mirrorFilePath)
        {
            // Save mirror file path
            MirrorFilePath = Reflector.FixLongPath(mirrorFilePath);

            // Initialise relative path to file
            int relStartPos = mirrorFilePath.LastIndexOf(Prog.MirrorFolderName);
            RelativeFilePath = mirrorFilePath.Remove(0, relStartPos + Prog.MirrorFolderName.Length);

            // Initialise XML document object
            XMLDoc = new XmlDocument();
            XMLRootElement = XMLDoc.CreateElement("Media");
            XMLDoc.AppendChild(XMLRootElement);

            // If the file needs to be converted to a valid XML file
            if (DoesFileNeedToBeConvertedToValidXML())
            {
                // Generate a valid XML file
                GenerateXMLFile();
            }
            else
            {
                // Else if the file is already valid XML, load it up
                LoadXMLFile();
            }

            // TESTING
            //PrintAllProperties();
        }

        /// <summary>
        /// Generate a valid XML file that stores the file's metadata
        /// </summary>
        public void GenerateXMLFile()
        {
            // Initialise all fields
            InitialiseSimpleCommonFields();
            InitialiseFieldsUsingMediaFolderName();
            InitialiseFieldsUsingMediaFileName();

            // Add common fields to the XML object
            SetElementValue("Filename", MediaFileName + Extension);
            SetElementValue("Type", Type);
            SetElementValue("Title", Title);
            SetElementValue("ReleaseYear", ReleaseYear);
            SetElementValue("DatabaseLink", DatabaseLink);
            SetElementValue("CustomFormat", CustomFormat);
            SetElementValue("QualityTitle", QualityTitle);
            SetElementValue("VideoDynamicRange", VideoDynamicRange);
            SetElementValue("AudioCodec", AudioCodec);
            SetElementValue("AudioChannels", AudioChannels);
            SetElementValue("VideoCodec", VideoCodec);
            
            // Add specific fields to the XML object
            AddSpecificFieldsToXMLDoc();

            // Add release group and  extension last
            SetElementValue("ReleaseGroup", ReleaseGroup);
            SetElementValue("Extension", Extension);

            // Save the XML object, overwriting the mirror file contents with valid XML content
            XMLDoc.Save(MirrorFilePath);
        }

        /// <summary>
        /// Read in metadata information from an XML file and use it to initialise this class's fields
        /// </summary>
        public void LoadXMLFile()
        {
            // Open the XML file using FileStream to avoid URI parsing issues for long paths
            using (FileStream fs = new FileStream(MirrorFilePath, FileMode.Open, FileAccess.Read))
            {
                using (XmlReader reader = XmlReader.Create(fs))
                {
                    XMLDoc.Load(reader);
                }
            }
            XMLRootElement = XMLDoc.DocumentElement;

            // Initialise common fields using XML data
            MediaFileName = GetElementValue("Filename");
            Title = GetElementValue("Title");
            Type = GetElementValue("Type");
            ReleaseYear = GetElementValue("ReleaseYear");
            DatabaseLink = GetElementValue("DatabaseLink");
            Extension = GetElementValue("Extension");
            CustomFormat = GetElementValue("CustomFormat");
            QualityTitle = GetElementValue("QualityTitle");
            VideoDynamicRange = GetElementValue("VideoDynamicRange");
            AudioCodec = GetElementValue("AudioCodec");
            AudioChannels = GetElementValue("AudioChannels");
            VideoCodec = GetElementValue("VideoCodec");
            ReleaseGroup = GetElementValue("ReleaseGroup");

            // Initialise specific fields using XML data
            InitialiseSpecificFieldsUsingXML();
        }

        /// <summary>
        /// Initialise simple common fields
        /// </summary>
        public void InitialiseSimpleCommonFields()
        {
            // Extract media extension from real file path
            Extension = Path.GetExtension(RealFilePath);

            // Get full relative path but without filename
            // e.g. "\Anime\A Certain Magical Index (2008) {tvdb-83322}\Season 01"
            // or "\Movies\8 Mile (2002) {tmdb-65}\"
            string folderPath = Path.GetDirectoryName(RelativeFilePath);

            // Split the relative path by backslashes to extract path parts
            string[] folderPathParts = folderPath.Split('\\');

            // Extract and save the media's type
            string rawMediaType = folderPathParts[1]; // e.g. Anime/Movies/Shows
            Type = rawMediaType.Replace("s", ""); // e.g. Anime/Movie/Show

            // Get media folder name
            // e.g. "A Certain Magical Index (2008) {tvdb-83322}", "8 Mile (2002) {tmdb-65}", etc.
            MediaFolderName = folderPathParts[2];

            // Get media file name without extension
            MediaFileName = Path.GetFileNameWithoutExtension(RelativeFilePath);

            // Try to apply media folder regex to media folder name (e.g. "8 Mile (2002) {tmdb-65}")
            Match mediaFolderMatch = folderRegex.Match(MediaFolderName);

            // If regex matched media folder name
            if (mediaFolderMatch.Success)
            {
                // Extract title and release year
                Title = mediaFolderMatch.Groups["Title"].Value;
                ReleaseYear = mediaFolderMatch.Groups["Year"].Value;

                // Break down database ID parts
                string[] databaseIDParts = mediaFolderMatch.Groups["ID"].Value.Split('-'); // e.g. split "tmdb-65"
                string databaseIDType = databaseIDParts[0]; // e.g. tmdb
                string databaseIDValue = databaseIDParts[1]; // e.g. 65

                // If database ID type matches expected type
                if (databaseIDType.Equals(GetExpectedDatabaseIDType()))
                {
                    // Save database link
                    DatabaseLink = GetDatabaseLink(databaseIDValue);
                }
                else
                {
                    // Else if unexpected database ID type found, print error
                    Prog.PrintErrMsg($"Unknown database ID type encountered: {databaseIDType}");
                }
            }
            else
            {
                // Else if the regex did not match the format of the media's folder name
                Prog.PrintErrMsg($"Could not parse media folder: {MediaFolderName}");
            }
        }

        /// <returns>A string representation of all file properties.</returns>
        public string ToAllPropertiesString()
        {
            // Add common properties to string
            string props = "";
            props += $"RelativeFilePath: {RelativeFilePath ?? "NULL"}\n";
            props += $"Type: {Type ?? "NULL"}\n";
            props += $"Title: {Title ?? "NULL"}\n";
            props += $"ReleaseYear: {ReleaseYear ?? "NULL"}\n";
            props += $"DatabaseLink: {DatabaseLink ?? "NULL"}\n";
            props += $"CustomFormat: {CustomFormat ?? "NULL"}\n";
            props += $"QualityTitle: {QualityTitle ?? "NULL"}\n";
            props += $"VideoDynamicRange: {VideoDynamicRange ?? "NULL"}\n";
            props += $"AudioCodec: {AudioCodec ?? "NULL"}\n";
            props += $"AudioChannels: {AudioChannels ?? "NULL"}\n";
            props += $"VideoCodec: {VideoCodec ?? "NULL"}\n";

            // Add specific properties to string
            props += GetSpecificPropString();

            // Add release group and extension last
            props += $"ReleaseGroup: {ReleaseGroup ?? "NULL"}\n";
            props += $"Extension: {Extension ?? "NULL"}\n";

            // Return all properties as a string 
            return props;
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
                Prog.PrintErrMsg($"Folder and episode '{groupName}' mismatch: {RelativeFilePath}");
            }
        }

        /// <summary>
        /// This function helps distinguish between:
        /// - Mirror files that have been freshly reflected, and only contain a single valid real file path, and no XML content.
        /// - Mirror files that were previously parsed, and contain valid XML content with the file's metadata.
        /// </summary>
        /// <returns>True if the file needs to be converted to a valid XML file</returns>
        public bool DoesFileNeedToBeConvertedToValidXML()
        {
            // Get file contents of mirror file
            string[] mirrorFileContents = File.ReadAllLines(MirrorFilePath);

            // Check whether mirror file contains one line
            bool mirrorFileContainsOneLine = mirrorFileContents.Length == 1;

            // Extract real file paths from mirror file contents
            string rawRealFilePath = mirrorFileContents[0];
            RealFilePath = Reflector.FixLongPath(rawRealFilePath);

            // Check if the real paths extracted are valid
            bool rawMirrorFilePathIsValid = File.Exists(rawRealFilePath);
            bool fixedMirrorFilePathIsValid = File.Exists(RealFilePath);
            bool mirrorFilePathIsValid = rawMirrorFilePathIsValid || fixedMirrorFilePathIsValid;
            // Note: Both versions need to be checked as one sometimes fails

            // If the mirror file contains a single, valid path, then it needs to be converted to a valid XML file
            return mirrorFileContainsOneLine && mirrorFilePathIsValid;
        }

        /// <summary>
        /// Sets the value of the specified XML element.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="elementValue">The value to set for the XML element.</param>
        public void SetElementValue(string elementName, string elementValue)
        {
            var existingElement = GetXmlElement(elementName);

            // If element exists, set its value
            if (existingElement != null)
            {
                existingElement.InnerText = elementValue;
                return;
            }

            // If element does not exist, create a new one and add it
            var newElement = XMLDoc.CreateElement(elementName);
            newElement.InnerText = elementValue;
            XMLRootElement.AppendChild(newElement);
        }

        /// <summary>
        /// Get a concise version of this file's quality title that doesn't include extra info.
        /// e.g. Exclude streaming service name, repack info etc.
        /// </summary>
        /// <returns>The simplified quality title</returns>
        public string GetConciseQualityTitle()
        {
            // Handle special DVD case
            if(QualityTitle.Equals("DVD"))
            {
                return QualityTitle;
            }

            // Try to apply concise quality title regex to full quality title
            Match conciseQualityTitleMatch = conciseQualityTitleRegex.Match(QualityTitle);

            // If regex matched part of quality title
            if (conciseQualityTitleMatch.Success)
            {
                // Return part that was found
                return conciseQualityTitleMatch.Value;
            }
            else
            {
                // Else if couldn't find match, print error and return full title
                string errMsg = $"Failed to parse Quality Title: {QualityTitle}";
                errMsg += $"\nMedia file info: \n{ToAllPropertiesString()}";
                Prog.PrintErrMsg(errMsg);
                return QualityTitle;
            }
        }
        /// <summary>
        /// Check media type (from path) against what is expected, and notify if not.
        /// </summary>
        /// <param name="expectedTypeS">The expected type as a string (e.g. movie, show, anime)</param>
        public void CheckType(string expectedTypeS)
        {
            // If this media's type does not match what is expected
            if (!Type.ToLower().Equals(expectedTypeS.ToLower()))
            {
                // Print error message
                Prog.PrintErrMsg($"Non-{expectedTypeS} mirror file path given to {expectedTypeS} file constructor: {MirrorFilePath}");
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

        /// <summary>
        /// Prints all properties of this file.
        /// </summary>
        public void PrintAllProperties()
        {
            Console.WriteLine(ToAllPropertiesString());
        }

        /// <returns>A concise string representing this media file</returns>
        public override string ToString()
        {
            return $"{Title} ({ReleaseYear})";
        }

        /// <summary>
        /// Get the expected database ID type for this media type (e.g. 'tmdb', 'tvdb' etc.)
        /// </summary>
        public abstract string GetExpectedDatabaseIDType();

        /// <summary>
        /// Generate a database link
        /// </summary>
        /// <param name="id">The media's ID in a certain media database</param>
        /// <returns>A string containing a link to the media's page in a media database</returns>
        public abstract string GetDatabaseLink(string id);

        /// <summary>
        /// Initialise fields by parsing the media's folder name
        /// </summary>
        public virtual void InitialiseFieldsUsingMediaFolderName()
        {
            // To be overridden by derived classes if needed
        }

        /// <summary>
        /// Initialise fields by parsing the media's file name
        /// </summary>
        public abstract void InitialiseFieldsUsingMediaFileName();

        /// <summary>
        /// Add fields specific to the media type to the XML document
        /// </summary>
        public abstract void AddSpecificFieldsToXMLDoc();

        /// <summary>
        /// Initialise fields specific to the media type using data from the XML document
        /// </summary>
        public abstract void InitialiseSpecificFieldsUsingXML();

        /// <summary>
        /// Get fields specific to the media type as a string
        /// </summary>
        public abstract string GetSpecificPropString();
    }
}