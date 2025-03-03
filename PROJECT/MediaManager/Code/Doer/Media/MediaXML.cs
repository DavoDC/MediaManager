using System;
using System.Xml;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// A media file's metadata stored as an XML file
    /// </summary>
    internal class MediaXML : MediaFile
    {
        // Private fields
        private XmlDocument xmlDoc;
        private XmlElement rootElement;

        /// <summary>
        /// Initialises a new instance of the <see cref="MediaXML"/> class.
        /// </summary>
        /// <param name="mirrorFilePath">The path to the mirror file.</param>
        /// <param name="tag">The file's metadata. Null if not given</param>
        public MediaXML(string mirrorFilePath, MediaTag tag = null)
        {
            try
            {
                // Initialise XML document
                xmlDoc = new XmlDocument();
                rootElement = xmlDoc.CreateElement("Media");
                xmlDoc.AppendChild(rootElement);

                // If tag provided
                if (tag != null)
                {
                    // CREATE AN XML FILE

                    // Set XML elements to metadata values for common properties
                    SetElementValue("Type", tag.Type);
                    SetElementValue("Title", tag.Title);
                    SetElementValue("ReleaseYear", tag.ReleaseYear);
                    SetElementValue("DatabaseLink", tag.DatabaseLink);
                    SetElementValue("Extension", tag.Extension);
                    SetElementValue("CustomFormats", tag.CustomFormats);
                    SetElementValue("QualityTitle", tag.QualityTitle);
                    SetElementValue("VideoDynamicRange", tag.VideoDynamicRange);
                    SetElementValue("VideoCodec", tag.VideoCodec);
                    SetElementValue("AudioCodec", tag.AudioCodec);
                    SetElementValue("AudioChannels", tag.AudioChannels);
                    SetElementValue("ReleaseGroup", tag.ReleaseGroup);

                    // Check if the media type is show or anime, and add those properties
                    if (tag.Type == "Show" || tag.Type == "Anime")
                    {
                        SetElementValue("SeasonType", tag.SeasonType);
                        SetElementValue("SeasonNum", tag.SeasonNum);
                        SetElementValue("EpisodeNum", tag.EpisodeNum);
                        SetElementValue("EpisodeTitle", tag.EpisodeTitle);
                    }

                    // Check if the media type is anime, and add anime-specific properties
                    if (tag.Type == "Anime")
                    {
                        SetElementValue("AbsEpisodeNum", tag.AbsEpisodeNum);
                        SetElementValue("VideoBitDepth", tag.VideoBitDepth);
                        SetElementValue("AudioLanguages", tag.AudioLanguages);
                    }

                    // Check if the media type is movie, and add movie-specific properties
                    if (tag.Type == "Movie")
                    {
                        SetElementValue("Edition", tag.Edition);
                        SetElementValue("ThreeDInfo", tag.ThreeDInfo);
                    }

                    // Save file
                    xmlDoc.Save(mirrorFilePath);
                }
                else
                {
                    // If no tag, LOAD EXISTING XML FILE
                    xmlDoc.Load(mirrorFilePath);
                    rootElement = xmlDoc.DocumentElement;

                    // Read data from XML and set properties
                    Title = GetElementValue("Title");
                    Type = GetElementValue("Type");
                    ReleaseYear = GetElementValue("ReleaseYear");
                    DatabaseLink = GetElementValue("DatabaseLink");
                    Extension = GetElementValue("Extension");
                    CustomFormats = GetElementValue("CustomFormats");
                    QualityTitle = GetElementValue("QualityTitle");
                    VideoDynamicRange = GetElementValue("VideoDynamicRange");
                    VideoCodec = GetElementValue("VideoCodec");
                    AudioCodec = GetElementValue("AudioCodec");
                    AudioChannels = GetElementValue("AudioChannels");
                    ReleaseGroup = GetElementValue("ReleaseGroup");

                    // Check if the media type is show or anime, and add those properties
                    if (Type.Equals("Show") || Type.Equals("Anime"))
                    {
                        SeasonType = GetElementValue("SeasonType");
                        SeasonNum = GetElementValue("SeasonNum");
                        EpisodeNum = GetElementValue("EpisodeNum");
                        EpisodeTitle = GetElementValue("EpisodeTitle");
                    }

                    // Check if the media type is anime, and add anime-specific properties
                    if (Type.Equals("Anime"))
                    {
                        AbsEpisodeNum = GetElementValue("AbsEpisodeNum");
                        VideoBitDepth = GetElementValue("VideoBitDepth");
                        AudioLanguages = GetElementValue("AudioLanguages");
                    }

                    // Check if the media type is movie, and add movie-specific properties
                    if (Type.Equals("Movie"))
                    {
                        Edition = GetElementValue("Edition");
                        ThreeDInfo = GetElementValue("ThreeDInfo");
                    }
                }
            }
            catch (Exception)
            {
                // Create error message and throw new exception with it
                string errMsg = "Error occurred while ";
                errMsg += $"{(tag != null ? "creating NEW" : "loading EXISTING")} XML file!";
                errMsg += $"\n'mirrorFilePath' was: {mirrorFilePath}";
                throw new XmlException(errMsg);
            }
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
            var newElement = xmlDoc.CreateElement(elementName);
            newElement.InnerText = elementValue;
            rootElement.AppendChild(newElement);
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

        //private string GetXmlValue(string value) => string.IsNullOrEmpty(value) ? "Unknown" : value;


        /// <summary>
        /// Gets the specified XML element from the root element.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <returns>The XML element, or null if not found.</returns>
        private XmlElement GetXmlElement(string elementName)
        {
            return rootElement.SelectSingleNode(elementName) as XmlElement;
        }
    }
}