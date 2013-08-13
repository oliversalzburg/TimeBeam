using System.Collections.Generic;
using TimeBeam.Helper;

namespace TimeBeam.Surrogates {
  /// <summary>
  ///   Wraps a single <see cref="ITimelineTrack" /> into an
  ///   <see cref="IMultiPartTimelineTrack" />.
  /// </summary>
  internal class SingleTrackToMultiTrackWrapper : IMultiPartTimelineTrack {
    /// <summary>
    ///   The elements within this track.
    /// </summary>
    public IEnumerable<ITimelineTrack> TrackElements {
      get { return _wrappedTrack.Yield(); }
    }

    /// <summary>
    ///   The wrapped timeline track.
    /// </summary>
    private readonly ITimelineTrack _wrappedTrack;

    /// <summary>
    ///   The name of the track.
    ///   This will be displayed alongside the track in the timeline.
    /// </summary>
    public string Name {
      get { return _wrappedTrack.Name; }
      set { _wrappedTrack.Name = value; }
    }

    /// <summary>
    ///   Construct a new SingleTrackToMultiTrackWrapper.
    /// </summary>
    /// <param name="track">The timeline track that should be wrapped.</param>
    public SingleTrackToMultiTrackWrapper( ITimelineTrack track ) {
      _wrappedTrack = track;
    }
  }
}