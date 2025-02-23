using System;
using System.IO;

namespace MediaManager
{
    internal class Program
    {
        //// CONSTANTS/SETTINGS

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
                Console.WriteLine("\n###### Media Manager ######\n");

                // 0) Proces new media
                NewMediaProcessor nmp = new NewMediaProcessor();

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
    }
}