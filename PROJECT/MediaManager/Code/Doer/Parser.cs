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
        /// A list of episode file objects
        /// </summary>
        public List<EpisodeFile> EpisodeFiles { get; }

        /// <summary>
        /// Construct a parser
        /// </summary>
        /// <param name="mirrorFolderPath">The mirror folder path</param>
        public Parser(string mirrorFolderPath)
        {
            // Notify
            Console.WriteLine("\nParsing media metadata...");

            // Initialise file lists
            MovieFiles = new List<MovieFile>();
            EpisodeFiles = new List<EpisodeFile>(); // TODO

            // Get mirrored movie folder path
            string mirrorMovieFolderPath = Path.Combine(mirrorFolderPath, Prog.MovieFolderName);

            // For every mirrored movie file
            string[] mirrorMovieFiles = Directory.GetFiles(mirrorMovieFolderPath, "*", SearchOption.AllDirectories);
            foreach (string mirrorMovieFilePath in mirrorMovieFiles)
            {
                // If XML file found
                string mirrorMovieFileExt = Path.GetExtension(mirrorMovieFilePath);
                if (mirrorMovieFileExt.Equals(".xml"))
                {
                    // Convert to movie file and add to list
                    MovieFiles.Add(new MovieFile(mirrorMovieFilePath));
                }
                else if (!Reflector.CopyExtensions.Contains(mirrorMovieFileExt))
                {
                    // Else if not a different kind of file we wanted in the mirror, throw exception
                    throw new ArgumentException($"Unexpected file found in mirror folder: {mirrorMovieFilePath}");
                }
            }

            // Print statistics
            Console.WriteLine($" - Files parsed: {MovieFiles.Count}");
            FinishAndPrintTimeTaken();
        }

        private void ParseMediaTypeFiles(string mirrorFolderPath, string mediaFolderName)
        {
            // Get mirror folder path
            string mirrorMediaFolderPath = Path.Combine(mirrorFolderPath, mediaFolderName);

            // For every mirror file
            string[] mirrorFiles = Directory.GetFiles(mirrorMediaFolderPath, "*", SearchOption.AllDirectories);
            foreach (string mirrorFilePath in mirrorFiles)
            {
                // If XML file found
                string mirrorFileExt = Path.GetExtension(mirrorFilePath);
                if (mirrorFileExt.Equals(".xml"))
                {
                    // Convert to media file and add to list
                    //MovieFiles.Add(new MovieFile(mirrorFilePath));
                }
                else if (!Reflector.CopyExtensions.Contains(mirrorFileExt))
                {
                    // Else if not an XML file and not a different kind of file we wanted in the mirror, throw exception
                    throw new ArgumentException($"Unexpected file found in mirror folder: {mirrorFilePath}");
                }
            }
        }

        // TEST
        //template T
        //public void AddToMediaList(List<T> mediaFileList)
    }
}
