using MediaManager.Code.Modules;
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

            // Calculate and print common stats
            Console.WriteLine("\n - Common statistics:");
            var extStats = CreateStatList("Extension", f => f.Extension);
            var yearStats = CreateStatList("ReleaseYear", f => f.ReleaseYear);
            var formatStats = CreateStatList("CustomFormat", f => f.CustomFormat);
            var qualityStats = CreateStatList("QualityTitle", f => f.QualityTitle);
            var videoRangeStats = CreateStatList("VideoDynamicRange", f => f.VideoDynamicRange);
            var videoCodecStats = CreateStatList("VideoCodec", f => f.VideoCodec);
            var audioCodecStats = CreateStatList("AudioCodec", f => f.AudioCodec);
            var audioChannelStats = CreateStatList("AudioChannels", f => f.AudioChannels);
            var relGroupStats = CreateStatList("ReleaseGroup", f => f.ReleaseGroup);
            //extStats.Print(0);
            //yearStats.Print(1.0);
            formatStats.Print(0);
            qualityStats.Print(0);
            videoRangeStats.Print(0);
            videoCodecStats.Print(0);
            audioCodecStats.Print(0);
            audioChannelStats.Print(0);
            relGroupStats.Print(0.25);

            // Calculate and print movie stats
            Console.WriteLine("\n - Movie-specific statistics:");
            var editionStats = new StatList<MovieFile>("Edition", Parser.MovieFiles, f => f.Edition);
            //editionStats.Print(0);

            // Calculate and print anime stats
            Console.WriteLine("\n - Anime-specific statistics:");
            var vidBitStats = new StatList<AnimeFile>("VideoBitDepth", Parser.AnimeFiles, f => f.VideoBitDepth);
            var audioLangStats = new StatList<AnimeFile>("AudioLanguages", Parser.AnimeFiles, f => f.AudioLanguages);
            vidBitStats.Print(0);
            audioLangStats.Print(0);

            // Finish and print time taken
            Console.WriteLine("");
            FinishAndPrintTimeTaken();
        }

        /// <summary>
        /// Creates a <see cref="StatList{MediaFile}"/> instance for a given property of <see cref="MediaFile"/> objects.
        /// </summary>
        /// <param name="name">The name of the statistic (e.g., "Extension", "ReleaseYear").</param>
        /// <param name="propertyExtractor">A function to extract the property from a <see cref="MediaFile"/> object.</param>
        /// <returns>A <see cref="StatList{MediaFile}"/> containing the frequency distribution of the specified property.</returns>
        private StatList<MediaFile> CreateStatList(string name, Func<MediaFile, string> propertyExtractor)
        {
            return new StatList<MediaFile>(name, Parser.MediaFiles, propertyExtractor);
        }
    }
}
