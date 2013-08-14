using System.Collections.Generic;
using TimeBeam.Helper;

namespace TimeBeam.Surrogates {
  /// <summary>
  ///   Wraps a single <see cref="ITrackSegment" /> into an
  ///   <see cref="ITrack" />.
  /// </summary>
  internal class SingleElementToTrackWrapper : ITrack {
    /// <summary>
    ///   The elements within this track segment.
    /// </summary>
    public IEnumerable<ITrackSegment> TrackElements {
      get { return _wrappedTrackSegment.Yield(); }
    }

    /// <summary>
    ///   The wrapped timeline track segment.
    /// </summary>
    private readonly ITrackSegment _wrappedTrackSegment;

    /// <summary>
    ///   The name of the track.
    ///   This will be displayed alongside the track in the timeline.
    /// </summary>
    public string Name {
      get { return _wrappedTrackSegment.Name; }
      set { _wrappedTrackSegment.Name = value; }
    }

    /// <summary>
    ///   Construct a new SingleElementToTrackWrapper.
    /// </summary>
    /// <param name="trackSegment">The timeline trackSegment that should be wrapped.</param>
    public SingleElementToTrackWrapper( ITrackSegment trackSegment ) {
      _wrappedTrackSegment = trackSegment;
    }
  }
}