using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeBeam.Helper;

namespace TimeBeam.Surrogates {
  class SingleTrackToMultiTrackWrapper : IMultiPartTimelineTrack {
    public IEnumerable<ITimelineTrack> TrackElements {
      get { return _wrappedTrack.Yield(); }
    }

    private ITimelineTrack _wrappedTrack;

    public string Name { get { return _wrappedTrack.Name; } set { _wrappedTrack.Name = value; } }

    public void Selected() {
      _wrappedTrack.Selected();
    }

    public SingleTrackToMultiTrackWrapper( ITimelineTrack track ) {
      _wrappedTrack = track;
    }
  }
}
