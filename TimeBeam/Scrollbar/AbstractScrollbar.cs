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
    ///   Thumb will always be at least this wide/high.
    /// </summary>
    public const int MinThumbExtent = 10;

    #region Value
    /// <summary>
    ///   The current value of the scrollbar.
    /// </summary>
    [Description( "The current value of the scrollbar." )]
    [Category( "Value" )]
    public int Value {
      get { return _value; }
      set {
        int oldValue = _value;
        _value = Math.Max( Min, Math.Min( Max, value ) );
        InvokeScrollEvent( new ScrollEventArgs( ScrollEventType.ThumbPosition, oldValue, _value, Orientation ) );

        if( _value == Min ) {
          InvokeScrollEvent( new ScrollEventArgs( ScrollEventType.First, oldValue, _value, Orientation ) );
        } else if( _value == Max ) {
          InvokeScrollEvent( new ScrollEventArgs( ScrollEventType.Last, oldValue, _value, Orientation ) );
        }

        Redraw();
      }
    }

    /// <summary>
    ///   Backing field for <see cref="Value" />.
    /// </summary>
    private int _value;

    /// <summary>
    ///   The smallest possible value of the scrollbar.
    /// </summary>
    [Description( "The smallest possible value of the scrollbar." )]
    [Category( "Value" )]
    public int Min { get; set; }

    /// <summary>
    ///   The largest possible value of the scrollbar.
    /// </summary>
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
    ///   The orientation of the scrollbar.
    /// </summary>
    public ScrollOrientation Orientation { get; protected set; }

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


    /// <summary>
    ///   Invoked when the scrollbar is being scrolled.
    /// </summary>
    public new event EventHandler<ScrollEventArgs> Scroll;

    /// <summary>
    ///   Invoked the <see cref="Scroll" /> event.
    /// </summary>
    /// <param name="eventArgs">The arguments to pass with the event.</param>
    protected void InvokeScrollEvent( ScrollEventArgs eventArgs ) {
      if( null != Scroll ) {
        Scroll( this, eventArgs );
      }
    }
    #endregion

    /// <summary>
    ///   Construct a new scrollbar
    /// </summary>
    protected AbstractScrollbar( ScrollOrientation orientation ) {
      Orientation = orientation;

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