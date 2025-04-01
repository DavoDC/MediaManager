using System;
using System.Collections.Generic;

namespace MediaManager
{
    /// <summary>
    /// Checks my media/mirror library against various conditions
    /// </summary>
    internal class LibChecker : Doer
    {
        /// <summary>
        /// Create library checker
        /// </summary>
        public LibChecker()
        {
            // Notify
            Console.WriteLine($"\nChecking mirror...");

            // Check anime bit depth
            CheckPropertyForUnknowns(Parser.AnimeFiles, f => f.VideoBitDepth, "bit depth");

            // Check anime audio languages
            CheckPropertyForUnknowns(Parser.AnimeFiles, f => f.AudioLanguages, "audio language");

            // Finish and print time taken
            FinishAndPrintTimeTaken();
        }

        /// <summary>
        /// Finds items where the specified property has the value "Unknown" (case-insensitive).
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="items">The collection to check.</param>
        /// <param name="propertySelector">Function to extract the property value.</param>
        /// <param name="propertyName">The name of the property.</param>
        public static void CheckPropertyForUnknowns<T>(IEnumerable<T> items, Func<T, string> propertySelector,
            string propertyName)
        {
            foreach (var item in items)
            {
                if (propertySelector(item).Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($" - '{item}' has an unknown {propertyName}!");
                }
            }
        }
    }
}