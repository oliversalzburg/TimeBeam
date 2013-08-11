namespace TimeBeam {
  /// <summary>
  ///   Common interface members for elements that can serve as timeline tracks.
  /// </summary>
  public interface ITimelineTrackBase {
    /// <summary>
    ///   The name of the track.
    ///   This will be displayed alongside the track in the timeline.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///   Invoked when the user selects this item.
    /// </summary>
    void Selected();

    /// <summary>
    ///   Invoked when the item was removed from the selection.
    /// </summary>
    void Deselected();
  }
}