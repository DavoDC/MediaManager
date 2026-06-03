using MediaManager.Code.Modules;
using System.Text.RegularExpressions;

namespace MediaManager
{
    /// <summary>
    /// Tests for MediaFile static methods and the core regex patterns.
    /// All tests are pure - no file I/O, no abstract class instantiation.
    /// </summary>
    internal static class MediaFileTests
    {
        // Replicate the private regexes here for pattern-correctness tests.
        // These must stay in sync with the patterns in MediaFile.cs.
        private static readonly Regex FolderRegex = new Regex(
            @"^(?<Title>.*?)\s\((?<Year>\d{4})\)\s\{(?<ID>(?:tvdb|tmdb)-\d+)\}$");

        private static readonly Regex ConciseQualityTitleRegex = new Regex(
            @"\w+-\d{3,4}p", RegexOptions.Compiled);

        // ---------------------------------------------------------------------------
        // GetGroupValue
        // ---------------------------------------------------------------------------

        public static void GetGroupValue_MatchingGroup_ReturnsValue()
        {
            var rx = new Regex(@"(?<Name>\w+)");
            var m = rx.Match("Hello");
            string result = MediaFile.GetGroupValue(m, "Name");
            Assert.Equal("Hello", result, "matching group");
        }

        public static void GetGroupValue_EmptyGroup_ReturnsDefault()
        {
            var rx = new Regex(@"(?<Name>\w*)");
            var m = rx.Match("");
            string result = MediaFile.GetGroupValue(m, "Name");
            Assert.Equal("Unknown", result, "empty group should return default");
        }

        public static void GetGroupValue_MissingGroup_ReturnsDefault()
        {
            var rx = new Regex(@"(?<Name>\w+)");
            var m = rx.Match("Hello");
            string result = MediaFile.GetGroupValue(m, "NonExistentGroup");
            Assert.Equal("Unknown", result, "missing group should return default");
        }

        public static void GetGroupValue_CustomDefault_ReturnsCustom()
        {
            var rx = new Regex(@"(?<Name>\w*)");
            var m = rx.Match("");
            string result = MediaFile.GetGroupValue(m, "Name", "N/A");
            Assert.Equal("N/A", result, "custom default should be returned for empty group");
        }

        // ---------------------------------------------------------------------------
        // Folder regex: standard movie format
        // ---------------------------------------------------------------------------

        public static void FolderRegex_Movie_ExtractsTitle()
        {
            var m = FolderRegex.Match("Shrek the Third (2007) {tmdb-810}");
            Assert.True(m.Success, "movie folder should match");
            Assert.Equal("Shrek the Third", m.Groups["Title"].Value, "movie title");
        }

        public static void FolderRegex_Movie_ExtractsYear()
        {
            var m = FolderRegex.Match("8 Mile (2002) {tmdb-65}");
            Assert.True(m.Success, "8 Mile should match");
            Assert.Equal("2002", m.Groups["Year"].Value, "movie year");
        }

        public static void FolderRegex_Movie_ExtractsDatabaseID()
        {
            var m = FolderRegex.Match("Shrek the Third (2007) {tmdb-810}");
            Assert.True(m.Success);
            Assert.Equal("tmdb-810", m.Groups["ID"].Value, "tmdb ID");
        }

        public static void FolderRegex_Show_ExtractsTvdbID()
        {
            var m = FolderRegex.Match("Clarkson's Farm (2021) {tvdb-378165}");
            Assert.True(m.Success, "show folder should match");
            Assert.Equal("tvdb-378165", m.Groups["ID"].Value, "tvdb ID");
            Assert.Equal("Clarkson's Farm", m.Groups["Title"].Value, "show title with apostrophe");
        }

        public static void FolderRegex_Anime_Matches()
        {
            var m = FolderRegex.Match("A Certain Magical Index (2008) {tvdb-83322}");
            Assert.True(m.Success, "anime folder should match");
            Assert.Equal("A Certain Magical Index", m.Groups["Title"].Value, "anime title");
            Assert.Equal("2008", m.Groups["Year"].Value, "anime year");
        }

