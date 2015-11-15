using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TimeBeam.Scrollbar;

namespace TimeBeam {
  partial class TimelineBase {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose( bool disposing ) {
      if( disposing && ( components != null ) ) {
        components.Dispose();
      }
      Application.RemoveMessageFilter(this);
      base.Dispose( disposing );
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.ScrollbarV = new VerticalScrollbar();
      this.ScrollbarH = new HorizontalScrollbar();
      this.SuspendLayout();
      // 
      // ScrollbarV
      // 
      this.ScrollbarV.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) 
            | AnchorStyles.Right)));
      this.ScrollbarV.BackgroundColor = Color.Black;
      this.ScrollbarV.ForegroundColor = Color.Gray;
      this.ScrollbarV.Location = new Point(791, 0);
      this.ScrollbarV.Max = 100;
      this.ScrollbarV.Min = 0;
      this.ScrollbarV.Name = "ScrollbarV";
      this.ScrollbarV.Size = new Size(10, 180);
      this.ScrollbarV.TabIndex = 1;
      this.ScrollbarV.Value = 0;
      this.ScrollbarV.Scroll += new EventHandler<ScrollEventArgs>(this.ScrollbarVScroll);
      // 
      // ScrollbarH
      // 
      this.ScrollbarH.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) 
            | AnchorStyles.Right)));
      this.ScrollbarH.BackgroundColor = Color.Black;
      this.ScrollbarH.ForegroundColor = Color.Gray;
      this.ScrollbarH.Location = new Point(0, 190);
      this.ScrollbarH.Max = 100;
      this.ScrollbarH.Min = 0;
      this.ScrollbarH.Name = "ScrollbarH";
      this.ScrollbarH.Size = new Size(780, 10);
      this.ScrollbarH.TabIndex = 0;
      this.ScrollbarH.Value = 0;
      this.ScrollbarH.Scroll += new EventHandler<ScrollEventArgs>(this.ScrollbarHScroll);
      // 
      // Timeline
      // 
      this.BackColor = Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.Controls.Add(this.ScrollbarV);
      this.Controls.Add(this.ScrollbarH);
      this.Name = "Timeline";
      this.Size = new Size(800, 200);
      this.ResumeLayout(false);

    }

    #endregion

    protected HorizontalScrollbar ScrollbarH;
    protected VerticalScrollbar ScrollbarV;
  }
}
