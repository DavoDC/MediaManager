﻿using System;
using System.IO;

namespace MediaManager
{
    /// <summary>
    /// The main class where program execution begins.
    /// </summary>
    internal class Prog
    {
        ////// CONSTANTS //////

        /// <summary>
        /// Paths
        /// </summary>
        // The relative path from the executable back to the project folder
        private static readonly string projectPath = "..\\..\\..\\";
        public static string ProjectPath { get => projectPath; }

        // The relative path to the mirror repo (assumed to be next to this repo)
        private static readonly string mirrorRepoPath = ProjectPath + "..\\..\\MediaMirror\\";
        public static string MirrorRepoPath { get => mirrorRepoPath; }

        // The mirror folder name
        private static readonly string mirrorFolderName = "MEDIA_MIRROR";
        public static string MirrorFolderName { get => mirrorFolderName; }

        // The relative path to the mirror folder
        private static readonly string mirrorFolderPath = $"{MirrorRepoPath}\\{MirrorFolderName}";
        public static string RelMirrorFolderPath { get => mirrorFolderPath; }

        // The absolute path to the mirror folder
        // (e.g. "C:\Users\David\GitHubRepos\MediaMirror\MEDIA_MIRROR")
        public static string AbsMirrorFolderPath { get; set; }

        // Main media folder
        private static readonly string mediaFolderPath = "E:\\Visual_Media";
        public static string MediaFolderPath { get => mediaFolderPath; }

        // The anime folder name
        private static readonly string animeFolderName = "Anime";
        public static string AnimeFolderName { get => animeFolderName; }

        // The movie folder name
        private static readonly string movieFolderName = "Movies";
        public static string MovieFolderName { get => movieFolderName; }

        // The shows folder name
        private static readonly string showFolderName = "Shows";
        public static string ShowFolderName { get => showFolderName; }

        /// <summary>
        /// The program's entry point, automatically invoked when the application starts. 
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Start message
                Console.WriteLine("\n###### Media Manager ######");

                // Get the path of the executable
                string progExecPath = AppDomain.CurrentDomain.BaseDirectory;

                // Set the absolute mirror path relative to the executable
                AbsMirrorFolderPath = Path.GetFullPath(Path.Combine(progExecPath, RelMirrorFolderPath));

                // Toggle forcing mirror to be regenerated (e.g. during development)
                bool forceMirrorRegen = false;
                //bool forceMirrorRegen = true;

                // 1) Check the age of the mirror
                AgeChecker ac = new AgeChecker(forceMirrorRegen);

                // 2) Create mirror of media folder
                // Note: XML files created at this stage just contain paths to the actual file, not metadata info.
                Reflector refl = new Reflector();

                // 3) Parse metadata into XML files and media file objects
                // Note: The file contents get overwritten with actual XML content in this stage.
                Parser p = new Parser();

                // 4) Analyse metadata and print statistics
                Analyser a = new Analyser();

                // 5) Check that values saved are correct
                LibChecker lc = new LibChecker();

                // Print total time
                Doer.PrintTotalTimeTaken();

                // Finish message
                Console.WriteLine("\nFinished!\n");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"\nEXCEPTION ENCOUNTERED");
                Console.WriteLine($"\nMessage: {ex.Message}");
                Console.WriteLine($"\nStack Trace: \n{ex.StackTrace}");
                Console.WriteLine("\n");
                Environment.Exit(123);
            }
        }

        /// <summary>
        /// Print an error message
        /// </summary>
        public static void PrintErrMsg(string errorMsg)
        {
            Console.WriteLine($"\nERROR: {errorMsg}");
        }
    }
}