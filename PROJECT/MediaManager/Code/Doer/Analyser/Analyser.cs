using System;
using System.Linq;

namespace MediaManager
{
    /// <summary>
    /// Analyses media file metadata to produce statistics
    /// </summary>
    internal class Analyser : Doer
    {
        /// <summary>
        /// Construct a media file analyser
        /// </summary>
        public Analyser()
        {
            // Notify
            Console.WriteLine("\nAnalysing metadata...");

            //// CALCULATE STATS
            // Extension stats
            StatList extStats = new StatList("Extension", Parser.MediaFiles, f => f.Extension);

            //// PRINT STATS
            extStats.Print();

            // Finish and print time taken
            Console.WriteLine("");
            FinishAndPrintTimeTaken();
        }
    }
}
