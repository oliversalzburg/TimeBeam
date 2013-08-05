using System;
using System.Windows.Forms;

namespace TimeBeam {
  partial class Timeline {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose( bool disposing ) {
      if( disposing && ( components != null ) ) {
        components.Dispose();
      }
      base.Dispose( disposing );
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.ScrollbarV = new TimeBeam.Scrollbar.VerticalScrollbar();
      this.ScrollbarH = new TimeBeam.Scrollbar.HorizontalScrollbar();
      this.SuspendLayout();
      // 
      // ScrollbarV
      // 
      this.ScrollbarV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ScrollbarV.BackgroundColor = System.Drawing.Color.Black;
      this.ScrollbarV.ForegroundColor = System.Drawing.Color.Gray;
      this.ScrollbarV.Location = new System.Drawing.Point(781, 0);
      this.ScrollbarV.Max = 100;
      this.ScrollbarV.Min = 0;
      this.ScrollbarV.Name = "ScrollbarV";
      this.ScrollbarV.Size = new System.Drawing.Size(20, 180);
      this.ScrollbarV.TabIndex = 1;
      this.ScrollbarV.Value = 0;
      this.ScrollbarV.Scroll += new System.EventHandler<System.Windows.Forms.ScrollEventArgs>(this.ScrollbarVScroll);
      // 
      // ScrollbarH
      // 
      this.ScrollbarH.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ScrollbarH.BackgroundColor = System.Drawing.Color.Black;
      this.ScrollbarH.ForegroundColor = System.Drawing.Color.Gray;
      this.ScrollbarH.Location = new System.Drawing.Point(0, 180);
      this.ScrollbarH.Max = 100;
      this.ScrollbarH.Min = 0;
      this.ScrollbarH.Name = "ScrollbarH";
      this.ScrollbarH.Size = new System.Drawing.Size(780, 20);
      this.ScrollbarH.TabIndex = 0;
      this.ScrollbarH.Value = 0;
      this.ScrollbarH.Scroll += new System.EventHandler<System.Windows.Forms.ScrollEventArgs>(this.ScrollbarHScroll);
      // 
      // Timeline
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.Controls.Add(this.ScrollbarV);
      this.Controls.Add(this.ScrollbarH);
      this.DoubleBuffered = true;
      this.Name = "Timeline";
      this.Size = new System.Drawing.Size(800, 200);
      this.Load += new System.EventHandler(this.TimelineLoad);
      this.Paint += new System.Windows.Forms.PaintEventHandler(this.TimelinePaint);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TimelineMouseDown);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TimelineMouseMove);
      this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TimelineMouseUp);
      this.Resize += new System.EventHandler(this.TimelineResize);
      this.ResumeLayout(false);

    }

    #endregion

    private Scrollbar.HorizontalScrollbar ScrollbarH;
    private Scrollbar.VerticalScrollbar ScrollbarV;
  }
}
