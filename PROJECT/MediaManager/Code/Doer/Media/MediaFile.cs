using System;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// A media file's metadata
    /// </summary>
    internal class MediaFile
    {
        // The media file's relative path within the library folder
        public string RelPath { get; set; }

        // The type of media this file represents (e.g. a movie, show or anime)
        public string Type { get; set; }

        /// <returns>A concise string representation of this file</returns>
        public override string ToString()
        {
            return "TODO";
            //return $"{Artists} - {Title}";
        }

        /// <returns>A string representation of all file properties.</returns>
        public string ToAllPropertiesString()
        {
            return "TODO";
            //return $"RelPath: {RelPath ?? "NULL"}\n" +
            //       $"Title: {Title ?? "NULL"}\n" +
            //       $"Artists: {Artists ?? "NULL"}\n" +
            //       $"PrimaryArtist: {PrimaryArtist ?? "NULL"}\n" +
            //       $"Album: {Album ?? "NULL"}\n" +
            //       $"Year: {Year ?? "NULL"}\n" +
            //       $"Genres: {Genres ?? "NULL"}\n" +
            //       $"Length: {Length ?? "NULL"}\n" +
            //       $"AlbumCoverCount: {AlbumCoverCount ?? "NULL"}\n" +
            //       $"Compilation: {Compilation ?? "NULL"}";
        }

        /// <summary>
        /// Prints all properties of this file.
        /// </summary>
        public void PrintAllProperties()
        {
            Console.WriteLine(ToAllPropertiesString());
        }
    }
}
