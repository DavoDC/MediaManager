using MediaManager.Code.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaManager
{
    /// <summary>
    /// Checks my media/mirror library against various conditions
    /// </summary>
    internal class LibChecker : Doer
    {
        /// <summary>
        /// Create library checker
        /// </summary>
        public LibChecker()
        {
            // Notify
            Console.WriteLine($"\nChecking mirror...");

            // Check for properties containing "Unknown"
            CheckPropertiesForUnknowns();

            // Finish and print time taken
            FinishAndPrintTimeTaken();
        }

        /// <summary>
        /// Check for properties containing "Unknown".
        /// </summary>
        public static void CheckPropertiesForUnknowns()
        {
            // Notify
            Console.WriteLine($" - Checking for 'Unknown' properties...");

            // Check common (MediaFile) string-type properties
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.AudioChannels, "audio channel");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.AudioCodec, "audio codec");
            //CheckPropertyForUnknowns(Parser.MediaFiles, f => f.CustomFormat, "custom format"); // Disabled
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.DatabaseLink, "database link");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.Extension, "extension");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.MediaFileName, "media file name");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.MediaFolderName, "media folder name");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.MirrorFilePath, "mirror folder path");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.QualityTitle, "quality title");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.RelativeFilePath, "relative file path");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.ReleaseGroup, "release group");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.ReleaseYear, "release year");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.Title, "title");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.Type, "type");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.VideoCodec, "video codec");
            //CheckPropertyForUnknowns(Parser.MediaFiles, f => f.VideoDynamicRange, "video dynamic range"); // Disabled

            // Check episode-specific string-type properties
            CheckPropertyForUnknowns(Parser.EpisodeFiles, f => f.SeasonType, "season type");
            CheckPropertyForUnknowns(Parser.EpisodeFiles, f => f.SeasonNum, "season number");
            CheckPropertyForUnknowns(Parser.EpisodeFiles, f => f.EpisodeNum, "episode number");
            CheckPropertyForUnknowns(Parser.EpisodeFiles, f => f.EpisodeTitle, "episode title");

            // Check anime-specific string-type properties
            CheckPropertyForUnknowns(Parser.AnimeFiles, f => f.AbsEpisodeNum, "absolute episode number");
            CheckPropertyForUnknowns(Parser.AnimeFiles, f => f.VideoBitDepth, "video bit depth");
            CheckPropertyForUnknowns(Parser.AnimeFiles, f => f.AudioLanguages, "audio language");
        }

        /// <summary>
        /// Finds items where the specified property has the value "Unknown" (case-insensitive).
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="items">The collection to check.</param>
        /// <param name="propertySelector">Function to extract the property value.</param>
        /// <param name="propertyName">The name of the property.</param>
        public static void CheckPropertyForUnknowns<T>(IEnumerable<T> items, Func<T, string> propertySelector,
            string propertyName)
        {
            // Issues found
            int totalHits = 0;

            // For every item in list provided
            foreach (var item in items)
            {
                try
                {
                    // Extract property value
                    string propValue = propertySelector(item);

                    // Check property value
                    bool isUnknown = propValue.Equals("Unknown", StringComparison.OrdinalIgnoreCase);
                    bool isEmpty = propValue.Length == 0;
                    if (isUnknown || isEmpty)
                    {
                        //Console.WriteLine($"  - '{item}' has an unknown {propertyName}!");
                        totalHits++;
                    }
                }
                catch (Exception ex)
                {
                    MediaFile curMediaFile = item as MediaFile;
                    string errMsg = $"Failed to extract property!";
                    errMsg += $"\n\nException message: \n{ex.Message}";
                    errMsg += $"\n\nMedia file info: \n{curMediaFile.ToAllPropertiesString()}";
                    Prog.PrintErrMsg(errMsg);
                    throw;
                }
            }

            // Notify
            Console.WriteLine($"  - {totalHits} items had an unknown {propertyName}!");

            // Special case
            if(propertyName.Equals("release group"))
            {
                Console.WriteLine($"   - The 55 Boondocks episodes without a release group are expected here.");
            }
        }
    }
}