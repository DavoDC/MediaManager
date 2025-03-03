using System;

namespace MediaManager.Code.Modules
{
    /// <summary>
    /// A media file's metadata
    /// </summary>
    internal class MediaFile
    {
        /// <summary>
        /// Common properties
        /// </summary>
        // The media file's relative path within the library folder
        public string RelPath { get; set; }

        // The type of media this file represents (e.g. a movie, show or anime)
        public string Type { get; set; }

        // The title of the media item
        public string Title { get; set; }

        // The year the media item was released
        public string ReleaseYear { get; set; }

        // A link to this media item in a media database
        public string DatabaseLink { get; set; }

        // The file extension of this media file
        public string Extension { get; set; }

        // The custom formats
        public string CustomFormats { get; set; }

        // The quality title
        public string QualityTitle { get; set; }

        // The video dynamic range
        public string VideoDynamicRange { get; set; }

        // The video codec
        public string VideoCodec { get; set; }

        // The audio codec
        public string AudioCodec { get; set; }

        // The audio channels
        public string AudioChannels { get; set; }

        // The release group
        public string ReleaseGroup { get; set; }

        /// <summary>
        /// Show and anime specific properties
        /// </summary>
        // The kind of show season (either "Regular" or "Special")
        public string SeasonType { get; set; }

        // The season number (e.g. 00, 01 etc.)
        public string SeasonNum { get; set; }

        // The episode number (e.g. 00, 01 etc.)
        public string EpisodeNum { get; set; }

        // The episode's title
        public string EpisodeTitle { get; set; }

        /// <summary>
        /// Anime-specific properties
        /// </summary>

        // The absolute episode number (e.g. 00, 01 etc.)
        public string AbsEpisodeNum { get; set; }

        // The video's bit depth
        public string VideoBitDepth { get; set; }

        // The audio languages available
        public string AudioLanguages { get; set; }

        /// <summary>
        /// Movie-specific properties
        /// </summary>
        // The movie's edition
        public string Edition { get; set; }

        // The movie's 3D info
        public string ThreeDInfo { get; set; }

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
