using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeBeam {
  public interface IMultiPartTimelineTrack {
    IEnumerable<ITimelineTrack> TrackElements { get; }
  }
}
