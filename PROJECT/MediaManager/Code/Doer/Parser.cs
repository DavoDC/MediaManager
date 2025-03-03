using MediaManager.Code.Modules;
using System;
using System.Collections.Generic;
using System.IO;

namespace MediaManager
{
    /// <summary>
    /// Parses media metadata into XML files.
    /// </summary>
    internal class Parser : Doer
    {
        // Properties
        public List<MediaTag> MediaTags { get; }

        /// <summary>
        /// Construct a parser
        /// </summary>
        /// <param name="mirrorPath">The mirror folder path</param>
        public Parser(string mirrorPath)
        {
            // Notify
            Console.WriteLine("\nParsing media metadata...");

            // Initialise tag list
            MediaTags = new List<MediaTag>();

            // For every mirrored file
            string[] mirrorFiles = Directory.GetFiles(mirrorPath, "*", SearchOption.AllDirectories);
            foreach (string mirrorFilePath in mirrorFiles)
            {
                // If XML file found
                string mirrorFileExt = Path.GetExtension(mirrorFilePath);
                if (mirrorFileExt.Equals(".xml"))
                {
                    // Apply long path fix 
                    string fixedMirrorFilePath = Reflector.FixLongPath(mirrorFilePath);

                    // Convert to tag and add to list
                    MediaTags.Add(new MediaTag(fixedMirrorFilePath));
                }
                else if (!Reflector.CopyExtensions.Contains(mirrorFileExt))
                {
                    // Else if not a different kind of file we wanted in the mirror, throw exception
                    throw new ArgumentException($"Unexpected file found in mirror folder: {mirrorFilePath}");
                }
            }

            // Print statistics
            Console.WriteLine($" - Files parsed: {MediaTags.Count}");
            FinishAndPrintTimeTaken();
        }
    }
}
