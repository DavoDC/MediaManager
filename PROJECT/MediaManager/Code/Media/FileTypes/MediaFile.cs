﻿using System;

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

        /// <returns>A string representation of all file properties.</returns>
        public string ToAllPropertiesString()
        {
            return $"RelPath: {RelPath ?? "NULL"}\n" +
                   $"Type: {Type ?? "NULL"}\n" +
                   $"Title: {Title ?? "NULL"}\n" +
                   $"ReleaseYear: {ReleaseYear ?? "NULL"}\n" +
                   $"DatabaseLink: {DatabaseLink ?? "NULL"}\n" +
                   $"Extension: {Extension ?? "NULL"}\n" +
                   $"CustomFormats: {CustomFormats ?? "NULL"}\n" +
                   $"QualityTitle: {QualityTitle ?? "NULL"}\n" +
                   $"VideoDynamicRange: {VideoDynamicRange ?? "NULL"}\n" +
                   $"VideoCodec: {VideoCodec ?? "NULL"}\n" +
                   $"AudioCodec: {AudioCodec ?? "NULL"}\n" +
                   $"AudioChannels: {AudioChannels ?? "NULL"}\n" +
                   $"ReleaseGroup: {ReleaseGroup ?? "NULL"}";
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