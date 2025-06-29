using MediaManager.Code.Modules;
using System;
using System.Collections.Generic;

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
            //CheckPropertyForUnknowns(Parser.MediaFiles, f => f.CustomFormat, "custom format"); // 98% unknown
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.DatabaseLink, "database link");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.Extension, "extension");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.MediaFileName, "media file name");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.MediaFolderName, "media folder name");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.MirrorFilePath, "mirror folder path");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.QualityTitle, "quality title");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.RealFilePath, "real file path");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.RelativeFilePath, "relative file path");
            //CheckPropertyForUnknowns(Parser.MediaFiles, f => f.ReleaseGroup, "release group"); // Boondocks doesn't have, need to set as exception?
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.ReleaseYear, "release year");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.Title, "title");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.Type, "type");
            //CheckPropertyForUnknowns(Parser.MediaFiles, f => f.VideoCodec, "video codec"); // Genuine issues
            //CheckPropertyForUnknowns(Parser.MediaFiles, f => f.VideoDynamicRange, "video dynamic range"); 100% unknown

            // Check episode-specific string-type properties
            // Use Parser.EpisodeFiles!!!
            // TODO

            // Check anime-specific string-type properties
            // TODO
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
            foreach (var item in items)
            {
                try
                {
                    // Extract property value
                    string propValue = propertySelector(item);

                    // Check property value
                    if (propValue.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"  - '{item}' has an unknown {propertyName}!");
                    }
                }
                catch (Exception ex)
                {
                    MediaFile curItem = item as MediaFile;

                    string errMsg = $"Failed to extract property!";
                    errMsg += $"\n\nException message: \n{ex.Message}";
                    errMsg += $"\n\nMedia file info: \n{curItem.ToAllPropertiesString()}";
                    Prog.PrintErrMsg(errMsg);

                    System.Environment.Exit(1);
                }
            }
        }
    }
}