using System.Collections.Generic;

namespace TimeBeam {
  /// <summary>
  ///   A timeline track that contains multiple parts.
  /// </summary>
  public interface IMultiPartTimelineTrack {
    /// <summary>
    ///   The elements within this track.
    /// </summary>
    IEnumerable<ITimelineTrack> TrackElements { get; }

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