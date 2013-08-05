using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TimeBeam;

namespace TimeBeamTest.TestObjects {
  class AdjustMyLength : ITimelineTrack {
    public float Start { get; set; }
    public float End { get; set; }
    public string Name { get; set; }

    public void Selected() {
      Debug.WriteLine( "Selected" );
    }
  }
}
