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
        /// Construct a parser
        /// </summary>
        /// <param name="mirrorPath">The mirror folder path</param>
        public Parser(string mirrorPath)
        {
            // Notify
            Console.WriteLine("\nParsing media metadata...");

            // Initialise movie file list
            MovieFiles = new List<MovieFile>();

            // Get mirrored movie folder path
            string mirrorMovieFolderPath = Path.Combine(mirrorPath, Prog.MovieFolderName);

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
    }
}
