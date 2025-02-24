using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediaManager
{
    /// <summary>
    /// Process new media in the offload folder
    /// </summary>
    internal class NewMediaProcessor : Doer
    {
        ////// CONSTANTS
        // Folder paths
        public static readonly string integrateFolderPath = Program.mediaFolderPath + "\\INTEGRATE";
        private static readonly string newShowsFolderPath = Path.Combine(integrateFolderPath, "NEW_SHOWS");
        private static readonly string newMoviesFolderPath = Path.Combine(integrateFolderPath, "NEW_MOVIES");

        // Regexes for renaming media
        private static readonly Regex episodeRegex = new Regex(@"S\d{2}E\d{2}\s-\s[^[]+", RegexOptions.IgnorePatternWhitespace);
        private static readonly Regex movieRegex = new Regex(@"^[^(]+\(\d{4}\)", RegexOptions.IgnorePatternWhitespace);
        private static readonly string regexErr = "Failed to apply regex pattern to:";

        public NewMediaProcessor()
        {
            // Handle new shows
            ProcessMediaFolder("shows", newShowsFolderPath, HandleShowFolder);

            // Handle new movies
            ProcessMediaFolder("movies", newMoviesFolderPath, HandleMovieFolder);

            // Finish and print time taken
            FinishAndPrintTimeTaken();
        }

        /// <summary>
        /// Processes a media folder by iterating over its subdirectories and applying the given handler.
        /// </summary>
        /// <param name="mediaType">The media type (e.g. 'shows' or 'movies').</param>
        /// <param name="folderPath">Path to the media folder.</param>
        /// <param name="handler">Handler function to process each subdirectory.</param>
        private void ProcessMediaFolder(string mediaType, string folderPath, Action<string> handler)
        {
            string[] subFolders = Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);

            if (subFolders.Length == 0)
            {
                Console.WriteLine($" - No {mediaType} to integrate!");
                return;
            }

            int foldersReadyForIntegration = 0;
            foreach (var subFolder in subFolders)
            {
                handler(subFolder);

                RenameFolder(subFolder);

                foldersReadyForIntegration++;
            }

            Console.WriteLine($" - {foldersReadyForIntegration} {mediaType} are ready for integration!");
        }

        /// <summary>
        /// Handles a show folder by processing its season subdirectories.
        /// </summary>
        /// <param name="showFolder">Path to the show folder.</param>
        private void HandleShowFolder(string showFolder)
        {
            //Console.WriteLine($"- Found show: '{Path.GetFileName(showFolder)}'");
            string[] seasonFolders = Directory.GetDirectories(showFolder, "*", SearchOption.TopDirectoryOnly);

            if (seasonFolders.Length == 0)
            {
                Program.PrintErrMsg($"'{showFolder}' contained no season folders!");
                return;
            }

            foreach (var seasonFolder in seasonFolders)
            {
                HandleMediaFolder(seasonFolder, episodeRegex);
            }
        }

        /// <summary>
        /// Handles a movie folder.
        /// </summary>
        /// <param name="movieFolder">Path to the movie folder.</param>
        private void HandleMovieFolder(string movieFolder)
        {
            //Console.WriteLine($"- Found movie: '{Path.GetFileName(movieFolder)}'");
            string[] files = Directory.GetFiles(movieFolder, "*", SearchOption.AllDirectories);

            if (Directory.GetDirectories(movieFolder).Length > 0)
            {
                Program.PrintErrMsg($"'{movieFolder}' movie folder has subfolders!");
                return;
            }

            if (files.Length == 0)
            {
                Program.PrintErrMsg($"'{movieFolder}' contains no files!");
                return;
            }

            if (files.Length > 2)
            {
                Program.PrintErrMsg($"'{movieFolder}' contains more than two files!");
                return;
            }

            HandleMediaFolder(movieFolder, movieRegex);
        }

        /// <summary>
        /// Handles a media folder by creating an info file and renaming files based on a regex pattern, if needed.
        /// </summary>
        /// <param name="folderPath">Path to the media folder.</param>
        /// <param name="mediaRegex">Regular expression to match and rename files.</param>
        private void HandleMediaFolder(string folderPath, Regex mediaRegex)
        {
            // Get file paths in media folder
            string[] filePaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

            // If an info file was created for the folder, rename the files it contains as well
            if (HandleInfoFile(folderPath, filePaths))
            {
                RenameFiles(filePaths, folderPath, mediaRegex);
            }
        }

        /// <summary>
        /// Handles the creation of an info file in the media folder if it does not already exist.
        /// The info file will contain a list of filenames (without paths) from the media folder.
        /// </summary>
        /// <param name="folderPath">Path to the media folder where the info file will be created.</param>
        /// <param name="filePaths">List of file paths in the media folder, used to extract filenames.</param>
        /// <returns>
        /// True if an info file was created, otherwise false.
        /// </returns>
        private bool HandleInfoFile(string folderPath, string[] filePaths)
        {
            // Return false if the info file already exists
            if (filePaths.Any(f => Path.GetFileName(f).Equals(Program.infoFileName, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            //// Otherwise if the info file doesn't exist, create it

            // Get array of filenames (that is guaranteed to have no null values, to prevent CS8620)
            string[] fileNames = filePaths.Select(Path.GetFileName).Where(fn => fn != null).Cast<string>().ToArray();

            // Create info file with filenames and notify
            File.WriteAllLines(Path.Combine(folderPath, Program.infoFileName), fileNames);
            //Console.WriteLine($"- Created {infoFileName} with {fileNames.Length} filename(s).");

            // Return true as an info file was created
            return true;
        }

        /// <summary>
        /// Renames files in a media folder based on a regex pattern.
        /// </summary>
        /// <param name="filePaths">List of file paths to be renamed.</param>
        /// <param name="folderPath">Path to the media folder.</param>
        /// <param name="regex">Regular expression used for renaming.</param>
        private void RenameFiles(string[] filePaths, string folderPath, Regex regex)
        {
            foreach (var filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                Match match = regex.Match(fileName);
                if (match.Success)
                {
                    string newFileName = match.Value.Trim() + Path.GetExtension(filePath);
                    string newFilePath = Path.Combine(folderPath, newFileName);
                    File.Move(filePath, newFilePath);
                }
                else
                {
                    Program.PrintErrMsg($"{regexErr} '{fileName}'!");
                }
            }
        }

        /// <summary>
        /// Renames a top-level media folder based on the movieRegex pattern.
        /// </summary>
        /// <param name="folderPath">Path to a movie or show folder (i.e. show folder holding season folders).</param>
        private void RenameFolder(string folderPath)
        {
            // Extract folder name and apply regex
            string folderName = Path.GetFileName(folderPath);
            Match match = movieRegex.Match(folderName);

            // If applying regex failed, notify and stop
            if (!match.Success)
            {
                Program.PrintErrMsg($"{regexErr} '{folderName}'!");
                return;
            }

            // Generate new folder path
            string newFolderName = match.Value.Trim();
            string newFolderPath = Path.Combine(Path.GetDirectoryName(folderPath), newFolderName);

            // If new path differs from original, rename folder with new name
            if (folderPath != newFolderPath)
            {
                Directory.Move(folderPath, newFolderPath);
                //Console.WriteLine($"Renamed folder: '{folderName}' -> '{newFolderName}'");
            }
        }
    }
}
