using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TimeBeam {
  public partial class Timeline : UserControl {
    #region Layout
    /// <summary>
    ///   The current value of the scrollbar.
    /// </summary>
    [DisplayName( "Row Height" )]
    [Description( "How high a single row should be." )]
    [Category( "Layout" )]
    public int RowHeight { get; set; }
    #endregion

    #region Drawing
    /// <summary>
    ///   The backbuffer itself.
    /// </summary>
    private Bitmap PixelMap { get; set; }

    /// <summary>
    ///   The <see cref="Graphics" /> object to draw into the backbuffer.
    /// </summary>
    protected Graphics GraphicsContainer { get; set; }

    /// <summary>
    ///   The background color of the timeline.
    /// </summary>
    [DisplayName( "Background Color" )]
    [Description( "The background color of the timeline." )]
    [Category( "Drawing" )]
    public Color BackgroundColor {
      get { return _backgroundColor; }
      set { _backgroundColor = value; }
    }

    /// <summary>
    ///   Backing field for <see cref="BackgroundColor" />.
    /// </summary>
    private Color _backgroundColor = Color.Black;
    #endregion

    /// <summary>
    ///   Construct a new timeline.
    /// </summary>
    public Timeline() {
      InitializeComponent();
      InitializePixelMap();
    }

    /// <summary>
    ///   Redraws the timeline.
    /// </summary>
    /// <exception cref="NotImplementedException">Should be overridden in derived class.</exception>
    protected void Redraw() {
      // Clear the buffer
      GraphicsContainer.Clear( BackgroundColor );
    }

    /// <summary>
    ///   Initialize the backbuffer
    /// </summary>
    private void InitializePixelMap() {
      PixelMap = new Bitmap( Width, Height );
      GraphicsContainer = Graphics.FromImage( PixelMap );
      GraphicsContainer.Clear( BackgroundColor );
      Refresh();
    }

    /// <summary>
    ///   Invoked when the control is resized.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimelineResize( object sender, EventArgs e ) {
      InitializePixelMap();
      Redraw();
      Refresh();
    }

    /// <summary>
    ///   Invoked when the control is repainted
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimelinePaint( object sender, PaintEventArgs e ) {
      e.Graphics.DrawImage( PixelMap, 0, 0 );
    }
  }
}