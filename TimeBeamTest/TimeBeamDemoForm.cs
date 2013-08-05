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
      List<AdjustMyLength> tracks = new List<AdjustMyLength> {
        new AdjustMyLength{Start=0,  End=50, Name="Position X"},
        new AdjustMyLength{Start=20, End=150,Name="Position Y"},
        new AdjustMyLength{Start=220,End=550,Name="Position Z"},
        new AdjustMyLength{Start=520,End=650,Name="Rotation X"},
        new AdjustMyLength{Start=100,End=150,Name="Position Y"},
        new AdjustMyLength{Start=120,End=250,Name="Position Z"},
        new AdjustMyLength{Start=320,End=650,Name="Alpha"},
        new AdjustMyLength{Start=620,End=750,Name="Visible"}
      };

      foreach( AdjustMyLength track in tracks ) {
        timeline1.AddTrack( track );
      }
    }
  }
}
