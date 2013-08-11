namespace TimeBeam {
  /// <summary>
  ///   Describes an item that can be placed on a track in the timeline.
  /// </summary>
  public interface ITimelineTrack {
    /// <summary>
    ///   The beginning of the item.
    /// </summary>
    float Start { get; set; }

    /// <summary>
    ///   The end of the item.
    /// </summary>
    float End { get; set; }

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