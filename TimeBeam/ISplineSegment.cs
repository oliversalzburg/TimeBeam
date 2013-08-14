using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeBeam {
  interface ISplineSegment : ITrackSegment {
    float StartValue { get; set; }
    float EndValue { get; set; }
  }
}
