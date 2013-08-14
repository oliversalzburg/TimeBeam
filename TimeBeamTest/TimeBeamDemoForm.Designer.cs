namespace TimeBeamTest {
  partial class TimeBeamDemoForm {
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.splitter1 = new System.Windows.Forms.Splitter();
      this.splineline1 = new TimeBeam.Splineline();
      this.timeline1 = new TimeBeam.Timeline();
      this.SuspendLayout();
      // 
      // timer1
      // 
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // splitter1
      // 
      this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
      this.splitter1.Location = new System.Drawing.Point(0, 222);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new System.Drawing.Size(1046, 5);
      this.splitter1.TabIndex = 1;
      this.splitter1.TabStop = false;
      // 
      // splineline1
      // 
      this.splineline1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.splineline1.BackgroundColor = System.Drawing.Color.Black;
      this.splineline1.Clock = null;
      this.splineline1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splineline1.GridAlpha = 40;
      this.splineline1.Location = new System.Drawing.Point(0, 227);
      this.splineline1.Name = "splineline1";
      this.splineline1.Size = new System.Drawing.Size(1046, 216);
      this.splineline1.TabIndex = 2;
      this.splineline1.Text = "splineline1";
      this.splineline1.TrackBorderSize = 2;
      this.splineline1.TrackHeight = 20;
      this.splineline1.TrackSpacing = 1;
      // 
      // timeline1
      // 
      this.timeline1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.timeline1.BackgroundColor = System.Drawing.Color.Black;
      this.timeline1.Clock = null;
      this.timeline1.Dock = System.Windows.Forms.DockStyle.Top;
      this.timeline1.GridAlpha = 40;
      this.timeline1.Location = new System.Drawing.Point(0, 0);
      this.timeline1.Name = "timeline1";
      this.timeline1.Size = new System.Drawing.Size(1046, 222);
      this.timeline1.TabIndex = 0;
      this.timeline1.TrackBorderSize = 2;
      this.timeline1.TrackHeight = 20;
      this.timeline1.TrackSpacing = 1;
      // 
      // TimeBeamDemoForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1046, 443);
      this.Controls.Add(this.splineline1);
      this.Controls.Add(this.splitter1);
      this.Controls.Add(this.timeline1);
      this.DoubleBuffered = true;
      this.KeyPreview = true;
      this.Name = "TimeBeamDemoForm";
      this.Text = "TimeBeam - Demo";
      this.Load += new System.EventHandler(this.TimeBeamDemoForm_Load);
      this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TimeBeamDemoForm_KeyUp);
      this.ResumeLayout(false);

    }

    #endregion

    private TimeBeam.Timeline timeline1;
    private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.Splitter splitter1;
    private TimeBeam.Splineline splineline1;


  }
}

