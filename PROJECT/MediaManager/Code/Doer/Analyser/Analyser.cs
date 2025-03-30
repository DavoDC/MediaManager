using System;
using System.Linq;

namespace MediaManager
{
    /// <summary>
    /// Analyses media file metadata to produce statistics
    /// </summary>
    internal class Analyser : Doer
    {
        // Variables
        //double artistStatsCutoff = 0.6;
        //double yearStatsCutoff = 2.0;
        //double decadeStatsCutoff = 0.0;

        /// <summary>
        /// Construct a media file analyser
        /// </summary>
        public Analyser()
        {
            // Notify
            Console.WriteLine("\nAnalysing metadata...");

            // ### CALCULATE STATS
            // Calculate basic stats
            //StatList artistStats = new StatList("Artists", audioTags, tag => tag.Artists);
            //StatList genreStats = new StatList("Genre", audioTags, tag => tag.Genres);
            //StatList yearStats = new StatList("Year", audioTags, tag => tag.Year);

            // Calculate decade stats 
            //StatList decadeStats = new StatList("Decade", StatList.GetDecadeFreqDist(yearStats));

            // ### PRINT STATS
            // Print artist stats
            //artistStatsExclMusivation.Print(artistStatsCutoff, "(Excluding Musivation)");
            //artistStats.Print(artistStatsCutoff, "(All)");

            // Print genre stats
            //genreStats.Print();

            // Print year and decade stats
            //yearStats.Print(yearStatsCutoff);
            //decadeStats.Print(decadeStatsCutoff);

            // Finish and print time taken
            FinishAndPrintTimeTaken();
        }
    }
}
