using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeBeam {
  interface ISpline : ITrack {
    new IEnumerable<ISplineSegment> TrackElements { get; }
  }
}