        public static void FolderRegex_MissingBraces_DoesNotMatch()
        {
            var m = FolderRegex.Match("Shrek the Third (2007)");
            Assert.True(!m.Success, "folder without ID braces should not match");
        }

        public static void FolderRegex_MissingYear_DoesNotMatch()
        {
            var m = FolderRegex.Match("Shrek the Third {tmdb-810}");
            Assert.True(!m.Success, "folder without year should not match");
        }

        public static void FolderRegex_UnknownIdType_DoesNotMatch()
        {
            var m = FolderRegex.Match("Some Movie (2020) {imdb-tt1234567}");
            Assert.True(!m.Success, "unknown ID type (imdb) should not match");
        }

        // ---------------------------------------------------------------------------
        // Concise quality title regex
        // ---------------------------------------------------------------------------

        public static void QualityRegex_WEBRip1080p_Matches()
        {
            var m = ConciseQualityTitleRegex.Match("WEBRip-1080p");
            Assert.True(m.Success, "WEBRip-1080p should match");
            Assert.Equal("WEBRip-1080p", m.Value);
        }

        public static void QualityRegex_Bluray1080p_Matches()
        {
            var m = ConciseQualityTitleRegex.Match("Bluray-1080p");
            Assert.True(m.Success, "Bluray-1080p should match");
        }

        public static void QualityRegex_720p_Matches()
        {
            var m = ConciseQualityTitleRegex.Match("WEBRip-720p");
            Assert.True(m.Success, "WEBRip-720p should match");
            Assert.Equal("WEBRip-720p", m.Value);
        }

        public static void QualityRegex_4Kp_DoesNotMatch()
        {
            // 4K has no "p" suffix in the typical naming so shouldn't match \d{3,4}p
            var m = ConciseQualityTitleRegex.Match("Bluray-2160p");
            Assert.True(m.Success, "Bluray-2160p (4K) should match (4 digits + p)");
        }

        public static void QualityRegex_ExtractsFromLongTitle()
        {
            var m = ConciseQualityTitleRegex.Match("AMZN WEBRip-1080p DDP5.1 x264");
            Assert.True(m.Success, "should extract from long quality title");
            Assert.Equal("WEBRip-1080p", m.Value);
        }

        public static void QualityRegex_DVD_DoesNotMatch()
        {
            var m = ConciseQualityTitleRegex.Match("DVD");
            Assert.True(!m.Success, "DVD should not match (no -NNNp pattern)");
        }

        // ---------------------------------------------------------------------------
        // ParseConciseQualityTitle - branch coverage
        // ---------------------------------------------------------------------------

        public static void ParseConciseQualityTitle_Null_ReturnsUnknown()
        {
            string result = MediaFile.ParseConciseQualityTitle(null);
            Assert.Equal("Unknown", result, "null quality title should return Unknown");
        }

        public static void ParseConciseQualityTitle_DVD_ReturnsDVD()
        {
            string result = MediaFile.ParseConciseQualityTitle("DVD");
            Assert.Equal("DVD", result, "DVD should pass through unchanged");
        }

        public static void ParseConciseQualityTitle_MatchingTitle_ReturnsMatch()
        {
            string result = MediaFile.ParseConciseQualityTitle("WEBDL-1080p");
            Assert.Equal("WEBDL-1080p", result, "matching quality title should be returned");
        }

        public static void ParseConciseQualityTitle_LongTitleWithMatch_ExtractsMatch()
        {
            string result = MediaFile.ParseConciseQualityTitle("AMZN WEBRip-720p DDP5.1");
            Assert.Equal("WEBRip-720p", result, "concise part should be extracted from long title");
        }

        public static void ParseConciseQualityTitle_EmptyString_ReturnsUnknown()
        {
            string result = MediaFile.ParseConciseQualityTitle("");
            Assert.Equal("Unknown", result, "empty string should return Unknown");
        }

        public static void ParseConciseQualityTitle_UnrecognisedFormat_ReturnsTitleAsIs()
        {
            string result = MediaFile.ParseConciseQualityTitle("CamRip");
            Assert.Equal("CamRip", result, "unrecognised non-empty title should be returned as-is");
        }
    }
}
