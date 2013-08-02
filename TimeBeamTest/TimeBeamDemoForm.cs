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
      AdjustMyLength sampleObject = new AdjustMyLength();
      sampleObject.End = 50f;
      timeline1.AddTrack( sampleObject );
    }
  }
}
