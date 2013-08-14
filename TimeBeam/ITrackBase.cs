namespace TimeBeam {
  /// <summary>
  ///   Common interface members for elements that can serve as timeline tracks.
  /// </summary>
  public interface ITrackBase {
    /// <summary>
    ///   The name of the track.
    ///   This will be displayed alongside the track in the timeline.
    /// </summary>
    string Name { get; set; }
  }
}