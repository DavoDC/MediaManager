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

            // Check properties against filename
            CheckPropertiesAgainstFilename();

            // Finish and print time taken
            FinishAndPrintTimeTaken();
        }

        /// <summary>
        /// Check for properties containing "Unknown".
        /// </summary>
        private static void CheckPropertiesForUnknowns()
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
            //CheckPropertyForUnknowns(Parser.MovieFiles, f => f.Edition, "edition");    // Disabled
            //CheckPropertyForUnknowns(Parser.MovieFiles, f => f.ThreeDInfo, "3D info"); // Disabled

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
        private static void CheckPropertyForUnknowns<T>(IEnumerable<T> items, Func<T, string> propExtractor,
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

            // Notify if any issues found
            if (issuesFound != 0)
            {
                Console.WriteLine(summaryMsg);
            }
        }

        /// <summary>
        /// Check that all properties are contained in filename
        /// </summary>
        private static void CheckPropertiesAgainstFilename()
        {
            // Notify
            Console.WriteLine($" - Checking for properties against filename...");

            // For every media file 
            foreach (MediaFile curMediaFile in Parser.MediaFiles)
            {
                // Get filename
                string filename = curMediaFile.MediaFileName;

                // Remove common properties
                filename = RemoveCommonProperties(filename, curMediaFile);

                // Remove media-type-specific properties
                if(curMediaFile.Type.Equals("Movie"))
                {
                    // Remove movie-specific properties
                    MovieFile movieFile = curMediaFile as MovieFile;
                    filename = RemoveBracedStr(filename, $"edition-{movieFile.Edition}");
                }
                else
                {
                    // Remove episode-specific properties
                    EpisodeFile epFile = curMediaFile as EpisodeFile;

                    // Remove season-ep code
                    filename = RemoveStr(filename, $"- {epFile.GetSeasonEpStr()}");

                    // Remove episode title
                    filename = RemoveStr(filename, $"- {epFile.EpisodeTitle}");

                    // Remove anime specific properties
                    if(curMediaFile.Type.Equals("Anime"))
                    {
                        AnimeFile animeFile = curMediaFile as AnimeFile;

                        // Remove absolute episode number
                        filename = RemoveStr(filename, $"- {animeFile.AbsEpisodeNum}");

                        // Remove video bit depth
                        filename = RemoveBracketedStr(filename, $"{animeFile.VideoBitDepth}bit");

                        // Remove audio language(s)
                        filename = RemoveBracketedStr(filename, animeFile.AudioLanguages);
                    }
                }

                // At this stage, the filename should have nothing left
                // If the filename still has something included, notify
                if (filename.Length != 0)
                {
                    Console.WriteLine($"  - '{curMediaFile.MediaFileName}' still had '{filename}' left!");
                }
            }
        }

        /// <summary>
        /// Removes the common media file properties from a given filename
        /// </summary>
        /// <param name="filename">The media filename</param>
        /// <returns></returns>
        private static string RemoveCommonProperties(string filename, MediaFile file)
        {
            // Remove title and year
            string titleAndYear = $"{file.Title} ({file.ReleaseYear})";
            filename = RemoveStr(filename, Reflector.SanitiseFilename(titleAndYear));

            // Remove database link
            filename = RemoveBracedStr(filename, file.DatabaseRef);

            // Remove custom format
            filename = RemoveBracketedStr(filename, file.CustomFormat);

            // Remove quality title
            filename = RemoveBracketedStr(filename, file.QualityTitle);

            // Remove audio codec/channel part
            filename = RemoveBracketedStr(filename, $"{file.AudioCodec} {file.AudioChannels}");

            // Remove video codec
            filename = RemoveBracketedStr(filename, file.VideoCodec);

            // Remove release group
            filename = RemoveStr(filename, $"-{file.ReleaseGroup}");

            // Remove extension
            filename = RemoveStr(filename, file.Extension);

            // Return new string
            return filename;
        }

        /// <summary>
        /// Removes the specified string enclosed in curly braces (e.g., {value}) from the original string.
        /// </summary>
        /// <param name="origStr">The original string.</param>
        /// <param name="strToRemove">The inner string to remove (excluding braces).</param>
        /// <returns>A new string with the specified braced substring removed.</returns>
        private static string RemoveBracedStr(string origStr, string strToRemove)
        {
            return RemoveDelimitedStr(origStr, strToRemove, "{", "}");
        }

        /// <summary>
        /// Removes the specified string enclosed in square brackets (e.g., [value]) from the original string.
        /// </summary>
        /// <param name="origStr">The original string.</param>
        /// <param name="strToRemove">The inner string to remove (excluding brackets).</param>
        /// <returns>A new string with the specified bracketed substring removed.</returns>
        private static string RemoveBracketedStr(string origStr, string strToRemove)
        {
            return RemoveDelimitedStr(origStr, strToRemove, "[", "]");
        }

        /// <summary>
        /// Removes the specified string enclosed between the given delimiters from the original string.
        /// </summary>
        /// <param name="origStr">The original string.</param>
        /// <param name="strToRemove">The inner string to remove (excluding delimiters).</param>
        /// <param name="startDelim">The starting delimiter (e.g., "{" or "[").</param>
        /// <param name="endDelim">The ending delimiter (e.g., "}" or "]").</param>
        /// <returns>A new string with the specified delimited substring removed.</returns>
        private static string RemoveDelimitedStr(string origStr, string strToRemove, string startDelim, string endDelim)
        {
            return RemoveStr(origStr, startDelim + strToRemove + endDelim);
        }

        /// <summary>
        /// Removes the specified substring from the original string and trims the result.
        /// </summary>
        /// <param name="origStr">The original string.</param>
        /// <param name="strToRemove">The exact substring to remove.</param>
        /// <returns>A new string with the specified substring removed and trimmed.</returns>
        private static string RemoveStr(string origStr, string strToRemove)
        {
            return origStr.Replace(strToRemove, "").Trim();
        }
    }
}