using MediaManager.Code.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
        public static List<MovieFile> MovieFiles { get; set; }

        /// <summary>
        /// A list of show file objects
        /// </summary>
        public static List<ShowFile> ShowFiles { get; set; }

        /// <summary>
        /// A list of anime file objects
        /// </summary>
        public static List<AnimeFile> AnimeFiles { get; set; }

        /// <summary>
        /// A list of episode file objects
        /// </summary>
        public static List<EpisodeFile> EpisodeFiles { get; set; }

        /// <summary>
        /// A list of all media file objects combined
        /// </summary>
        public static List<MediaFile> MediaFiles { get; set; }

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

            // Initialise episode file list
            var episodeFileListsCombined = new List<IEnumerable<EpisodeFile>> { ShowFiles, AnimeFiles };
            EpisodeFiles = episodeFileListsCombined.SelectMany(list => list).ToList();

            // Initialise combined media file list
            var mediaFileListsCombined = new List<IEnumerable<MediaFile>> { MovieFiles, ShowFiles, AnimeFiles };
            MediaFiles = mediaFileListsCombined.SelectMany(list => list).ToList();

            // Print total number of files parsed
            PrintFilesParsed("Total", MediaFiles.Count);

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
                    try
                    {
                        // Create a media object instance dynamically using reflection
                        T mediaFile = (T)Activator.CreateInstance(typeof(T), mirrorFilePath);

                        // Add to list
                        mediaFileList.Add(mediaFile);
                    }
                    catch (TargetInvocationException ex)
                    {
                        // If constructor failed, print more info
                        Console.WriteLine("\nConstructor threw an exception: " + ex.InnerException?.Message);
                        Console.WriteLine("\nStack trace: " + ex.InnerException?.StackTrace);
                        Console.WriteLine("\nMirror file path: '" + mirrorFilePath + "'");
                        throw;
                    }
                }
                else if (!Reflector.CopyExtensions.Contains(mirrorFileExt))
                {
                    // Else if an unexpected extension is encountered, throw exception
                    throw new ArgumentException($"Unexpected file found in mirror folder: {mirrorFilePath}");
                }
            }

            // Print number of items parsed
            int mediaFileCount = mediaFileList.Count;
            string mediaTypeDesc = mediaFolderName.Replace("s", "");
            if (mediaTypeDesc.Equals("Movie"))
            {
                // For movies, simply print count (as movies == files)
                PrintFilesParsed(mediaTypeDesc, mediaFileCount);
            } 
            else
            {
                // Else for episodic media, print the number of media items, and the total files
                // Note: If a show folder is empty, it won't add to the "mediaItems" count, meaning the actual show folder can have more folders.
                int mediaItems = mediaFileList
                    .Select(m => $"{m.Title}_{m.DatabaseRef}")
                    .Distinct()
                    .Count();

                PrintFilesParsed(mediaTypeDesc, mediaItems, $"({mediaFileList.Count} files)");
            }

            // Return media list generated
            return mediaFileList;
        }

        /// <summary>
        /// Print a message regarding how many files were passed
        /// </summary>
        /// <param name="fileDesc">A descriptor for the files parsed.</param>
        /// <param name="mediaFileCount">The number of files parsed.</param>
        private void PrintFilesParsed(string fileDesc, int mediaFileCount, string extra = "")
        {
            Console.WriteLine($" - {fileDesc} files parsed: {mediaFileCount} {extra}");
        }
    }
}
