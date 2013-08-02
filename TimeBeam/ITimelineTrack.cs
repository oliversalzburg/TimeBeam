using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeBeam {
  public interface ITimelineTrack {
    float Start { get; set; }
    float End { get; set; }
  }
}
