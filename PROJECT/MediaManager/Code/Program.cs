using System.Text.RegularExpressions;

namespace MediaManager
{
    internal class Program
    {
        //// CONSTANTS/SETTINGS

        // Offload folder
        private static readonly string offloadFolderPath = "E:\\OFFLOAD_TEST_GROUND";

        // New media folders
        private static readonly string newShowsFolderName = "NEW_SHOWS";
        private static readonly string newShowsFolderPath = $"{offloadFolderPath}\\{newShowsFolderName}";
        private static readonly string newMoviesFolderName = "NEW_MOVIES";
        private static readonly string newMoviesFolderPath = $"{offloadFolderPath}\\{newMoviesFolderName}";

        // Info file name
        private static readonly string infoFileName = "INFO.txt";

        /// <summary>
        /// Main function
        /// </summary>
        /// <param name="args">Arguments given to program</param>
        private static void Main(string[] args)
        {
            // Start message
            Console.WriteLine("\n###### Media Manager ######\n");

            // Handle new shows folder
            HandleNewShowsFolder();
        }

        /// <summary>
        /// Handle the new shows folder
        /// </summary>
        private static void HandleNewShowsFolder()
        {
            Console.WriteLine($"Checking {newShowsFolderName} folder...");

            // Iterate over new show folders
            string[] newShowFolders = Directory.GetDirectories(newShowsFolderPath, "*", SearchOption.TopDirectoryOnly);

            if (newShowFolders.Length == 0)
            {
                Console.WriteLine("\nNo new shows found!");
            }

            foreach (string curNewShowFolder in newShowFolders)
            {
                // Info
                Console.Write($"- Found show: '{Path.GetFileName(curNewShowFolder)}'\n");

                // Iterate over new show season folders
                string[] newShowSeasonFolders = Directory.GetDirectories(curNewShowFolder, "*", SearchOption.TopDirectoryOnly);

                if (newShowSeasonFolders.Length == 0)
                {
                    throw new Exception($"'{curNewShowFolder}' contained no folders");
                }

                foreach (string curNewShowSeasonFolder in newShowSeasonFolders)
                {
                    HandleNewShowSeasonFolder(curNewShowSeasonFolder);
                }
            }
        }

        /// <summary>
        /// Handle a season folder of a new show
        /// </summary>
        /// <param name="curNewShowSeasonFolderPath">Path to the new show's season folder</param>
        private static void HandleNewShowSeasonFolder(string curNewShowSeasonFolderPath)
        {
            // Message start 
            string msgStart = " - ";

            // Info
            Console.Write($"{msgStart}Found season folder: '{Path.GetFileName(curNewShowSeasonFolderPath)}'\n");

            // Get list of the season folder's file paths, which should be mainly episodes
            string[] episodeFilePaths = Directory.GetFiles(curNewShowSeasonFolderPath, "*", SearchOption.AllDirectories);

            if (episodeFilePaths.Length == 0)
            {
                throw new Exception($"{msgStart}'{curNewShowSeasonFolderPath}' contained no files");
            }

            // Message start
            msgStart = $" {msgStart}";

            ////// Handle info file
            //// If info file already exists, skip folder
            if (episodeFilePaths.Any(f => Path.GetFileName(f).Equals(infoFileName, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"{msgStart}{infoFileName} already exists, skipping!\n");
                return;
            }

            //// Otherwise if info file doesn't exist, create it
            // Get list of episode filenames
            string[] episodeFileNames = episodeFilePaths.Select(f => Path.GetFileName(f)).ToArray();

            // Create info file in season folder with episode filenames and notify
            string infoFilePath = Path.Combine(curNewShowSeasonFolderPath, infoFileName);
            File.WriteAllLines(infoFilePath, episodeFileNames);
            Console.WriteLine($"{msgStart}Created {infoFileName} with {episodeFileNames.Length} filename(s).");

            ////// Rename files
            // Define regular expression to extract "SXXEXX - Episode Title"
            Regex episodeRegex = new Regex(@"
                    (                   # Start capturing group
                        S\d{2}E\d{2}     # Matches 'S01E10' (S followed by 2 digits, E followed by 2 digits)
                        \s-\s            # Matches ' - ' (space, hyphen, space)
                        [^[]+            # Matches everything **before** the first '[' (Episode Title)
                    )                   # End capturing group
                ", RegexOptions.IgnorePatternWhitespace); // Allows multiline regex with comments

            // Iterate over episode file paths
            foreach (string curEpFilePath in episodeFilePaths)
            {
                // Extract episode filename and apply regex
                string curEpFileName = Path.GetFileName(curEpFilePath);
                Match curEpMatch = episodeRegex.Match(curEpFileName);

                // If successfully extracted part of the filename
                if (curEpMatch.Success)
                {
                    // Generate new episode name and add file extension
                    string newEpName = curEpMatch.Value.Trim();
                    newEpName += Path.GetExtension(curEpFilePath);

                    // Get path to where episode file is and check
                    string? epFolder = Path.GetDirectoryName(curEpFilePath);
                    if (epFolder == null)
                    {
                        throw new Exception("Failed to get episode folder");
                    }

                    // Generate new episode file path
                    string newEpFilePath = Path.Combine(epFolder, newEpName);

                    // Rename file
                    File.Move(curEpFilePath, newEpFilePath);
                }
                else
                {
                    throw new Exception("Failed to apply regex pattern");
                }
            }

            Console.WriteLine("");
        }
    }
}