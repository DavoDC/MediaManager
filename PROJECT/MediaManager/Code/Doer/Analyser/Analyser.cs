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

            var vidBitStats = new StatList<AnimeFile>("VideoBitDepth", Parser.AnimeFiles, f => f.VideoBitDepth);
            vidBitStats.Print(0, "(Anime-Specific)");

            var audioChannelStats = CreateStatList("AudioChannels", f => f.AudioChannels);
            audioChannelStats.Print(0);

            var videoCodecStats = CreateStatList("VideoCodec", f => f.VideoCodec);
            videoCodecStats.Print(0);

            var audioLangStats = new StatList<AnimeFile>("AudioLanguages", Parser.AnimeFiles, f => f.AudioLanguages);
            audioLangStats.Print(0, "(Anime-Specific)");

            var audioCodecStats = CreateStatList("AudioCodec", f => f.AudioCodec);
            audioCodecStats.Print(0);

            var editionStats = new StatList<MovieFile>("Edition", Parser.MovieFiles, f => f.Edition);
            editionStats.Print(0, "(Movie-Specific)");

            var yearStats = CreateStatList("ReleaseYear", f => f.ReleaseYear);
            yearStats.Print(1.0);

            var qualityStats = CreateStatList("QualityTitle", f => f.QualityTitle);
            qualityStats.Print(0);

            var relGroupStats = CreateStatList("ReleaseGroup", f => f.ReleaseGroup);
            relGroupStats.Print(0.25);

            // DISABLED BECAUSE 98% unknown
            //var formatStats = CreateStatList("CustomFormat", f => f.CustomFormat);
            //formatStats.Print(0);

            // DISABLED BECAUSE 100% mkv
            //var extStats = CreateStatList("Extension", f => f.Extension);
            //extStats.Print(0);

            // DISABLED BECAUSE 100% unknown
            //var videoRangeStats = CreateStatList("VideoDynamicRange", f => f.VideoDynamicRange);
            //videoRangeStats.Print(0);

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
