using System.Text.RegularExpressions;

namespace MediaManager
{
    /// <summary>
    /// Tests for MovieFile regex pattern.
    /// Pattern replicated inline - must stay in sync with MovieFile.cs.
    /// </summary>
    internal static class MovieFileTests
    {
        // Replicate movieRegex from MovieFile.cs
        private static readonly Regex MovieRegex = new Regex(@"
                    ^(?<Title>.+?)\s*\((?<ReleaseYear>\d{4})\)\s*
                    \{(?<DBID>tmdb-\d+)\}\s*-\s*
                    (?:\{edition-(?<Edition>[^}]+)\}\s*)?
                    (?:\[(?<ThreeD>3D)\])?
                    (?:\[(?<CustomFormat>(?!DVD\b)(?!\w+-\d{3,4}p)[^\]]+)\])?
                    (?:\[(?<QualityTitle>(?!EAC3 5\.1)[^\]]+)\])?
                    (?:\[(?<AudioCodec>EAC3|[^\]\s]+(?:\s+[^\]\s]+)*)\s+(?<AudioChannels>[\d.]+)\])?
                    (?:\[(?<VideoDynamicRange>HDR|SDR|HLG)\])?
                    (?:\[(?<VideoCodec>x264|x265|h264|h265|AVC)\])?
                    (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace);

        public static void MovieRegex_DocstringExample_Matches()
        {
            var m = MovieRegex.Match(
                "Doctor Strange in the Multiverse of Madness (2022) {tmdb-453395} - {edition-IMAX Enhanced} [DSNP IMAX Enhanced][WEBDL-1080p][EAC3 Atmos 5.1][h264]-FLUX");
            Assert.True(m.Success, "docstring example should match");
        }

        public static void MovieRegex_ExtractsTitle()
        {
            var m = MovieRegex.Match(
                "Doctor Strange in the Multiverse of Madness (2022) {tmdb-453395} - {edition-IMAX Enhanced} [DSNP IMAX Enhanced][WEBDL-1080p][EAC3 Atmos 5.1][h264]-FLUX");
            Assert.Equal("Doctor Strange in the Multiverse of Madness", m.Groups["Title"].Value, "movie title");
        }

        public static void MovieRegex_ExtractsYearAndTmdbId()
        {
            var m = MovieRegex.Match(
                "Shrek (2001) {tmdb-808} - [WEBDL-1080p][EAC3 5.1][h264]-GROUP");
            Assert.True(m.Success, "simple movie should match");
            Assert.Equal("2001", m.Groups["ReleaseYear"].Value, "release year");
            Assert.Equal("tmdb-808", m.Groups["DBID"].Value, "tmdb ID");
        }

        public static void MovieRegex_ExtractsEdition()
        {
            var m = MovieRegex.Match(
                "Doctor Strange in the Multiverse of Madness (2022) {tmdb-453395} - {edition-IMAX Enhanced} [DSNP IMAX Enhanced][WEBDL-1080p][EAC3 Atmos 5.1][h264]-FLUX");
            Assert.Equal("IMAX Enhanced", m.Groups["Edition"].Value, "edition should be extracted");
        }

        public static void MovieRegex_ExtractsReleaseGroup()
        {
            var m = MovieRegex.Match(
                "Doctor Strange in the Multiverse of Madness (2022) {tmdb-453395} - {edition-IMAX Enhanced} [DSNP IMAX Enhanced][WEBDL-1080p][EAC3 Atmos 5.1][h264]-FLUX");
            Assert.Equal("FLUX", m.Groups["ReleaseGroup"].Value, "release group");
        }

        public static void MovieRegex_NoEdition_StillMatches()
        {
            var m = MovieRegex.Match("8 Mile (2002) {tmdb-65} - [WEBDL-720p][EAC3 2.0][h264]-GROUP");
            Assert.True(m.Success, "movie without edition should match");
            Assert.Equal("8 Mile", m.Groups["Title"].Value, "title");
            Assert.True(string.IsNullOrEmpty(m.Groups["Edition"].Value), "edition should be empty");
        }

        public static void MovieRegex_MissingTmdbId_DoesNotMatch()
        {
            var m = MovieRegex.Match("Shrek (2001) - [WEBDL-1080p][EAC3 5.1][h264]-GROUP");
            Assert.True(!m.Success, "movie without tmdb ID should not match");
        }

        public static void MovieRegex_RandomString_DoesNotMatch()
        {
            var m = MovieRegex.Match("random-file-name.mkv");
            Assert.True(!m.Success, "random string should not match");
        }
    }
}
