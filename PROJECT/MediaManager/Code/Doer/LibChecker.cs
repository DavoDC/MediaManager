using MediaManager.Code.Modules;
using System;

namespace MediaManager
{
    /// <summary>
    /// Checks my media/mirror library against various conditions
    /// </summary>
    internal class LibChecker : Doer
    {
        /// <summary>
        /// Create library checker
        /// </summary>
        public LibChecker()
        {
            // Notify
            Console.WriteLine($"\nChecking mirror...");

            // Check anime bit depth
            Console.WriteLine("");
            foreach (AnimeFile curFile in Parser.AnimeFiles)
            {
                if (curFile.VideoBitDepth.Equals("Unknown"))
                {
                    Console.WriteLine(" - Found anime file with unknown vid bit depth: " + curFile.ToString());
                }
            }

            // Check anime audio languages
            Console.WriteLine("");
            foreach (AnimeFile curFile in Parser.AnimeFiles)
            {
                if (curFile.AudioLanguages.Equals("Unknown"))
                {
                    Console.WriteLine(" - Found anime file with unknown audio lang: " + curFile.ToString());
                }
            }

            // Finish and print time taken
            FinishAndPrintTimeTaken();
        }
    }
}