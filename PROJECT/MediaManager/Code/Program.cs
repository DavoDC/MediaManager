using MediaManager.Code;

namespace MediaManager
{
    internal class Program
    {
        /// <summary>
        /// Main entry point of the program.
        /// </summary>
        private static void Main()
        {
            Console.WriteLine("\n###### Media Manager ######\n");

            // Process new media
            NewMediaProcessor nmp = new NewMediaProcessor();
        }
    }
}
