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
        //// CONSTANTS

        // Invalid file name characters
        private static readonly char[] invalidChars = Path.GetInvalidFileNameChars();

        // Media file extensions
        HashSet<string> mediaExtensions = new HashSet<string> { ".mp4", ".mkv", ".m4v", ".avi" };

        // Extensions of other files we want to include the mirror as-is
        HashSet<string> mirrorExtensions = new HashSet<string> { ".txt", ".lnk", ".url", ".srt", ".docx", ".ass", ".ssa" };

        // Extensions of file we expect to be present, but don't want in the mirror
        HashSet<string> expectedExtensions = new HashSet<string> { ".ini", ".ffs_db" };

        //// VARIABLES
        private string mirrorPath;
        private bool recreateMirror;
        private static readonly string mediaFolderPathInside = Program.MediaFolderPath + "\\";

        /// <summary>
        /// Construct an audio mirror
        /// </summary>
        /// <param name="mirrorPath">The audio mirror folder path</param>
        /// <param name="recreateMirror">Whether to recreate the mirror each time</param>
        public Reflector(string mirrorPath, bool recreateMirror)
        {
            // Save parameters
            this.mirrorPath = mirrorPath;
            this.recreateMirror = recreateMirror;

            // Notify
            Console.WriteLine($"\nCreating mirror of '{Program.MediaFolderPath}'...");

            // Setup folder structure
            CreateFolders();

            // Populate with files
            var statisticsInfo = CreateFiles();

            // Print statistics
            PrintStats(statisticsInfo);
        }


        /// <summary>
        /// Set up folder structure mirroring the actual folder
        /// </summary>
        public void CreateFolders()
        {
            // Note: This hardcoded path check is done to prevent folders from outside the repo from being deleted
            // If mirror path is outside of mirror repo
            if (!mirrorPath.Contains("C:\\Users\\David\\GitHubRepos\\MediaMirror"))
            {
                // Throw exception and notify
                string msg = $"\nMirror path was incorrect, outside the repo:\n{mirrorPath}\n";
                throw new ArgumentException(msg);
            }

            // If recreation wanted
            if (recreateMirror)
            {
                // Remove the mirror path if it exists, to recreate it
                string fixedMirrorPath = FixLongPath(mirrorPath, true);
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
                string newDirectoryPath = Path.Combine(mirrorPath, relativePath);
                Directory.CreateDirectory(newDirectoryPath);
            }
        }


        /// <summary>
        /// Populate folder structure with mirrored files
        /// </summary>
        /// <returns>Statistics tuple</returns>
        private Tuple<int, int, List<string>> CreateFiles()
        {
            // Holders
            int mediaFileCount = 0;
            int sanitisationCount = 0;
            List<string> unexpectedFiles = new List<string>();

            // For every actual file
            string[] realFiles = Directory.GetFiles(mediaFolderPathInside, "*", SearchOption.AllDirectories);
            foreach (string realFilePath in realFiles)
            {
                // Get relative file path and extension
                string relativePath = GetRelativePath(mediaFolderPathInside, realFilePath);
                string relPathExt = Path.GetExtension(relativePath);

                // Get file type info
                bool isMediaFile = mediaExtensions.Contains(relPathExt);
                bool isMirrorFile = mirrorExtensions.Contains(relPathExt);
                bool isExpectedFile = expectedExtensions.Contains(relPathExt);

                // If the file is a media file or other file we want to mirror 
                if(isMediaFile || isMirrorFile)
                {
                    // Create a mirror file
                    if (CreateMirrorFile(realFilePath, relativePath, isMediaFile))
                    {
                        sanitisationCount++;
                    }

                    if (isMediaFile)
                    {
                        mediaFileCount++;
                    }
                }
                else if (!isExpectedFile)
                {
                    // If the file wasn't a media, mirror or expected file,
                    // add relative path to unexpected list
                    unexpectedFiles.Add(relativePath);
                }
            }

            // Return holders
            return Tuple.Create(mediaFileCount, sanitisationCount, unexpectedFiles);
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
            if (fileName != sanitisedFilename)
            {
                // Replace filename in path with sanitised version
                relativePath = relativePath.Replace(fileName, sanitisedFilename);

                // Set flag
                sanitised = true;
            }

            // Generate the full mirror path
            string fullMirrorPath = Path.Combine(mirrorPath, relativePath);

            // If XML extension requested, change it 
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
        /// <param name="statisticsInfo"></param>
        private void PrintStats(Tuple<int, int, List<string>> statisticsInfo)
        {
            // Extract info items
            int mediaFileCount = statisticsInfo.Item1;
            int sanitisedFileNames = statisticsInfo.Item2;
            List<string> unexpectedFiles = statisticsInfo.Item3;

            // Print mirror path
            Console.WriteLine($" - Path: '{mirrorPath}'");

            // Print file count
            Console.WriteLine($" - Media file count: {mediaFileCount}");

            // Print non-media file info
            Console.WriteLine($" - Unexpected files found: {unexpectedFiles.Count}");
            if (unexpectedFiles.Count != 0)
            {
                Console.WriteLine($"  - Found: {string.Join(",", unexpectedFiles)}");
            }

            // Print sanitisation count
            Console.WriteLine($" - Media filenames sanitised: {sanitisedFileNames}");

            // Print recreation setting
            Console.WriteLine($" - Recreated: {recreateMirror}");

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
        /// </summary>
        /// <param name="longPath">A 'raw' long path</param>
        /// <param name="force">Force fixing the path regardless of length</param>
        /// <returns>The fixed path</returns>
        private string FixLongPath(string longPath, bool force = false)
        {
            // If path length is close to the Windows 260 character limit
            if (longPath.Length > 240 || force)
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
        private string GetRelativePath(string basePath, string targetPath)
        {
            // Convert paths to URIs for accurate relative path calculation
            Uri baseUri = new Uri(basePath);
            Uri targetUri = new Uri(targetPath);

            // Calculate relative URI
            Uri relativeUri = baseUri.MakeRelativeUri(targetUri);

            // Convert relative URI to a string and unescape special characters
            return Uri.UnescapeDataString(relativeUri.ToString());
        }
    }
}
