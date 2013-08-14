using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TimeBeam;

namespace TimeBeamTest.TestObjects {
  class LinearSplineSegment : ISplineSegment {
    public string Name { get; set; }
    public float Start { get; set; }
    public float End { get; set; }
    public float StartValue { get; set; }
    public float EndValue { get; set; }
    public PointF TangentStart { get { return PointF.Empty; } set { } }
    public PointF TangentEnd { get { return PointF.Empty; } set { } }
  }
}
