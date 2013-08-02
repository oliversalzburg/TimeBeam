using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TimeBeamTest.TestObjects;

namespace TimeBeamTest {
  public partial class TimeBeamDemoForm : Form {
    public TimeBeamDemoForm() {
      InitializeComponent();
    }

    private void TimeBeamDemoForm_Load( object sender, EventArgs e ) {
      AdjustMyLength track0 = new AdjustMyLength();
      track0.End = 50f;
      AdjustMyLength track1 = new AdjustMyLength();
      track1.Start = 20f;
      track1.End = 150f;
      timeline1.AddTrack( track0 );
      timeline1.AddTrack( track1 );
    }
  }
}
