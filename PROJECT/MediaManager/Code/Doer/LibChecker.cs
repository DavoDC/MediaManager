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
            //CheckPropertyForUnknowns(Parser.MediaFiles, f => f.CustomFormat, "custom format"); // Disabled
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.DatabaseLink, "database link");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.Extension, "extension");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.MediaFileName, "media file name");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.MediaFolderName, "media folder name");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.MirrorFilePath, "mirror folder path");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.QualityTitle, "quality title");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.RelativeFilePath, "relative file path");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.ReleaseGroup, "release group", 55, "Boondocks");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.ReleaseYear, "release year");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.Title, "title");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.Type, "type");
            CheckPropertyForUnknowns(Parser.MediaFiles, f => f.VideoCodec, "video codec");
            //CheckPropertyForUnknowns(Parser.MediaFiles, f => f.VideoDynamicRange, "video dynamic range"); // Disabled

            // Check movie-specific string-type properties
            // TODO

            // Check episode-specific string-type properties
            CheckPropertyForUnknowns(Parser.EpisodeFiles, f => f.SeasonType, "season type");
            CheckPropertyForUnknowns(Parser.EpisodeFiles, f => f.SeasonNum, "season number");
            CheckPropertyForUnknowns(Parser.EpisodeFiles, f => f.EpisodeNum, "episode number");
            CheckPropertyForUnknowns(Parser.EpisodeFiles, f => f.EpisodeTitle, "episode title");

            // Check anime-specific string-type properties
            CheckPropertyForUnknowns(Parser.AnimeFiles, f => f.AbsEpisodeNum, "absolute episode number", 5, "Endymion+4 MHA");
            CheckPropertyForUnknowns(Parser.AnimeFiles, f => f.VideoBitDepth, "video bit depth", 5, "Endymion+4 MHA");
            CheckPropertyForUnknowns(Parser.AnimeFiles, f => f.AudioLanguages, "audio language", 341, "File Issue");
            // More info: This is due to the files not having the language encoded in the metadata.
            // Sonarr reads this info from the file and applies to the filenames.
            // Even if you manually add the languages, Sonarr will remove it on next rename, as it cannot detect it.
            // It cannot detect it even if you select the language within Sonarr.
            // This is not an issue with Sonarrr or this program, but an issue with the files themselves.

        }

        /// <summary>
        /// Finds items where the specified property has the value "Unknown" (case-insensitive).
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="items">The collection to check.</param>
        /// <param name="propExtractor">Function to extract the property value.</param>
        /// <param name="propName">The name of the property.</param>
        /// <param name="expIssues">The number of issues expected (Optional)</param>
        /// <param name="expDesc">A short description explaining the issues (Optional)</param>
        public static void CheckPropertyForUnknowns<T>(IEnumerable<T> items, Func<T, string> propExtractor,
            string propName, int expIssues = 0, string expDesc = null)
        {
            // Issues found
            int issuesFound = 0;

            // For every item in list provided
            foreach (var item in items)
            {
                try
                {
                    // Extract property value
                    string propValue = propExtractor(item);

                    // Check property value
                    bool isUnknown = propValue.Equals("Unknown", StringComparison.OrdinalIgnoreCase);
                    bool isEmpty = propValue.Length == 0;
                    if (isUnknown || isEmpty)
                    {
                        // Print out certain instances to investigate further
                        if (propName.Equals("audio language"))
                        {
                            //Console.WriteLine($"  - '{item}' has an unknown {propertyName}!");
                        }

                        issuesFound++;
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

            // Subtract expected issues from those found
            issuesFound = issuesFound - expIssues;

            // Base summary message
            string summaryMsg = $"  - {issuesFound} items had an unknown {propName}!";

            // Add expected issues part if needed
            if(expIssues != 0)
            {
                summaryMsg += $" ({expIssues} exceptions";

                if(expDesc != null)
                {
                    summaryMsg += $"; {expDesc}";
                }

                summaryMsg += ")";
            }

            // Notify
            Console.WriteLine(summaryMsg);
        }
    }
}