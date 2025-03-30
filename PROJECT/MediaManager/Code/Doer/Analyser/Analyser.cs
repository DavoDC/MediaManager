using System;

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

            // Calculate common stats
            StatList extStats = new StatList("Extension", Parser.MediaFiles, f => f.Extension);
            StatList yearStats = new StatList("ReleaseYear", Parser.MediaFiles, f => f.ReleaseYear);
            StatList formatStats = new StatList("CustomFormat", Parser.MediaFiles, f => f.CustomFormat);
            StatList qualityStats = new StatList("QualityTitle", Parser.MediaFiles, f => f.QualityTitle);
            StatList videoRangeStats = new StatList("VideoDynamicRange", Parser.MediaFiles, f => f.VideoDynamicRange);
            StatList videoCodecStats = new StatList("VideoCodec", Parser.MediaFiles, f => f.VideoCodec);
            StatList audioCodecStats = new StatList("AudioCodec", Parser.MediaFiles, f => f.AudioCodec);
            StatList audioChannelStats = new StatList("AudioChannels", Parser.MediaFiles, f => f.AudioChannels);
            StatList relGroupStats = new StatList("ReleaseGroup", Parser.MediaFiles, f => f.ReleaseGroup);

            // Print common stats
            extStats.Print(0);
            yearStats.Print(0);
            formatStats.Print(0);
            qualityStats.Print(0);
            videoRangeStats.Print(0);
            videoCodecStats.Print(0);
            audioCodecStats.Print(0);
            audioChannelStats.Print(0);
            relGroupStats.Print(0);

            // Calculate and print movie stats
            // MAKE STATLIST HANDLE GENERIC TYPES
            //StatList editionStats = new StatList("Edition", Parser.MovieFiles, );

            // Finish and print time taken
            Console.WriteLine("");
            FinishAndPrintTimeTaken();
        }
    }
}
