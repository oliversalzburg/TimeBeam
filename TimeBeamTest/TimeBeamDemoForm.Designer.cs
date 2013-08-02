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
      this.timeline1 = new TimeBeam.Timeline();
      this.SuspendLayout();
      // 
      // timeline1
      // 
      this.timeline1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.timeline1.BackgroundColor = System.Drawing.Color.Black;
      this.timeline1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.timeline1.Location = new System.Drawing.Point(0, 0);
      this.timeline1.Name = "timeline1";
      this.timeline1.TrackHeight = 10;
      this.timeline1.Size = new System.Drawing.Size(1046, 222);
      this.timeline1.TabIndex = 0;
      // 
      // TimeBeamDemoForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1046, 222);
      this.Controls.Add(this.timeline1);
      this.DoubleBuffered = true;
      this.Name = "TimeBeamDemoForm";
      this.Text = "TimeBeamDemoForm";
      this.Load += new System.EventHandler(this.TimeBeamDemoForm_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private TimeBeam.Timeline timeline1;


  }
}

