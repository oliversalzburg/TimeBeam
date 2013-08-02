using System;
using System.Drawing;
using System.Windows.Forms;

namespace TimeBeam {
  public partial class Scrollbar : UserControl {
    private Bitmap PixelMap { get; set; }
    private Graphics GraphicsContainer { get; set; }

    public Scrollbar() {
      InitializeComponent();
      InitializePixelMap();
    }

    private void InitializePixelMap() {
      PixelMap = new Bitmap( Width, Height );
      GraphicsContainer = Graphics.FromImage( PixelMap );
      GraphicsContainer.Clear( Color.Black );
      Refresh();
    }

    private void ScrollbarResize( object sender, EventArgs e ) {
      InitializePixelMap();
    }

    private void ScrollbarPaint( object sender, PaintEventArgs e ) {
      e.Graphics.DrawImage( PixelMap, 0, 0 );
    }
  }
}