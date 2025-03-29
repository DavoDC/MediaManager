using MediaManager.Code.Modules;
using System;
using System.Collections.Generic;
using System.IO;

namespace MediaManager
{
    /// <summary>
    /// Parses media metadata to generate media file objects and XML files
    /// </summary>
    internal class Parser : Doer
    {
        /// <summary>
        /// A list of movie file objects
        /// </summary>
        public List<MovieFile> MovieFiles { get; }

        /// <summary>
        /// A list of show file objects
        /// </summary>
        public List<ShowFile> ShowFiles { get; }

        /// <summary>
        /// A list of anime file objects
        /// </summary>
        public List<AnimeFile> AnimeFiles { get; }

        /// <summary>
        /// Construct a parser
        /// </summary>
        public Parser()
        {
            // Notify
            Console.WriteLine("\nParsing media metadata...");

            // Initialise movie file list
            MovieFiles = ParseMediaTypeFiles<MovieFile>(Prog.MovieFolderName);

            // Initialise show file list
            ShowFiles = ParseMediaTypeFiles<ShowFile>(Prog.ShowFolderName);

            // Initialise anime file list
            AnimeFiles = ParseMediaTypeFiles<AnimeFile>(Prog.AnimeFolderName);

            // Print total number of files parsed
            int totalFilesParsed = MovieFiles.Count + ShowFiles.Count + AnimeFiles.Count;
            PrintFilesParsed("Total", totalFilesParsed);

            // Finish
            FinishAndPrintTimeTaken();
        }

        /// <summary>
        /// Parses media files of a specified type from the given folder and adds them to the provided list.
        /// </summary>
        /// <typeparam name="T">The type of media file to parse (e.g., MovieFile).</typeparam>
        /// <param name="mirrorFolderPath">The root path of the mirrored media folder.</param>
        /// <param name="mediaFolderName">The name of the specific media subfolder to scan.</param>
        /// <param name="mediaFileList">The list to which parsed media files will be added.</param>
        /// <exception cref="ArgumentException">Thrown if an unexpected file is found in the folder.</exception>
        private List<T> ParseMediaTypeFiles<T>(string mediaFolderName) where T : MediaFile
        {
            // Initialize list
            List<T> mediaFileList = new List<T>();

            // Get the path to the media folder in the mirror
            string mirrorMediaFolderPath = Path.Combine(Prog.AbsMirrorFolderPath, mediaFolderName);

            // Get a list of all the mirrored files in that folder
            string[] mirrorFiles = Directory.GetFiles(mirrorMediaFolderPath, "*", SearchOption.AllDirectories);

            // For every mirror file
            foreach (string mirrorFilePath in mirrorFiles)
            {
                // If the file is an XML file
                string mirrorFileExt = Path.GetExtension(mirrorFilePath);
                if (mirrorFileExt.Equals(".xml"))
                {
                    // Create a media object instance dynamically using reflection
                    T mediaFile = (T)Activator.CreateInstance(typeof(T), mirrorFilePath);

                    // Add to list
                    mediaFileList.Add(mediaFile);
                }
                else if (!Reflector.CopyExtensions.Contains(mirrorFileExt))
                {
                    // Else if an unexpected extension is encountered, throw exception
                    throw new ArgumentException($"Unexpected file found in mirror folder: {mirrorFilePath}");
                }
            }

            // Log the number of files parsed
            PrintFilesParsed(mediaFolderName.Replace("s", ""), mediaFileList.Count);

            // Return media list generated
            return mediaFileList;
        }

        /// <summary>
        /// Print a message regarding how many files were passed
        /// </summary>
        /// <param name="fileDesc">A descriptor for the files parsed.</param>
        /// <param name="mediaFileCount">The number of files parsed.</param>
        private void PrintFilesParsed(string fileDesc, int mediaFileCount)
        {
            Console.WriteLine($" - {fileDesc} files parsed: {mediaFileCount}");
        }
    }
}
