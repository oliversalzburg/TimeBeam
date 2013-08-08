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
  }
}