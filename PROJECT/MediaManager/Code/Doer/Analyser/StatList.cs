﻿using System;
using System.Collections.Generic;
using System.Linq;
using StringIntFreqDist = System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<string, int>>;

namespace MediaManager
{
    /// <summary>
    /// Calculates, stores and displays frequency statistics for a specific property of items of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items (e.g., MediaFile, MovieFile, etc.).</typeparam>
    internal class StatList<T>
    {
        // Name of these statistics
        private string name;

        // The sorted frequency distribution used to calculate these statistics
        private StringIntFreqDist sortedFreqDist;
        public StringIntFreqDist SortedFreqDist
        {
            get => sortedFreqDist;
        }

        // Underlying data structure - a list of statistics objects
        private List<Statistic> statList;

        /// <summary>
        /// Create a StatList object from media files and a property extractor
        /// </summary>
        /// <param name="name">The name of the statistics category</param>
        /// <param name="mediaFilesIn">The list of media files inputted</param>
        /// <param name="func">Function that returns the property</param>
        public StatList(string name, List<T> mediaFilesIn, Func<T, string> func)
        {
            // Save name
            this.name = name;

            // Calculate sorted frequency distribution
            sortedFreqDist = GetSortedFreqDist(mediaFilesIn, func);

            // Initialise underlying statistics list
            InitStatList();
        }

        /// <summary>
        /// Create a StatList object from a sorted frequency distribution
        /// </summary>
        /// <param name="name">The name of the statistics category</param>
        /// <param name="sortedFreqDistIn">The sorted freq dist inputted</param>
        public StatList(string name, StringIntFreqDist sortedFreqDistIn)
        {
            // Save name
            this.name = name;

            // Save sorted frequency distribution
            sortedFreqDist = sortedFreqDistIn;

            // Initialise underlying statistics list
            InitStatList();
        }

        /// <summary>
        /// Initialise statistics list using sorted freq dist
        /// </summary>
        private void InitStatList()
        {
            // Sum up total number of items (i.e. sum of occurrences)
            int totalItems = sortedFreqDist.Sum(pair => pair.Value);

            // Convert each frequency pair to a Statistic and save to list
            statList = new List<Statistic>();
            foreach (var freqPair in sortedFreqDist)
            {
                statList.Add(new Statistic(freqPair.Key, freqPair.Value, totalItems));
            }
        }

        /// <summary>
        /// Generates a frequency distribution of sub-properties extracted from a list of items.
        /// </summary>
        /// <param name="items">The list of items inputted.</param>
        /// <param name="func">A function that extracts a property from a given item.</param>
        /// <returns>List of key-value pairs (property-frequency_count pairs), sorted in descending order by count.</returns>
        public static StringIntFreqDist GetSortedFreqDist(List<T> items, Func<T, string> func)
        {
            // A dictionary that maps each unique item to how many there are
            var itemVariants = new Dictionary<string, int>();

            // For each file
            foreach (var file in items)
            {
                // Extract property using the given function
                string property = func(file);

                // Do null check
                if(property == null)
                {
                    Prog.PrintErrMsg("NULL property was retrieved!");
                    break;
                }

                // If property is in dictionary
                if (itemVariants.ContainsKey(property))
                {
                    // Increment its value
                    itemVariants[property]++;
                }
                else
                {
                    // Otherwise if not in dictionary, add it
                    // NOTE: REQUIRED to prevent 'KeyNotFoundException' errors
                    itemVariants[property] = 1;
                }
            }

            // Sort the dictionary by count in descending order,
            // and return as an IOrderedEnumerable of KeyValuePairs
            return itemVariants.OrderByDescending(pair => pair.Value);
        }

        /// <summary>
        /// Print out this statistics list
        /// </summary>
        /// <param name="cutoff">Percentage cutoff for statistics</param>
        public void Print(double cutoff = 0, string comment = "")
        {
            // Print heading and columns
            Console.WriteLine($"\n# {name} Statistics {comment}");
            Statistic.PrintColumns("#", "%", name, "Occurrences");

            // Print out every statistics object
            for (int i = 0; i < statList.Count; i++)
            {
                statList[i].Print(i + 1, cutoff);
            }
        }

        /// <summary>
        /// Get statistics on how many files are in each time/decade period
        /// </summary>
        /// <param name="yearStats">The normal year statistics.</param>
        /// <returns>A frequency distribution of decade periods.</returns>
        public static StringIntFreqDist GetDecadeFreqDist(StatList<T> yearStats)
        {
            // Extract year frequency distribution
            var yearFreqDist = yearStats.SortedFreqDist;

            // Process year frequency data into sorted decade groups with transformed keys
            return yearFreqDist
                .GroupBy(yearPair => GetDecade(yearPair.Key.ToString())) // Group by string decades
                .Select(group => new KeyValuePair<string, int>(
                    group.Key,  // Use pre-formatted decade key
                    group.Sum(pair => pair.Value))) // Calculate sum
                .OrderByDescending(pair => pair.Value); // Sort by value
        }

        /// <summary>
        /// Calculates the starting year of the decade for a given year.
        /// </summary>
        /// <param name="year">The year as a string, or "Missing" if the file didn't have it.</param>
        /// <returns>The starting year of the decade as a string with an 's' (e.g., "1990s" for 1995).</returns>
        private static string GetDecade(string year)
        {
            // If year parsing successful
            if (int.TryParse(year, out int yearNum))
            {
                // Return decade with "s" on the end
                return $"{(yearNum / 10) * 10}s";
            }
            else
            {
                // Otherwise throw error with message
                throw new FormatException($"Cannot parse year string: '{year}'");
            }
        }
    }
}