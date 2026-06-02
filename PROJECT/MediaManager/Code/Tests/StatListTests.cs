using System.Collections.Generic;
using System.Linq;

namespace MediaManager
{
    /// <summary>
    /// Tests for StatList static methods - pure computation, no I/O or media file objects.
    /// </summary>
    internal static class StatListTests
    {
        // ---------------------------------------------------------------------------
        // GetSortedFreqDist
        // ---------------------------------------------------------------------------

        public static void GetSortedFreqDist_EmptyList_ReturnsEmpty()
        {
            var result = StatList<string>.GetSortedFreqDist(new List<string>(), s => s);
            Assert.True(!result.Any(), "empty input should produce empty distribution");
        }

        public static void GetSortedFreqDist_SingleItem_CountIsOne()
        {
            var items = new List<string> { "Action" };
            var result = StatList<string>.GetSortedFreqDist(items, s => s).ToList();
            Assert.True(result.Count == 1, "should have one entry");
            Assert.Equal("Action", result[0].Key, "key should match item");
            Assert.True(result[0].Value == 1, "count should be 1");
        }

        public static void GetSortedFreqDist_DuplicateItems_CountsAccumulate()
        {
            var items = new List<string> { "Comedy", "Drama", "Comedy", "Comedy" };
            var result = StatList<string>.GetSortedFreqDist(items, s => s).ToList();
            var comedy = result.First(r => r.Key == "Comedy");
            Assert.True(comedy.Value == 3, "Comedy should have count 3");
            var drama = result.First(r => r.Key == "Drama");
            Assert.True(drama.Value == 1, "Drama should have count 1");
        }

        public static void GetSortedFreqDist_SortedDescending()
        {
            var items = new List<string> { "A", "B", "A", "C", "A", "B" };
            var result = StatList<string>.GetSortedFreqDist(items, s => s).ToList();
            Assert.Equal("A", result[0].Key, "highest count first");
            Assert.True(result[0].Value == 3, "A count should be 3");
            Assert.True(result[1].Value == 2, "B count should be 2");
            Assert.True(result[2].Value == 1, "C count should be 1");
        }

        public static void GetSortedFreqDist_PropertyExtractor_UsesLambda()
        {
            // Use object list with lambda extracting a field
            var items = new List<string> { "action-2020", "drama-2020", "action-2021" };
            var result = StatList<string>.GetSortedFreqDist(items, s => s.Split('-')[0]).ToList();
            var action = result.First(r => r.Key == "action");
            Assert.True(action.Value == 2, "action extracted by lambda should count 2");
            var drama = result.First(r => r.Key == "drama");
            Assert.True(drama.Value == 1, "drama extracted by lambda should count 1");
        }

        // ---------------------------------------------------------------------------
        // GetDecadeFreqDist
        // ---------------------------------------------------------------------------

        public static void GetDecadeFreqDist_GroupsByDecade()
        {
            var items = new List<string> { "2001", "2005", "2010", "2001" };
            var yearStatList = new StatList<string>("Year", items, s => s);
            var decades = StatList<string>.GetDecadeFreqDist(yearStatList).ToList();

            var d2000 = decades.FirstOrDefault(d => d.Key == "2000s");
            Assert.True(d2000.Value == 3, "2001 x2 + 2005 x1 should sum to 3 in 2000s");

            var d2010 = decades.FirstOrDefault(d => d.Key == "2010s");
            Assert.True(d2010.Value == 1, "2010 should appear in 2010s");
        }

        public static void GetDecadeFreqDist_SortedDescending()
        {
            var items = new List<string> { "1990", "2000", "2001", "2002" };
            var yearStatList = new StatList<string>("Year", items, s => s);
            var decades = StatList<string>.GetDecadeFreqDist(yearStatList).ToList();
            // 2000s has 3 (2000, 2001, 2002); 1990s has 1
            Assert.Equal("2000s", decades[0].Key, "decade with highest count should be first");
        }

        public static void GetDecadeFreqDist_SameDecade_SingleGroup()
        {
            var items = new List<string> { "1995", "1997", "1999" };
            var yearStatList = new StatList<string>("Year", items, s => s);
            var decades = StatList<string>.GetDecadeFreqDist(yearStatList).ToList();
            Assert.True(decades.Count == 1, "all years in same decade should produce one group");
            Assert.Equal("1990s", decades[0].Key, "decade should be 1990s");
            Assert.True(decades[0].Value == 3, "total count should be 3");
        }

        public static void GetDecadeFreqDist_EarlyYear_CorrectDecade()
        {
            var items = new List<string> { "1968" };
            var yearStatList = new StatList<string>("Year", items, s => s);
            var decades = StatList<string>.GetDecadeFreqDist(yearStatList).ToList();
            Assert.Equal("1960s", decades[0].Key, "1968 should fall in 1960s");
        }
    }
}
