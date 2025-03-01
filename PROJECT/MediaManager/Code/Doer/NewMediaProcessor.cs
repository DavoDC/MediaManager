using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediaManager
{
    /// <summary>
    /// Process new media so that it is ready for integration into the library
    /// </summary>
    internal class NewMediaProcessor : Doer
    {
        ////// CONSTANTS
        // Folder paths
        public static readonly string integrateFolderPath = Program.mediaFolderPath + "\\INTEGRATE";
        private static readonly string newShowsFolderPath = Path.Combine(integrateFolderPath, "SHOWS");
        private static readonly string newMoviesFolderPath = Path.Combine(integrateFolderPath, "MOVIES");

        public NewMediaProcessor()
        {
            // Handle new shows
            ProcessMediaFolder("show(s)", newShowsFolderPath, HandleShowFolder);

            // Handle new movies
            ProcessMediaFolder("movie(s)", newMoviesFolderPath, HandleMovieFolder);

            // Finish and print time taken
            FinishAndPrintTimeTaken();
            Console.WriteLine("");
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

                foldersReadyForIntegration++;
            }

            Console.WriteLine($" - {foldersReadyForIntegration} {mediaType} ready for integration!");
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
                HandleMediaFolder(seasonFolder);
            }
        }

        /// <summary>
        /// Handles a movie folder.
        /// </summary>
        /// <param name="movieFolder">Path to the movie folder.</param>
        private void HandleMovieFolder(string movieFolder)
        {
            //Console.WriteLine($"  - Found movie: '{Path.GetFileName(movieFolder)}'");
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

            //if (files.Length > 2)
            //{
            //    Program.PrintErrMsg($"'{movieFolder}' contains more than two files!");
            //    return;
            //}

            HandleMediaFolder(movieFolder);
        }

        /// <summary>
        /// Handles a media folder.
        /// </summary>
        /// <param name="folderPath">Path to the media folder.</param>
        private void HandleMediaFolder(string folderPath)
        {
            // Get file paths in media folder
            string[] filePaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

            // Look for info file path
            string infoFilePath = filePaths.
                FirstOrDefault(fp => Path.GetFileName(fp).Equals(Program.infoFileName)) ?? string.Empty;

            // If couldn't find it, notify
            if (string.IsNullOrEmpty(infoFilePath))
            {
                // Program.PrintErrMsg("Couldn't find info file.\n");
                return;
            }

            // Get list of media files (all files but info file)
            List<string> mediaFilePaths = new List<string>(filePaths);
            mediaFilePaths.Remove(infoFilePath);

            // Read file names from info file
            string[] infoFileNames = File.ReadAllLines(infoFilePath);

            // If info file name amount doesn't match media file amount 
            if (infoFileNames.Length != mediaFilePaths.Count)
            {
                Program.PrintErrMsg($"Found {infoFileNames.Length} info file name(s) " +
                    $"but {mediaFilePaths.Count} media file(s)");
                Console.Write($"Path: '{folderPath}'");
                return;
            }

            // Correct file name count 
            int correctFileNames = 0;

            // For every media path
            foreach (string mediaFilePath in mediaFilePaths)
            {
                // For every info file path
                foreach (string infoFileName in infoFileNames)
                {
                    // If info file name matches current name
                    if (infoFileName.Equals(Path.GetFileName(mediaFilePath)))
                    {
                        // Notify and skipped
                        //Console.WriteLine("   - File name is already correct!");
                        correctFileNames++;
                        continue;
                    }

                    // Extract current media file name with no extension
                    string curMediaFileName = Path.GetFileNameWithoutExtension(mediaFilePath);

                    // If the info file name is a longer version of the current name
                    if (infoFileName.Contains(curMediaFileName))
                    {
                        // Generate new path by combining folder with info file name
                        string newPath = $"{Path.GetDirectoryName(mediaFilePath)}\\{infoFileName}";

                        // Apply the longer name to the file
                        Directory.Move(mediaFilePath, newPath);

                        // Notify
                        Console.WriteLine($"   - Renamed '{curMediaFileName}' to '{infoFileName}'!");
                        correctFileNames++;
                    }
                }
            }

            // If not all media paths now have correct file names, notify
            if (correctFileNames != mediaFilePaths.Count)
            {
                Console.WriteLine($"  - ISSUE: Not all filenames are correct!!!");
            }

            //Console.WriteLine("");
        }
    }
}
