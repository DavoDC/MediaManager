﻿using System;
using System.IO;

namespace MediaManager
{
    /// <summary>
    /// Checks whether the mirror was generated too long ago.
    /// </summary>
    internal class AgeChecker : Doer
    {
        /// <summary>
        /// True if the mirror should be regenerated
        /// </summary>
        public bool recreateMirror { get; set; }

        // The path to the last run info file
        private static readonly string lastRunInfoFilePath = Prog.MirrorRepoPath + "LastRunInfo.txt";

        // If the mirror is older than this amount of time, it is considered outdated
        private static readonly TimeSpan ageThreshold = TimeSpan.FromDays(7);

        /// <summary>
        /// Construct an age checker
        /// </summary>
        public AgeChecker() 
        {
            // Notify
            Console.WriteLine($"\nChecking age of mirror...");

            // Get current date
            DateTime curDate = DateTime.Now;
            PrintDate("Now", curDate);

            // If the last run info file doesn't exist
            if (!File.Exists(lastRunInfoFilePath))
            {
                // Create with it the current date
                File.WriteAllText(lastRunInfoFilePath, GetStrFromDate(curDate));

                // Schedule recreation and stop
                recreateMirror = true;
                return;
            }

            //// Else if the file exists:
            // Parse and process date
            DateTime mirrorCreationDate;
            if (DateTime.TryParse(File.ReadAllText(lastRunInfoFilePath), out mirrorCreationDate))
            {
                ProcessDates(curDate, mirrorCreationDate);
            }
            else
            {
                string parseErr = "\nERROR: Cannot parse date in: " + lastRunInfoFilePath;
                throw new FileLoadException(parseErr);
            }
        }


        /// <param name="curDate">The current date</param>
        /// <param name="mirrorCreationDate">The date the mirror was last created</param>
        private void ProcessDates(DateTime curDate, DateTime mirrorCreationDate)
        {
            // Print mirror creation date
            PrintDate("MirrorCreation", mirrorCreationDate);

            // Determine age of mirror and print
            TimeSpan mirrorAge = curDate.Subtract(mirrorCreationDate);
            PrintDate("MirrorAge", mirrorAge);

            // If the mirror's age exceeds the threshold, regenerate it
            recreateMirror = mirrorAge > ageThreshold;

            // If mirror will be regenerated
            if (recreateMirror)
            {
                // Notify and update date in file
                Console.WriteLine(" - Mirror is outdated, will regenerate!");
                File.WriteAllText(lastRunInfoFilePath, GetStrFromDate(curDate));
            }
            else
            {
                // Else if not, notify
                Console.WriteLine(" - Mirror was created recently, no regeneration needed!");
            }

            // Finish and print time taken
            FinishAndPrintTimeTaken();
        }


        /// <param name="desc">A description of the date/time</param>
        /// <param name="date">The date/time object</param>
        private void PrintDate(string desc, object timeVal)
        {
            string paddedDesc = (" - DateTime." + desc).PadRight(27);
            Console.WriteLine($"{paddedDesc}   {GetStrFromDate(timeVal)}");
        }


        /// <summary>
        /// Format a DateTime or TimeSpan as a string.
        /// </summary>
        /// <param name="timeVal">The DateTime or TimeSpan object</param>
        /// <returns>The formatted string</returns>
        private string GetStrFromDate(object timeVal)
        {
            if (timeVal is DateTime)
            {
                DateTime dateTime = (DateTime)timeVal;
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else if (timeVal is TimeSpan)
            {
                TimeSpan ts = (TimeSpan)timeVal;
                return $"{ts.Days}d, {ts.Hours}h, {ts.Minutes}m, {ts.Seconds}s";
            }
            else
            {
                throw new ArgumentException("Unsupported type given to function");
            }
        }
    }
}
