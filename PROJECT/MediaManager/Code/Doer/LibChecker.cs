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
                            //Console.WriteLine($"  - '{item}' has an unknown {propName}!");
                        }

                        issuesFound++;
                    }
                }
                catch (Exception ex)
                {
                    MediaFile curMediaFile = item as MediaFile;
                    string errMsg = $"Failed to extract property from '{curMediaFile.MediaFileName}'";
                    errMsg += $" due to exception ({ex.Message})!";
                    Prog.PrintErrMsg(errMsg);
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
            Console.WriteLine($" - Checking property values against filename...");

            // For every media file 
            foreach (MediaFile curFile in Parser.MediaFiles)
            {
                // Get copy of filename to remove parts from
                string nameCopy = curFile.MediaFileName;

                // Remove title and year
                nameCopy = RemovePrefix(nameCopy, curFile.GetSanitisedTitleAndYear());

                // If file is an episode
                if(curFile.IsEpisode())
                {
                    // Remove episode code
                    nameCopy = RemovePrefix(nameCopy, $"- {((EpisodeFile)curFile).GetSeasonEpStr()}");

                    // If file is an anime, remove absolute episode number
                    if (curFile.IsAnime())
                    {
                        nameCopy = RemovePrefix(nameCopy, $"- {((AnimeFile)curFile).AbsEpisodeNum}");
                    }

                    // Remove episode title
                    nameCopy = RemovePrefix(nameCopy, $"- {((EpisodeFile)curFile).EpisodeTitle}");
                }

                // If file is a movie, remove database reference and edition
                if(curFile.IsMovie())
                {
                    nameCopy = RemoveBracedPrefix(nameCopy, curFile.DatabaseRef);
                    nameCopy = RemovePrefix(nameCopy, "-");
                    nameCopy = RemoveBracedPrefix(nameCopy, $"edition-{((MovieFile)curFile).Edition}");
                }

                // Remove custom format
                nameCopy = RemoveBracketedPrefix(nameCopy, curFile.CustomFormat);

                // Remove quality title
                nameCopy = RemoveBracketedPrefix(nameCopy, curFile.QualityTitle);

                // If file is an anime, remove video and audio info in right order
                if (curFile.IsAnime())
                {
                    nameCopy = RemoveBracketedPrefix(nameCopy, curFile.GetAudioInfo());
                    nameCopy = RemoveBracketedPrefix(nameCopy, ((AnimeFile)curFile).AudioLanguages);
                    nameCopy = RemoveBracketedPrefix(nameCopy, $"{curFile.VideoCodec} {((AnimeFile)curFile).VideoBitDepth}bit");
                }

                // Else if not anime, remove audio info, THEN video codec
                nameCopy = RemoveBracketedPrefix(nameCopy, curFile.GetAudioInfo());
                nameCopy = RemoveBracketedPrefix(nameCopy, curFile.VideoCodec);

                // Remove release group
                nameCopy = RemovePrefix(nameCopy, $"-{curFile.ReleaseGroup}");

                // Remove extension
                nameCopy = RemovePrefix(nameCopy, curFile.Extension);

                // At this stage, the filename should have nothing left
                // If the filename still has something included, notify
                if (nameCopy.Length != 0)
                {
                    Console.WriteLine($"  - '{curFile.MediaFileName}' still had '{nameCopy}' left!");
                }
            }
        }

        /// <summary>
        /// Removes a `{value}` prefix from the input string, if present.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="value">The value inside the braces to remove.</param>
        /// <returns>The string with the braced prefix removed and trimmed.</returns>
        private static string RemoveBracedPrefix(string input, string value)
        {
            return RemoveDelimitedPrefix(input, value, "{", "}");
        }

        /// <summary>
        /// Removes a `[value]` prefix from the input string, if present.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="value">The value inside the brackets to remove.</param>
        /// <returns>The string with the bracketed prefix removed and trimmed.</returns>
        private static string RemoveBracketedPrefix(string input, string value)
        {
            return RemoveDelimitedPrefix(input, value, "[", "]");
        }

        /// <summary>
        /// Removes a delimited prefix (e.g., `{value}`, `[value]`) from the input string, if present.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="value">The value inside the delimiters to remove.</param>
        /// <param name="startDelimiter">The opening delimiter (e.g., `{`).</param>
        /// <param name="endDelimiter">The closing delimiter (e.g., `}`).</param>
        /// <returns>The string with the delimited prefix removed and trimmed.</returns>
        private static string RemoveDelimitedPrefix(string input, string value, string startDelimiter, string endDelimiter)
        {
            return RemovePrefix(input, startDelimiter + value + endDelimiter);
        }

        /// <summary>
        /// Removes the specified prefix from the start of a string and trims the result.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="prefix">The prefix to remove.</param>
        /// <returns>The trimmed string with the prefix removed if present.</returns>
        private static string RemovePrefix(string input, string prefix)
        {
            if (!input.StartsWith(prefix))
            {
                return input;
            }

            return input.Substring(prefix.Length).Trim();
        }
    }
}