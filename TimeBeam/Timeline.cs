using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TimeBeam.Events;
using TimeBeam.Helper;
using TimeBeam.Surrogates;
using TimeBeam.Timing;

namespace TimeBeam {
  /// <summary>
  ///   Used to host simple tracks with a start and an end.
  /// </summary>
  public partial class Timeline : TimelineBase {
    public Timeline() {
      InitializeComponent();
    }
  }
}