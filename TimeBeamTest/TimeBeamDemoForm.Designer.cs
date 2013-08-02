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
      this.horizontalScrollbar1 = new TimeBeam.Scrollbar.HorizontalScrollbar();
      this.SuspendLayout();
      // 
      // horizontalScrollbar1
      // 
      this.horizontalScrollbar1.BackgroundColor = System.Drawing.Color.Black;
      this.horizontalScrollbar1.ForegroundColor = System.Drawing.Color.Gray;
      this.horizontalScrollbar1.Location = new System.Drawing.Point(12, 12);
      this.horizontalScrollbar1.Max = 100;
      this.horizontalScrollbar1.Min = 0;
      this.horizontalScrollbar1.Name = "horizontalScrollbar1";
      this.horizontalScrollbar1.Size = new System.Drawing.Size(509, 63);
      this.horizontalScrollbar1.TabIndex = 0;
      this.horizontalScrollbar1.Value = 50;
      // 
      // TimeBeamDemoForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1046, 222);
      this.Controls.Add(this.horizontalScrollbar1);
      this.DoubleBuffered = true;
      this.Name = "TimeBeamDemoForm";
      this.Text = "TimeBeamDemoForm";
      this.ResumeLayout(false);

    }

    #endregion

    private TimeBeam.Scrollbar.HorizontalScrollbar horizontalScrollbar1;

  }
}

