using System.Collections.Generic;

namespace TimeBeam {
  /// <summary>
  ///   A timeline track that contains multiple parts.
  /// </summary>
  public interface ITrack : ITrackBase {
    /// <summary>
    ///   The elements within this track.
    /// </summary>
    IEnumerable<ITrackSegment> TrackElements { get; }
  }
}