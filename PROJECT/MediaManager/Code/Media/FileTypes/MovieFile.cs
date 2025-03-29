namespace MediaManager.Code.Modules
{
    /// <summary>
    /// A movie file's metadata
    /// </summary>
    internal class MovieFile : MediaFile
    {
        // The movie's edition/cut information
        public string Edition { get; set; }

        // The movie's 3D info
        public string ThreeDInfo { get; set; }
    }
}
