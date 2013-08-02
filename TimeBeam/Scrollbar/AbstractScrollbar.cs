using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TimeBeam {
  /// <summary>
  ///   A scrollbar.
  /// </summary>
  public partial class AbstractScrollbar : UserControl {
    /// <summary>
    /// Thumb will always be at least this wide/high.
    /// </summary>
    public const int MinThumbExtent = 10;

    #region Value
    /// <summary>
    ///   The current value of the scrollbar.
    /// </summary>
    [DisplayName( "Value" )]
    [Description( "The current value of the scrollbar." )]
    [Category( "Value" )]
    public int Value { get; set; }

    /// <summary>
    ///   The smallest possible value of the scrollbar.
    /// </summary>
    [DisplayName( "Min" )]
    [Description( "The smallest possible value of the scrollbar." )]
    [Category( "Value" )]
    public int Min { get; set; }

    /// <summary>
    ///   The largest possible value of the scrollbar.
    /// </summary>
    [DisplayName( "Max" )]
    [Description( "The largest possible value of the scrollbar." )]
    [Category( "Value" )]
    public int Max {
      get { return _max; }
      set { _max = value; }
    }

    private int _max = 100;
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
    ///   The background color of the scrollbar.
    /// </summary>
    [DisplayName( "Background Color" )]
    [Description( "The background color of the scrollbar." )]
    [Category( "Drawing" )]
    public Color BackgroundColor {
      get { return _backgroundColor; }
      set { _backgroundColor = value; }
    }

    /// <summary>
    ///   Backing field for <see cref="BackgroundColor" />.
    /// </summary>
    private Color _backgroundColor = Color.Black;


    /// <summary>
    ///   The foreground color of the scrollbar.
    /// </summary>
    [DisplayName( "Foreground Color" )]
    [Description( "The foreground color of the scrollbar." )]
    [Category( "Drawing" )]
    [Browsable( true )]
    public Color ForegroundColor {
      get { return _foregroundColor; }
      set {
        _foregroundColor = value;
        ForegroundBrush = new SolidBrush( _foregroundColor );
      }
    }

    /// <summary>
    ///   Backing field for <see cref="ForegroundColor" />.
    /// </summary>
    private Color _foregroundColor = Color.Gray;

    /// <summary>
    ///   The default, gray brush.
    /// </summary>
    protected Brush ForegroundBrush = new SolidBrush( Color.Gray );

    /// <summary>
    ///   The bounds of the thumb that is used to define the value on the bar.
    /// </summary>
    protected Rectangle ThumbBounds = new Rectangle( 0, 0, 0, 0 );
    #endregion

    #region Scrolling
    /// <summary>
    ///   The pixel position at which the user started holding down the mouse button that will activate the scrolling action.
    ///   This is used to calculate an offset during the scrolling process and then apply it onto the original position of the thumb.
    /// </summary>
    /// <see cref="ScrollOrigin" />
    protected int ScrollDeltaOrigin;

    /// <summary>
    ///   The original position of the thumb when a scrolling process started.
    ///   The delta that is continously calculated during the scrolling process is applied onto this origin.
    /// </summary>
    /// <see cref="ScrollDeltaOrigin" />
    protected int ScrollOrigin;
    #endregion

    /// <summary>
    ///   Construct a new scrollbar
    /// </summary>
    protected AbstractScrollbar() {
      InitializeComponent();
      InitializePixelMap();
    }

    /// <summary>
    ///   Redraws the scrollbar.
    /// </summary>
    /// <exception cref="NotImplementedException">Should be overridden in derived class.</exception>
    protected virtual void Redraw() {
      throw new NotImplementedException( "Should be overridden in derived class." );
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
    private void AbstractScrollbarResize( object sender, EventArgs e ) {
      InitializePixelMap();
      Redraw();
      Refresh();
    }

    /// <summary>
    ///   Invoked when the control is repainted
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AbstractScrollbarPaint( object sender, PaintEventArgs e ) {
      e.Graphics.DrawImage( PixelMap, 0, 0 );
    }
  }
}