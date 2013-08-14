using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TimeBeam {
  public interface ISplineSegment : ITrackSegment {
    float StartValue { get; set; }
    float EndValue { get; set; }


    PointF TangentStart { get; set; }
    PointF TangentEnd { get; set; }
  }
}
