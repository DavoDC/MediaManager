using System.Text.RegularExpressions;

namespace MediaMirror
{
    internal class Program
    {
        /// <summary>
        /// Main function
        /// </summary>
        /// <param name="args">Arguments given to program</param>
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Media Mirror!\n");

            // Path to show
            string showPath = "E:\\Visual Media TEST GROUND\\Visual Media - Shows\\Spider-Man Test";

            // Get file list
            string[] fileList = Directory.GetFiles(showPath, "*", SearchOption.AllDirectories);

            // 1) HANDLE INFO.txt 
            string targetFile = "INFO.txt";
            // Check if INFO.txt exists
            if (fileList.Any(f => Path.GetFileName(f).Equals(targetFile, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("INFO.txt already exists. Stopping execution.");
                return;
            }

            // Filter out INFO.txt if it exists (though unlikely at this point)
            string[] otherFiles = fileList
                .Where(f => !Path.GetFileName(f).Equals(targetFile, StringComparison.OrdinalIgnoreCase))
                .Select(f => Path.GetFileName(f)) // Only file names, not full paths
                .ToArray();

            // Create INFO.txt and write the list of files
            string newFilePath = Path.Combine(showPath, targetFile);
            File.WriteAllLines(newFilePath, otherFiles);

            Console.WriteLine($"INFO.txt was not found. Created a new one with a list of {otherFiles.Length} files.");

            // 2) RENAME FILES
            foreach (string filePath in fileList)
            {
                Console.WriteLine(filePath);

                // Regular expression to match SXXEXX pattern
                //Regex regex = new Regex(@"S\d{2}E\d{2}", RegexOptions.IgnoreCase);
                //Match match = regex.Match(filePath);
                //if (match.Success)
                //{
                //    Console.WriteLine("Extracted episode: " + match.Value);
                //}
                //else
                //{
                //    Console.WriteLine("No match found.");
                //}

                string fileName = Path.GetFileName(filePath);

                //Console.WriteLine(fileName);

                //string originalName = "Your Friendly Neighborhood Spider-Man (2025) - S01E10 - If This Be My Destiny [DSNP WEBDL-720p][EAC3 Atmos 5.1][h264]-FLUX.mkv";

                // Regular expression to extract "SXXEXX - Episode Title"
                Regex regex = new Regex(@"
                    (                   # Start capturing group
                        S\d{2}E\d{2}     # Matches 'S01E10' (S followed by 2 digits, E followed by 2 digits)
                        \s-\s            # Matches ' - ' (space, hyphen, space)
                        [^[]+            # Matches everything **before** the first '[' (Episode Title)
                    )                   # End capturing group
                ", RegexOptions.IgnorePatternWhitespace); // Allows multiline regex with comments

                Match match = regex.Match(fileName);

                if (match.Success)
                {
                    string extension = Path.GetExtension(filePath);
                    string newName = match.Value.Trim() + extension;
                    //Console.WriteLine("New name: '" + newName + "'");

                    // Actually rename 
                    string directory = Path.GetDirectoryName(filePath);
                    string newPath = Path.Combine(directory, newName);
                    Console.WriteLine($"{newPath}");

                    File.Move(filePath, newPath);
                }
                else
                {
                    Console.WriteLine("Pattern not found.");
                }

                Console.WriteLine("");
            }

        }
    }
}