using System.Text.RegularExpressions;

namespace MediaManager
{
    /// <summary>
    /// Tests for EpisodeFile and AnimeFile regex patterns.
    /// Patterns replicated inline - must stay in sync with EpisodeFile.cs / AnimeFile.cs.
    /// </summary>
    internal static class EpisodeFileTests
    {
        // Replicate showEpRegex from EpisodeFile.cs
        private static readonly Regex ShowEpRegex = new Regex(@"
                    ^(?<Title>.+?)\s*(?:\((?<ReleaseYear>\d{4})\))?\s*-\s*
                    S(?<SeasonNum>\d{2})E(?<EpisodeNum>\d{2})\s*-\s*
                    (?<EpisodeTitle>.+?)\s*
                    (?:\[(?<CustomFormat>(?!DVD\b)(?!\w+-\d{3,4}p)[^\]]+)\])?
                    (?:\[(?<QualityTitle>(?!EAC3|AC3|AAC|DTS)[^\]]+)\])?
                    (?:\[(?<AudioCodec>[^\]\s]+(?:\s+[^\]\s]+)*)\s+(?<AudioChannels>[\d.]+)\])?
                    (?:\[(?<VideoDynamicRange>HDR|SDR|HLG)\])?
                    (?:\[(?<VideoBitDepth>\d+)bit\])?
                    (?:\[(?<VideoCodec>x264|x265|h264|h265|MPEG2)\])?
                    (?:\[(?<AudioLanguages>[^\]]+)\])?
                    (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace);

        // Replicate animeEpRegex from AnimeFile.cs
        private static readonly Regex AnimeEpRegex = new Regex(@"
                        ^(?<Title>.+?)\s*(?:\((?<ReleaseYear>\d{4})\))?\s*-\s*
                        S(?<SeasonNum>\d{2})E(?<EpisodeNum>\d{2})\s*-\s*
                        (?<AbsoluteEpisode>\d{3})\s*-\s*
                        (?<EpisodeTitle>.+?)\s*
                        (?:\[(?<CustomFormat>(?:(?![A-Z]{2,}(?:\+[A-Z]{2,})*|\w+-\d{3,4}p)[^\]])+)\])?
                        (?:\[(?<QualityTitle>(DVD|\w+-\d{3,4}p))\])?
                        (?:\[(?<AudioCodec>[^\]\s]+(?:\s+[^\]\s]+)*)\s+(?<AudioChannels>[\d.]+)\])?
                        (?:\[(?<AudioLanguages>[A-Z]{2,}(?:\+[A-Z]{2,})*)\])?
                        (?:\[(?<VideoCodec>x264|x265|h264|h265)\s+(?<VideoBitDepth>\d+)bit\])?
                        (?:\[(?<VideoDynamicRange>HDR|SDR|HLG)\])?
                        (?:-(?<ReleaseGroup>[^\]]+))?$", RegexOptions.IgnorePatternWhitespace);

        // ---------------------------------------------------------------------------
        // Show episode regex (EpisodeFile.showEpRegex)
        // ---------------------------------------------------------------------------

        public static void ShowEpRegex_DocstringExample_Matches()
        {
            var m = ShowEpRegex.Match("Gen V (2023) - S01E06 - Jumanji [AMZN][WEBDL-720p][EAC3 5.1][h264]-NTb");
            Assert.True(m.Success, "docstring example should match");
        }

        public static void ShowEpRegex_ExtractsTitle()
        {
            var m = ShowEpRegex.Match("Gen V (2023) - S01E06 - Jumanji [AMZN][WEBDL-720p][EAC3 5.1][h264]-NTb");
            Assert.Equal("Gen V", m.Groups["Title"].Value, "title should be extracted");
        }

        public static void ShowEpRegex_ExtractsSeasonAndEpisode()
        {
            var m = ShowEpRegex.Match("Gen V (2023) - S01E06 - Jumanji [AMZN][WEBDL-720p][EAC3 5.1][h264]-NTb");
            Assert.Equal("01", m.Groups["SeasonNum"].Value, "season number");
            Assert.Equal("06", m.Groups["EpisodeNum"].Value, "episode number");
        }

        public static void ShowEpRegex_ExtractsEpisodeTitle()
        {
            var m = ShowEpRegex.Match("Gen V (2023) - S01E06 - Jumanji [AMZN][WEBDL-720p][EAC3 5.1][h264]-NTb");
            Assert.Equal("Jumanji", m.Groups["EpisodeTitle"].Value, "episode title should be extracted");
        }

        public static void ShowEpRegex_ExtractsReleaseGroup()
        {
            var m = ShowEpRegex.Match("Gen V (2023) - S01E06 - Jumanji [AMZN][WEBDL-720p][EAC3 5.1][h264]-NTb");
            Assert.Equal("NTb", m.Groups["ReleaseGroup"].Value, "release group should be extracted");
        }

        public static void ShowEpRegex_RandomString_DoesNotMatch()
        {
            var m = ShowEpRegex.Match("not-an-episode-filename.mkv");
            Assert.True(!m.Success, "random string should not match");
        }

        // ---------------------------------------------------------------------------
        // Anime episode regex (AnimeFile.animeEpRegex)
        // ---------------------------------------------------------------------------

        public static void AnimeEpRegex_DocstringExample_Matches()
        {
            var m = AnimeEpRegex.Match("DAN DA DAN (2024) - S01E11 - 011 - First Love [v2][WEBDL-1080p][AAC 2.0][JA+EN][x264 8bit]-MALD");
            Assert.True(m.Success, "docstring anime example should match");
        }

        public static void AnimeEpRegex_ExtractsTitle()
        {
            var m = AnimeEpRegex.Match("DAN DA DAN (2024) - S01E11 - 011 - First Love [v2][WEBDL-1080p][AAC 2.0][JA+EN][x264 8bit]-MALD");
            Assert.Equal("DAN DA DAN", m.Groups["Title"].Value, "anime title");
        }

        public static void AnimeEpRegex_ExtractsAbsoluteEpisode()
        {
            var m = AnimeEpRegex.Match("DAN DA DAN (2024) - S01E11 - 011 - First Love [v2][WEBDL-1080p][AAC 2.0][JA+EN][x264 8bit]-MALD");
            Assert.Equal("011", m.Groups["AbsoluteEpisode"].Value, "absolute episode number");
        }

        public static void AnimeEpRegex_ExtractsAudioLanguages()
        {
            var m = AnimeEpRegex.Match("DAN DA DAN (2024) - S01E11 - 011 - First Love [v2][WEBDL-1080p][AAC 2.0][JA+EN][x264 8bit]-MALD");
            Assert.Equal("JA+EN", m.Groups["AudioLanguages"].Value, "audio languages");
        }

        public static void AnimeEpRegex_ExtractsVideoCodecAndBitDepth()
        {
            var m = AnimeEpRegex.Match("DAN DA DAN (2024) - S01E11 - 011 - First Love [v2][WEBDL-1080p][AAC 2.0][JA+EN][x264 8bit]-MALD");
            Assert.Equal("x264", m.Groups["VideoCodec"].Value, "video codec");
            Assert.Equal("8", m.Groups["VideoBitDepth"].Value, "video bit depth");
        }

        public static void AnimeEpRegex_RandomString_DoesNotMatch()
        {
            var m = AnimeEpRegex.Match("not-an-anime-filename.mkv");
            Assert.True(!m.Success, "random string should not match");
        }
    }
}
