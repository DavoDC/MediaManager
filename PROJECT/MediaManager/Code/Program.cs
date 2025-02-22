using System.Text.RegularExpressions;

namespace MediaManager
{
    internal class Program
    {
        //// CONSTANTS
        // Offload folder
        private static readonly string offloadFolderPath = "E:\\OFFLOAD";

        // New media folders
        private static readonly string newShowsFolderPath = Path.Combine(offloadFolderPath, "NEW_SHOWS");
        private static readonly string newMoviesFolderPath = Path.Combine(offloadFolderPath, "NEW_MOVIES");

        // Info file
        private static readonly string infoFileName = "INFO.txt";

        // Regexes for renaming media
        private static readonly Regex episodeRegex = new Regex(@"S\d{2}E\d{2}\s-\s[^[]+", RegexOptions.IgnorePatternWhitespace);
        private static readonly Regex movieRegex = new Regex(@"^[^(]+\(\d{4}\)", RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Main entry point of the program.
        /// </summary>
        private static void Main()
        {
            Console.WriteLine("\n###### Media Manager ######\n");

            // Handle new shows
            ProcessMediaFolder(newShowsFolderPath, HandleShowFolder);

            // Handle new movies
            ProcessMediaFolder(newMoviesFolderPath, HandleMovieFolder);
        }

        /// <summary>
        /// Processes a media folder by iterating over its subdirectories and applying the given handler.
        /// </summary>
        /// <param name="folderPath">Path to the media folder.</param>
        /// <param name="handler">Handler function to process each subdirectory.</param>
        private static void ProcessMediaFolder(string folderPath, Action<string> handler)
        {
            Console.WriteLine($"Checking {Path.GetFileName(folderPath)} folder...");
            string[] subFolders = Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);

            if (subFolders.Length == 0)
            {
                Console.WriteLine("- No new media found!\n");
                return;
            }

            int foldersReadyForIntegration = 0;
            foreach (var subFolder in subFolders)
            {
                handler(subFolder);
                foldersReadyForIntegration++;
            }
            Console.WriteLine($"- {foldersReadyForIntegration} folders ready for integration!\n");
        }

        /// <summary>
        /// Handles a show folder by processing its season subdirectories.
        /// </summary>
        /// <param name="showFolder">Path to the show folder.</param>
        private static void HandleShowFolder(string showFolder)
        {
            //Console.WriteLine($"- Found show: '{Path.GetFileName(showFolder)}'");
            string[] seasonFolders = Directory.GetDirectories(showFolder, "*", SearchOption.TopDirectoryOnly);

            if (seasonFolders.Length == 0)
            {
                throw new Exception($"'{showFolder}' contained no season folders!");
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
        private static void HandleMovieFolder(string movieFolder)
        {
            //Console.WriteLine($"- Found movie: '{Path.GetFileName(movieFolder)}'");
            string[] files = Directory.GetFiles(movieFolder, "*", SearchOption.AllDirectories);

            if (Directory.GetDirectories(movieFolder).Length > 0)
            {
                throw new Exception($"'{movieFolder}' movie folder has subfolders!");
            }

            if (files.Length == 0)
            {
                throw new Exception($"'{movieFolder}' contains no files!");
            }

            if (files.Length > 2)
            {
                throw new Exception($"'{movieFolder}' contains more than two files!");
            }

            HandleMediaFolder(movieFolder, movieRegex);
        }

        /// <summary>
        /// Handles a media folder by creating an info file and renaming files based on a regex pattern, if needed.
        /// </summary>
        /// <param name="folderPath">Path to the media folder.</param>
        /// <param name="mediaRegex">Regular expression to match and rename files.</param>
        private static void HandleMediaFolder(string folderPath, Regex mediaRegex)
        {
            // Get file paths in media folder
            string[] filePaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

            // If an info file was created for the folder, rename the files it contains as well
            if(HandleInfoFile(folderPath, filePaths))
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
        private static bool HandleInfoFile(string folderPath, string[] filePaths)
        {
            // Return false if the info file already exists
            if (filePaths.Any(f => Path.GetFileName(f).Equals(infoFileName, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            //// Otherwise if the info file doesn't exist, create it
      
            // Get array of filenames (that is guaranteed to have no null values, to prevent CS8620)
            string[] fileNames = filePaths.Select(Path.GetFileName).Where(fn => fn != null).Cast<string>().ToArray();

            // Create info file with filenames and notify
            File.WriteAllLines(Path.Combine(folderPath, infoFileName), fileNames);
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
        private static void RenameFiles(string[] filePaths, string folderPath, Regex regex)
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
                    throw new Exception($"Failed to apply regex pattern to: '{fileName}'!");
                }
            }
        }
    }
}
