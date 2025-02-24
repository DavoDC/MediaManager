using System;
using System.IO;

namespace MediaManager
{
    internal class Program
    {
        ////// CONSTANTS
        // Main media folder
        public static readonly string mediaFolderPath = "E:\\Visual Media";

        // Media folders
        public static readonly string animeFolderPath = mediaFolderPath + "\\Visual Media - Anime";
        public static readonly string moviesFolderPath = mediaFolderPath + "\\Visual Media - Movies";
        public static readonly string showsFolderPath = mediaFolderPath + "\\Visual Media - Shows";
        public static readonly string universesFolderPath = mediaFolderPath + "\\Visual Media - Universes";

        // Info file
        public static readonly string infoFileName = "INFO.txt";

        // The path back to the project folder
        private static readonly string projectPath = "..\\..\\..\\";
        public static string ProjectPath { get => projectPath; }

        // The mirror folder name
        private static readonly string mirrorFolder = "MEDIA_MIRROR";
        public static string MirrorFolder { get => mirrorFolder; }

        // The path to the mirror folder relative to program executable
        private static readonly string relMirrorPath = projectPath + "..\\..\\MediaMirror\\" + mirrorFolder;

        /// <summary>
        /// Main function
        /// </summary>
        /// <param name="args">Arguments given to program</param>
        static void Main(string[] args)
        {
            try
            {
                // Start message
                Console.WriteLine("\n###### Media Manager ######");

                // 0) Process new media
                PrintCheckingFolderMsg(NewMediaProcessor.integrateFolderPath);
                NewMediaProcessor nmp = new NewMediaProcessor();

                // TODO
                //PrintCheckingFolderMsg(animeFolderPath);
                //PrintCheckingFolderMsg(moviesFolderPath);
                //PrintCheckingFolderMsg(showsFolderPath);
                //PrintCheckingFolderMsg(universesFolderPath);

                // 1) Check the age of the mirror
                //AgeChecker ac = new AgeChecker();

                // 2) Create mirror of audio folder
                // Set mirror path relative to program executable
                //string programDir = AppDomain.CurrentDomain.BaseDirectory;
                //string mirrorPath = Path.GetFullPath(Path.Combine(programDir, relMirrorPath));
                // Note: Files created at this stage just contain paths to the actual file, not metadata info.
                //Reflector r = new Reflector(mirrorPath, ac.recreateMirror);

                // 3) Parse metadata into XML files and tag list
                // Note: The file contents get overwritten with actual XML content in this stage.
                //Parser p = new Parser(mirrorPath);

                // 4) Analyse metadata and print statistics
                //Analyser a = new Analyser(p.audioTags);

                // 5) Do audio library organisational/metadata checks
                //LibChecker lc = new LibChecker(p.audioTags);

                // Print total time
                TimeSpan totalTime = nmp.ExecutionTime;
                //ac.ExecutionTime + r.ExecutionTime + p.ExecutionTime;
                //totalTime += a.ExecutionTime + lc.ExecutionTime;
                Console.WriteLine("\nTotal time taken: " + Doer.ConvertTimeSpanToString(totalTime));

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
        /// Print a message that the folder at the given path is being checked
        /// </summary>
        public static void PrintCheckingFolderMsg(string folderPath)
        {
            Console.WriteLine($"\nChecking '{Path.GetFileName(folderPath)}' folder...");
        }

        /// <summary>
        /// Print an error message
        /// </summary>
        public static void PrintErrMsg(string errorMsg)
        {
            Console.WriteLine($"ERROR: {errorMsg}");
        }
    }
}