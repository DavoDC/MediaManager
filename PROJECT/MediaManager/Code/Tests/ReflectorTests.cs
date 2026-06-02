namespace MediaManager
{
    /// <summary>
    /// Tests for Reflector public static pure methods.
    /// </summary>
    internal static class ReflectorTests
    {
        // ---------------------------------------------------------------------------
        // SanitiseFilename
        // ---------------------------------------------------------------------------

        public static void SanitiseFilename_ValidAscii_Unchanged()
        {
            string result = Reflector.SanitiseFilename("Shrek the Third (2007)");
            Assert.Equal("Shrek the Third (2007)", result, "valid ASCII filename should be unchanged");
        }

        public static void SanitiseFilename_InvalidChars_ReplacedWithUnderscore()
        {
            // '?' is an invalid file name character
            string result = Reflector.SanitiseFilename("file?name");
            Assert.Equal("file_name", result, "invalid char should become underscore");
        }

        public static void SanitiseFilename_MultipleInvalidChars_AllReplaced()
        {
            // '*' and '?' are both invalid
            string result = Reflector.SanitiseFilename("file*?.mkv");
            Assert.True(!result.Contains("*") && !result.Contains("?"),
                "all invalid chars should be replaced");
        }

        public static void SanitiseFilename_DashAndParens_Preserved()
        {
            // Common in media filenames
            string result = Reflector.SanitiseFilename("8 Mile (2002) [WEBRip-1080p]");
            Assert.Equal("8 Mile (2002) [WEBRip-1080p]", result, "dashes, parens, brackets should be preserved");
        }

        public static void SanitiseFilename_EmptyString_ReturnsEmpty()
        {
            string result = Reflector.SanitiseFilename("");
            Assert.Equal("", result, "empty string should return empty string");
        }

        // ---------------------------------------------------------------------------
        // FixLongPath
        // ---------------------------------------------------------------------------

        public static void FixLongPath_ShortPath_Unchanged()
        {
            string shortPath = @"C:\Users\David\short.mkv";
            string result = Reflector.FixLongPath(shortPath);
            Assert.Equal(shortPath, result, "short path should be returned unchanged");
        }

        public static void FixLongPath_ForcedShortPath_GetsUNCPrefix()
        {
            string shortPath = @"C:\Users\David\short.mkv";
            string result = Reflector.FixLongPath(shortPath, force: true);
            Assert.True(result.StartsWith(@"\\?\"), "forced path should get UNC prefix");
        }

        public static void FixLongPath_LongPath_GetsUNCPrefix()
        {
            // Build a path that exceeds 250 chars
            string longPath = @"C:\Users\David\" + new string('A', 240) + ".mkv";
            string result = Reflector.FixLongPath(longPath);
            Assert.True(result.StartsWith(@"\\?\"), "long path should get UNC prefix");
        }

        public static void FixLongPath_ForwardSlashesNormalized()
        {
            // When prefix is added, forward slashes become backslashes
            string path = @"C:\Users\David\short.mkv";
            string result = Reflector.FixLongPath(path, force: true);
            Assert.True(!result.Contains("/"), "no forward slashes after fix");
        }

        // ---------------------------------------------------------------------------
        // GetRelativePath
        // ---------------------------------------------------------------------------

        public static void GetRelativePath_SameDirectory_ReturnsFilenameOnly()
        {
            // URI-based: trailing slash on base dir is required
            string result = Reflector.GetRelativePath(
                @"C:\Media\Movies\",
                @"C:\Media\Movies\Shrek.mkv");
            Assert.Equal("Shrek.mkv", result, "file in same dir should return filename only");
        }

        public static void GetRelativePath_SubDirectory_ReturnsRelativePath()
        {
            string result = Reflector.GetRelativePath(
                @"C:\Media\",
                @"C:\Media\Movies\Shrek.mkv");
            Assert.Equal("Movies/Shrek.mkv", result, "subdirectory file should include folder");
        }
    }
}
