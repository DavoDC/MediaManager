using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MediaManager
{
    /// <summary>
    /// Creates a lightweight 'mirror' of a Media folder.
    /// </summary>
    internal class Reflector : Doer
    {
        /// <summary>
        /// CONSTANTS
        /// </summary>

        // Paths equal to or longer than this are considered "long paths"
        private static readonly int LongPathThreshold = 250;

        // Invalid file name characters
        private static readonly char[] invalidChars = Path.GetInvalidFileNameChars();

        // Extensions of files we expect to be present, but don't want in the mirror
        private static readonly HashSet<string> expectedExtensions = new HashSet<string> { ".lnk", ".ini", ".ffs_db" };

        // Extensions of media files that we want XML files for in the mirror
        private static readonly HashSet<string> mediaExtensions = new HashSet<string> { ".mp4", ".mkv", ".m4v", ".avi" };

        // Extensions of other files that we can copy as-is into the mirror
        private static readonly HashSet<string> copyExtensions = new HashSet<string> { ".ass", ".srt" };
        public static HashSet<string> CopyExtensions { get => copyExtensions; }

        // Currently all the files we want to copy as-is are subtitle extensions!
        public static HashSet<string> SubtitleExtensions { get => copyExtensions; }

        // Extensions of files we want in the mirror = media + copy extensions
        private static readonly HashSet<string> mirrorExtensions = new HashSet<string>(mediaExtensions.Union(copyExtensions));

        /// <summary>
        /// VARIABLES
        /// </summary>

        // The folder path to just inside the media folder
        private static readonly string mediaFolderPathInside = Prog.MediaFolderPath + "\\";

        // The number of media files in the mirror
        private int mediaFileCount;

        // A list of subtitle files (relative file paths) in the mirror
        private List<string> subtitleFiles = new List<string>();

        // The number of filenames sanitised while making the mirror
        private int sanitisationCount;

        // A list of unexpected files found while making the mirror
        private List<string> unexpectedFiles = new List<string>();

        // The longest path encountered while making the mirror
        private string longestPath = "";

        // The number of "long" paths found while making the mirror
        private int numberOfLongPaths = 0;


        /// <summary>
        /// Construct a media mirror
        /// </summary>
        public Reflector()
        {
            // Notify
            Console.WriteLine($"\nCreating mirror of '{Prog.MediaFolderPath}'...");

            // Setup folder structure
            CreateFolders();

            // Populate with files
            CreateFiles();

            // Print statistics
            PrintStats();
        }


        /// <summary>
        /// Set up folder structure mirroring the actual folder
        /// </summary>
        public void CreateFolders()
        {
            // Note: This hardcoded path check is done to prevent folders from outside the repo from being deleted
            // If mirror path is outside of mirror repo
            if (!Prog.AbsMirrorFolderPath.Contains("C:\\Users\\David\\GitHubRepos\\MediaMirror"))
            {
                // Throw exception and notify
                string msg = $"\nMirror path was incorrect, outside the repo:\n{Prog.AbsMirrorFolderPath}\n";
                throw new ArgumentException(msg);
            }

            // If mirror regeneration  wanted
            if (AgeChecker.RegenMirror)
            {
                // Remove the mirror path if it exists, so it gets fully regenerated
                string fixedMirrorPath = FixLongPath(Prog.AbsMirrorFolderPath, true);
                if (Directory.Exists(fixedMirrorPath))
                {
                    Directory.Delete(fixedMirrorPath, true);
                }
            }

            // For every real folder
            string[] realDirs = Directory.GetDirectories(mediaFolderPathInside, "*", SearchOption.AllDirectories);
            foreach (var directoryPath in realDirs)
            {
                // Get relative path of real folder
                string relativePath = GetRelativePath(mediaFolderPathInside, directoryPath);

                // Create folder at same location within mirror
                string newDirectoryPath = Path.Combine(Prog.AbsMirrorFolderPath, relativePath);
                Directory.CreateDirectory(newDirectoryPath);
            }
        }


        /// <summary>
        /// Populate folder structure with mirrored files
        /// </summary>
        private void CreateFiles()
        {
            // For every actual file
            string[] realFiles = Directory.GetFiles(mediaFolderPathInside, "*", SearchOption.AllDirectories);
            foreach (string realFilePath in realFiles)
            {
                // Get relative file path and extension
                string relativePath = GetRelativePath(mediaFolderPathInside, realFilePath);
                string relPathExt = Path.GetExtension(relativePath);

                // If this is a file we want to mirror
                if (mirrorExtensions.Contains(relPathExt))
                {
                    // Check if its a media file 
                    bool isMediaFile = mediaExtensions.Contains(relPathExt);

                    // Fix long real file paths
                    string fixedRealFilePath = FixLongPath(realFilePath);

                    // Create a mirror file for it
                    if (CreateMirrorFile(fixedRealFilePath, relativePath, isMediaFile))
                    {
                        sanitisationCount++;
                    }

                    // Increment media file count 
                    if (isMediaFile)
                    {
                        mediaFileCount++;
                    }

                    // Update subtitle file list
                    if (SubtitleExtensions.Contains(relPathExt))
                    {
                        subtitleFiles.Add(relativePath);
                    }
                }
                else if (!expectedExtensions.Contains(relPathExt))
                {
                    // Else if the file was not a file we want to mirror, and it wasn't expected,
                    // add its relative path to unexpected list
                    unexpectedFiles.Add(relativePath);
                }

                // Get the current file's path length
                int curPathLength = realFilePath.Length;

                // Update longest path encountered
                if (longestPath.Length < curPathLength)
                {
                    longestPath = realFilePath;
                }

                // Update number of long paths encountered
                if (LongPathThreshold < curPathLength)
                {
                    numberOfLongPaths++;
                }
            }
        }


        /// <summary>
        /// Create a mirrored file.
        /// <param name="filePath">The actual file path</param>
        /// <param name="relativePath">The relative file path</param>
        /// <param name="useXmlExt">Whether to create the mirror file as an XML file containing the real path. Otherwise copy as-is</param>
        /// <returns>True if the filename was sanitised</returns>
        private bool CreateMirrorFile(string realFilePath, string relativePath, bool useXmlExt)
        {
            // Sanitisation flag
            bool sanitised = false;

            // Get file name and sanitise it
            string fileName = Path.GetFileName(realFilePath);
            string sanitisedFilename = SanitiseFilename(fileName);

            // If file name was sanitised
            if (!fileName.Equals(sanitisedFilename))
            {
                // Replace filename in path with sanitised version
                relativePath = relativePath.Replace(fileName, sanitisedFilename);

                // Set flag
                sanitised = true;
            }

            // Generate the full mirror path
            string fullMirrorPath = Path.Combine(Prog.AbsMirrorFolderPath, relativePath);

            // If XML extension requested, change the path's extension
            if (useXmlExt)
            {
                fullMirrorPath = Path.ChangeExtension(fullMirrorPath, ".xml");
            }

            // If the mirrored file doesn't exist already
            if (!File.Exists(fullMirrorPath))
            {
                // Fix long mirror paths 
                fullMirrorPath = FixLongPath(fullMirrorPath);

                // If XML file requested
                if (useXmlExt)
                {
                    // Create mirror XML file and store real path inside it
                    File.WriteAllText(fullMirrorPath, realFilePath);
                }
                else
                {
                    // Otherwise, copy file as-is from real file path to mirror path
                    File.Copy(realFilePath, fullMirrorPath, true); // 'true' overwrites if the file exists
                }
            }

            // Return sanitised flag
            return sanitised;
        }


        /// <summary>
        /// Print info about completed mirroring process 
        /// </summary>
        private void PrintStats()
        {
            // Print mirror path
            Console.WriteLine($" - Path: '{Prog.AbsMirrorFolderPath}'");

            // Print regen setting
            Console.WriteLine($" - Regenerated: {AgeChecker.RegenMirror}");

            // Print media file count
            Console.WriteLine($" - Media file count: {mediaFileCount}");

            // Print subtitle file count
            Console.WriteLine($" - Subtitle file count: {subtitleFiles.Count}");
            //PrintRelPaths(subtitleFiles);

            // Print non-media file info
            Console.WriteLine($" - Unexpected files found: {unexpectedFiles.Count}");
            if (unexpectedFiles.Count != 0)
            {
                // Get unique extensions and print out
                HashSet<string> uniqueExtensions = unexpectedFiles
                    .Select(file => Path.GetExtension(file).ToLower()) // Extract and normalize extensions
                    .Where(ext => !string.IsNullOrEmpty(ext)) // Exclude empty extensions
                    .ToHashSet();
                Console.WriteLine($"  - Extension(s): {string.Join(",", uniqueExtensions)}");

                // Print out unexpected file paths
                PrintRelPaths(unexpectedFiles);
            }

            // Print sanitisation count
            Console.WriteLine($" - Media filenames sanitised: {sanitisationCount}");

            // Print number of long paths encountered
            Console.WriteLine($" - Long paths encountered: {numberOfLongPaths} paths exceeded {LongPathThreshold} chars");

            // Print longest path length
            Console.WriteLine($" - Longest path length: {longestPath.Length} chars");
            //Console.WriteLine($"  - Longest path: {longestPath}");

            // Print time taken
            FinishAndPrintTimeTaken();
        }


        /// <summary>
        /// Sanitise a given filename of special characters
        /// </summary>
        /// <param name="filename">Original filename.</param>
        /// <returns>Sanitised filename.</returns>
        public static string SanitiseFilename(string filename)
        {
            // Remove any non-ASCII characters and replace wide characters with their closest equivalent
            string sanitisedFilename = new string(filename
                .Where(c => Char.GetUnicodeCategory(c) != UnicodeCategory.Control &&
                            Char.GetUnicodeCategory(c) != UnicodeCategory.ModifierSymbol &&
                            Char.GetUnicodeCategory(c) != UnicodeCategory.OtherSymbol)
                .ToArray());
            sanitisedFilename = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(sanitisedFilename));

            // Replace invalid characters with an underscore if found
            if (sanitisedFilename.Any(c => invalidChars.Contains(c)))
            {
                sanitisedFilename = new string(sanitisedFilename.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            }

            // Return new file name
            return sanitisedFilename;
        }


        /// <summary>
        /// Fix long paths which cause I/O exceptions
        /// See https://learn.microsoft.com/en-us/windows/win32/fileio/naming-a-file#maximum-path-length-limitation
        /// </summary>
        /// <param name="longPath">A 'raw' long path</param>
        /// <param name="force">Force fixing the path regardless of length</param>
        /// <returns>The fixed path</returns>
        public static string FixLongPath(string longPath, bool force = false)
        {
            // If path length is close to the Windows 260 character limit
            if (longPath.Length > LongPathThreshold || force)
            {
                // Add the UNC prefix to tell Windows to bypass the limit
                longPath = @"\\?\" + longPath;

                // Normalise slashes (i.e. replace all forward slashes with backslashes)
                longPath = longPath.Replace('/', '\\');

                // Return new path
                return longPath;
            }

            // Return path as is
            return longPath;
        }


        /// <summary>
        /// Calculates the relative path from a base path to a target path.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="targetPath">The target path.</param>
        /// <returns>The relative path from the base path to the target path.</returns>
        public static string GetRelativePath(string basePath, string targetPath)
        {
            // Convert paths to URIs for accurate relative path calculation
            Uri baseUri = new Uri(basePath);
            Uri targetUri = new Uri(targetPath);

            // Calculate relative URI
            Uri relativeUri = baseUri.MakeRelativeUri(targetUri);

            // Convert relative URI to a string and unescape special characters
            return Uri.UnescapeDataString(relativeUri.ToString());
        }

        /// <summary>
        /// Print out a list of relative file paths
        /// </summary>
        /// <param name="relFilePathList">The list of relative file paths</param>
        public static void PrintRelPaths(List<string> relFilePathList)
        {
            Console.WriteLine($"  - Path(s): ");
            foreach (string curRelFilePath in relFilePathList)
            {
                Console.WriteLine($"   - {curRelFilePath}");
            }
        }
    }
}
