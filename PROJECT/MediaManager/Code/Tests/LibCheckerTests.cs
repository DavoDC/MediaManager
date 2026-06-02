namespace MediaManager
{
    /// <summary>
    /// Tests for LibChecker string-stripping helpers.
    /// These are the core logic of CheckPropertiesAgainstFilename - made internal for testability.
    /// </summary>
    internal static class LibCheckerTests
    {
        // ---------------------------------------------------------------------------
        // RemovePrefix
        // ---------------------------------------------------------------------------

        public static void RemovePrefix_MatchingPrefix_Stripped()
        {
            string result = LibChecker.RemovePrefix("Hello World", "Hello");
            Assert.Equal("World", result, "matching prefix should be stripped and trimmed");
        }

        public static void RemovePrefix_NoMatch_InputUnchanged()
        {
            string result = LibChecker.RemovePrefix("Hello World", "Goodbye");
            Assert.Equal("Hello World", result, "non-matching prefix should leave input unchanged");
        }

        public static void RemovePrefix_ExactMatch_ReturnsEmpty()
        {
            string result = LibChecker.RemovePrefix("Hello", "Hello");
            Assert.Equal("", result, "exact match should return empty string");
        }

        public static void RemovePrefix_WhitespaceAfterPrefix_Trimmed()
        {
            string result = LibChecker.RemovePrefix("prefix   rest", "prefix");
            Assert.Equal("rest", result, "whitespace after stripped prefix should be trimmed");
        }

        public static void RemovePrefix_EmptyPrefix_InputUnchanged()
        {
            // Empty prefix always matches at start - StartsWith("") is always true
            // Result: strip nothing, trim "Hello World" -> "Hello World"
            string result = LibChecker.RemovePrefix("Hello World", "");
            Assert.Equal("Hello World", result, "empty prefix strips nothing, output trimmed only");
        }

        // ---------------------------------------------------------------------------
        // RemoveBracedPrefix
        // ---------------------------------------------------------------------------

        public static void RemoveBracedPrefix_MatchingValue_Stripped()
        {
            string result = LibChecker.RemoveBracedPrefix("{tmdb-810} rest", "tmdb-810");
            Assert.Equal("rest", result, "braced prefix should be stripped");
        }

        public static void RemoveBracedPrefix_NoMatch_InputUnchanged()
        {
            string result = LibChecker.RemoveBracedPrefix("{tmdb-810} rest", "tmdb-999");
            Assert.Equal("{tmdb-810} rest", result, "non-matching braced prefix should be unchanged");
        }

        public static void RemoveBracedPrefix_WrongDelimiter_InputUnchanged()
        {
            string result = LibChecker.RemoveBracedPrefix("[tmdb-810] rest", "tmdb-810");
            Assert.Equal("[tmdb-810] rest", result, "bracketed value should not match braced removal");
        }

        // ---------------------------------------------------------------------------
        // RemoveBracketedPrefix
        // ---------------------------------------------------------------------------

        public static void RemoveBracketedPrefix_MatchingValue_Stripped()
        {
            string result = LibChecker.RemoveBracketedPrefix("[WEBRip-1080p] rest", "WEBRip-1080p");
            Assert.Equal("rest", result, "bracketed prefix should be stripped");
        }

        public static void RemoveBracketedPrefix_NoMatch_InputUnchanged()
        {
            string result = LibChecker.RemoveBracketedPrefix("[WEBRip-720p] rest", "WEBRip-1080p");
            Assert.Equal("[WEBRip-720p] rest", result, "non-matching value should leave input unchanged");
        }

        public static void RemoveBracketedPrefix_WrongDelimiter_InputUnchanged()
        {
            string result = LibChecker.RemoveBracketedPrefix("{WEBRip-1080p} rest", "WEBRip-1080p");
            Assert.Equal("{WEBRip-1080p} rest", result, "braced value should not match bracketed removal");
        }

        // ---------------------------------------------------------------------------
        // Chained removal (simulates real filename stripping pipeline)
        // ---------------------------------------------------------------------------

        public static void ChainedRemoval_MovieFilenamePattern_StripsCorrectly()
        {
            // Simulate: "Shrek (2001) {tmdb-808} [WEBRip-1080p] [EAC3 5.1] [x265]-GROUP.mkv"
            string filename = "Shrek (2001) {tmdb-808} [WEBRip-1080p] [EAC3 5.1] [x265]-GROUP.mkv";

            string s = LibChecker.RemovePrefix(filename, "Shrek (2001)");
            s = LibChecker.RemoveBracedPrefix(s, "tmdb-808");
            s = LibChecker.RemoveBracketedPrefix(s, "WEBRip-1080p");
            s = LibChecker.RemoveBracketedPrefix(s, "EAC3 5.1");
            s = LibChecker.RemoveBracketedPrefix(s, "x265");
            s = LibChecker.RemovePrefix(s, "-GROUP");
            s = LibChecker.RemovePrefix(s, ".mkv");

            Assert.Equal("", s, "fully stripped movie filename should leave empty string");
        }
    }
}
