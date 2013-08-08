using System.Collections.Generic;
using System.Diagnostics;
using TimeBeam;

namespace TimeBeamTest.TestObjects {
  internal class AdjustMyParts : IMultiPartTimelineTrack {
    public IEnumerable<ITimelineTrack> TrackElements {
      get { return Parts; }
    }

    private List<AdjustMyLength> Parts = new List<AdjustMyLength>();

    public string Name { get; set; }

    public void Selected() {
      Debug.WriteLine( "Selected: " + this );
    }

    public AdjustMyParts() {
      for( int partIndex = 0; partIndex < 10; partIndex++ ) {
        AdjustMyLength part = new AdjustMyLength {
          Start = 50 * partIndex,
          Name = "Part " + partIndex
        };
        part.End = part.Start + 20;
        Parts.Add( part );
      }
    }

    public override string ToString() {
      return string.Format( "Name: {0}", Name );
    }
  }
}