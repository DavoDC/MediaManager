using System;
using System.IO;

namespace MediaManager
{
    /// <summary>
    /// Tests for AgeChecker - all 5 branches via temp file injection.
    /// </summary>
    internal static class AgeCheckerTests
    {
        private static string TempPath() =>
            Path.Combine(Path.GetTempPath(), $"mm_age_{Guid.NewGuid():N}.txt");

        public static void NoFile_SetsRegenTrue_CreatesFile()
        {
            string path = TempPath();
            try
            {
                new AgeChecker(forceMirrorRegen: false, lastRunPath: path);
                Assert.True(AgeChecker.RegenMirror, "missing file -> RegenMirror should be true");
                Assert.True(File.Exists(path), "file should be created when missing");
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        public static void FileExists_RecentDate_ForceFalse_SetsRegenFalse()
        {
            string path = TempPath();
            try
            {
                // Write a date 1 day ago (within 7-day threshold)
                File.WriteAllText(path, DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                new AgeChecker(forceMirrorRegen: false, lastRunPath: path);
                Assert.True(!AgeChecker.RegenMirror, "recent mirror + force=false -> RegenMirror should be false");
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        public static void FileExists_RecentDate_ForceTrue_SetsRegenTrue()
        {
            string path = TempPath();
            try
            {
                File.WriteAllText(path, DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                new AgeChecker(forceMirrorRegen: true, lastRunPath: path);
                Assert.True(AgeChecker.RegenMirror, "recent mirror + force=true -> RegenMirror should be true");
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        public static void FileExists_OutdatedDate_SetsRegenTrue()
        {
            string path = TempPath();
            try
            {
                // Write a date 8 days ago (exceeds 7-day threshold)
                File.WriteAllText(path, DateTime.Now.AddDays(-8).ToString("yyyy-MM-dd HH:mm:ss"));
                new AgeChecker(forceMirrorRegen: false, lastRunPath: path);
                Assert.True(AgeChecker.RegenMirror, "outdated mirror -> RegenMirror should be true");
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        public static void FileExists_CorruptDate_ThrowsException()
        {
            string path = TempPath();
            try
            {
                File.WriteAllText(path, "not-a-date");
                bool threw = false;
                try { new AgeChecker(forceMirrorRegen: false, lastRunPath: path); }
                catch (FileLoadException) { threw = true; }
                Assert.True(threw, "corrupt date file should throw FileLoadException");
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }
    }
}
