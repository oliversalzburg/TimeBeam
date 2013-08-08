using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TimeBeam.Helper;
using TimeBeam.Surrogates;
using TimeBeam.Timing;

namespace TimeBeam {
  /// <summary>
  ///   The main host control.
  /// </summary>
  public partial class Timeline : Control {
    #region Layout
    /// <summary>
    ///   How high a single track should be.
    /// </summary>
    [Description( "How high a single track should be." )]
    [Category( "Layout" )]
    public int TrackHeight {
      get { return _trackHeight; }
      set { _trackHeight = value; }
    }

    /// <summary>
    ///   Backing field for <see cref="TrackHeight" />.
    /// </summary>
    private int _trackHeight = 20;

    /// <summary>
    ///   How wide/high the border on a track item should be.
    ///   This border allows you to interact with an item.
    /// </summary>
    [Description( "How wide/high the border on a track item should be." )]
    [Category( "Layout" )]
    public int TrackBorderSize {
      get { return _trackBorderSize; }
      set { _trackBorderSize = value; }
    }

    /// <summary>
    ///   Backing field for <see cref="TrackBorderSize" />.
    /// </summary>
    private int _trackBorderSize = 2;

    /// <summary>
    ///   How much space should be left between every track.
    /// </summary>
    [Description( "How much space should be left between every track." )]
    [Category( "Layout" )]
    public int TrackSpacing {
      get { return _trackSpacing; }
      set { _trackSpacing = value; }
    }

    /// <summary>
    ///   Backing field for <see cref="TrackSpacing" />.
    /// </summary>
    private int _trackSpacing = 1;

    /// <summary>
    ///   The width of the label section before the tracks.
    /// </summary>
    [Description( "The width of the label section before the tracks." )]
    [Category( "Layout" )]
    private int TrackLabelWidth {
      get { return _trackLabelWidth; }
      set { _trackLabelWidth = value; }
    }

    /// <summary>
    ///   Backing field for <see cref="TrackLabelWidth" />.
    /// </summary>
    private int _trackLabelWidth = 100;

    /// <summary>
    ///   The font to use to draw the track labels.
    /// </summary>
    private Font _labelFont = DefaultFont;

    /// <summary>
    ///   The size of the top part of the playhead.
    /// </summary>
    private SizeF _playheadExtents = new SizeF( 5, 16 );
    #endregion

    #region Drawing
    /// <summary>
    ///   The backbuffer itself.
    /// </summary>
    private Bitmap PixelMap { get; set; }

    /// <summary>
    ///   The <see cref="Graphics" /> object to draw into the backbuffer.
    /// </summary>
    private Graphics GraphicsContainer { get; set; }

    /// <summary>
    ///   The background color of the timeline.
    /// </summary>
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

    /// <summary>
    ///   When the timeline is scrolled (panned) around, this offset represents the panned distance.
    /// </summary>
    private PointF _renderingOffset = PointF.Empty;

    /// <summary>
    ///   The scale at which to render the timeline.
    ///   This enables us to "zoom" the timeline in and out.
    /// </summary>
    private PointF _renderingScale = new PointF( 1, 1 );

    /// <summary>
    ///   The transparency of the background grid.
    /// </summary>
    [Description( "The transparency of the background grid." )]
    [Category( "Drawing" )]
    public int GridAlpha {
      get { return _gridAlpha; }
      set { _gridAlpha = value; }
    }

    /// <summary>
    ///   Backing field for <see cref="GridAlpha" />.
    /// </summary>
    private int _gridAlpha = 40;
    #endregion

    #region Tracks
    /// <summary>
    ///   The tracks currently placed on the timeline.
    /// </summary>
    private readonly List<List<ITimelineTrack>> _tracks = new List<List<ITimelineTrack>>();

    /// <summary>
    ///   The currently selected tracks.
    /// </summary>
    private readonly List<ITimelineTrack> _selectedTracks = new List<ITimelineTrack>();
    #endregion

    #region Interaction
    /// <summary>
    ///   What mode is the timeline currently in?
    /// </summary>
    public BehaviorMode CurrentMode { get; private set; }

    /// <summary>
    ///   The list of surrogates (stand-ins) for timeline tracks.
    ///   These surrogates are used as temporary placeholders during certain operations.
    /// </summary>
    private List<ITimelineTrack> _trackSurrogates = new List<ITimelineTrack>();

    /// <summary>
    ///   The point at where a dragging operation started.
    /// </summary>
    private PointF _dragOrigin;

    /// <summary>
    ///   The point where the user started drawing up a selection rectangle.
    /// </summary>
    private PointF _selectionOrigin;

    /// <summary>
    ///   The point where the user started panning the view.
    /// </summary>
    private PointF _panOrigin;

    /// <summary>
    ///   The rendering offset as it was before a panning operation started.
    ///   Remembering this allows us to dynamically apply a delta during the panning operation.
    /// </summary>
    private PointF _renderingOffsetBeforePan = PointF.Empty;

    /// <summary>
    ///   The current selection rectangle.
    /// </summary>
    private RectangleF _selectionRectangle = RectangleF.Empty;

    /// <summary>
    ///   The currently active edge of the tracks in focus (if any).
    /// </summary>
    private RectangleHelper.Edge _activeEdge = RectangleHelper.Edge.None;
    #endregion

    #region Timing
    /// <summary>
    ///   The clock to use as the timing source.
    /// </summary>
    public IClock Clock {
      get { return _clock; }
      set {
        _clock = value;
        RedrawAndRefresh();
      }
    }

    /// <summary>
    ///   Backing field for <see cref="Clock" />.
    /// </summary>
    private IClock _clock;
    #endregion

    #region Enums
    /// <summary>
    ///   Enumerates states the timeline can be in.
    ///   These are usually invoked through user interaction.
    /// </summary>
    public enum BehaviorMode {
      /// <summary>
      ///   The timeline is idle or not using any more specific state.
      /// </summary>
      Idle,

      /// <summary>
      ///   The user is currently in the process of selecting items on the timeline.
      /// </summary>
      Selecting,

      /// <summary>
      ///   The user is currently moving selected items.
      /// </summary>
      MovingSelection,

      /// <summary>
      ///   The user is resizing the selected tracks.
      /// </summary>
      ResizingSelection
    }
    #endregion

    #region Constructor
    /// <summary>
    ///   Construct a new timeline.
    /// </summary>
    public Timeline() {
      InitializeComponent();
      SetStyle(
        ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.UserPaint, true );

      InitializePixelMap();

      // Set up the font to use to draw the track labels
      float emHeightForLabel = EmHeightForLabel( "WM_g^~", TrackHeight );
      _labelFont = new Font( DefaultFont.FontFamily, emHeightForLabel );
    }
    #endregion

    /// <summary>
    ///   Add a track to the timeline.
    /// </summary>
    /// <param name="track">The track to add.</param>
    public void AddTrack( ITimelineTrack track ) {
      _tracks.Add(
        new List<ITimelineTrack> {
          track
        } );
      RecalculateScrollbarBounds();
      RedrawAndRefresh();
    }

    /// <summary>
    /// Add a track to the timeline which contains multiple other tracks.
    /// </summary>
    /// <param name="track"></param>
    public void AddTrack( IMultiPartTimelineTrack track ) {
      _tracks.Add( track.TrackElements.ToList() );
      RecalculateScrollbarBounds();
      RedrawAndRefresh();
    }

    /// <summary>
    ///   Invoked when the external clock is updated.
    /// </summary>
    public void Tick() {
      if( Clock.IsRunning ) {
        Redraw();
      }
    }

    #region Helpers
    /// <summary>
    ///   Recalculates appropriate values for scrollbar bounds.
    /// </summary>
    private void RecalculateScrollbarBounds() {
      ScrollbarV.Max = (int)( ( _tracks.Count * ( TrackHeight + TrackSpacing ) ) * _renderingScale.Y );
      ScrollbarH.Max = (int)( _tracks.Max( t => t.Max( te => te.End ) ) * _renderingScale.X );
    }

    /// <summary>
    ///   Calculate the rectangle within which track should be drawn.
    /// </summary>
    /// <returns>The rectangle within which all tracks should be drawn.</returns>
    private Rectangle GetTrackAreaBounds() {
      Rectangle trackArea = new Rectangle();

      // Start after the track labels
      trackArea.X = TrackLabelWidth;
      // Start at the top (later, we'll deduct the playhead and time label height)
      trackArea.Y = (int)_playheadExtents.Height;
      // Deduct scrollbar width.
      trackArea.Width = Width - ScrollbarV.Width;
      // Deduct scrollbar height.
      trackArea.Height = Height - ScrollbarH.Height;

      return trackArea;
    }

    /// <summary>
    ///   Calculate the bounding rectangle for a track.
    /// </summary>
    /// <param name="track">The track for which to calculate the bounding rectangle.</param>
    /// <returns>The bounding rectangle for the given track.</returns>
    private RectangleF GetTrackExtents( ITimelineTrack track ) {
      Rectangle trackAreaBounds = GetTrackAreaBounds();

      // The index of this track (or the one it's a substitute for).
      int trackIndex = TrackIndexForTrack( track );
      int actualRowHeight = (int)( ( TrackHeight + TrackSpacing ) * _renderingScale.Y );
      // Calculate the Y offset for the track.
      int trackOffsetY = (int)( trackAreaBounds.Y + ( actualRowHeight * trackIndex ) + _renderingOffset.Y );

      // Calculate the X offset for track.
      int trackOffsetX = (int)( trackAreaBounds.X + ( track.Start * _renderingScale.X ) + _renderingOffset.X );

      // The extent of the track, including the border
      RectangleF trackExtent = new RectangleF( trackOffsetX, trackOffsetY, ( track.End - track.Start ) * _renderingScale.X, TrackHeight * _renderingScale.Y );
      return trackExtent;
    }

    /// <summary>
    ///   Check if a track is located at the given position.
    /// </summary>
    /// <param name="test">The point to test for.</param>
    /// <returns>
    ///   The <see cref="ITimelineTrack" /> if there is one under the given point; <see langword="null" /> otherwise.
    /// </returns>
    private ITimelineTrack TrackHitTest( PointF test ) {
      foreach( ITimelineTrack track in _tracks.SelectMany( t => t ) ) {
        // The extent of the track, including the border
        RectangleF trackExtent = GetTrackExtents( track );

        if( trackExtent.Contains( test ) ) {
          return track;
        }
      }

      return null;
    }

    /// <summary>
    ///   Calculate an Em-height for a font to fit within a given height.
    /// </summary>
    /// <param name="label">The text to use for the measurement.</param>
    /// <param name="maxHeight">The largest height the text must fit into.</param>
    /// <returns>An Em-height that can be used to construct a font that will fit into the given height.</returns>
    private float EmHeightForLabel( string label, float maxHeight ) {
      float size = DefaultFont.Size;
      Font currentFont = new Font( DefaultFont.FontFamily, size );
      SizeF measured = GraphicsContainer.MeasureString( label, currentFont );
      while( measured.Height < maxHeight ) {
        size += 1;
        currentFont = new Font( DefaultFont.FontFamily, size );
        measured = GraphicsContainer.MeasureString( label, currentFont );
      }
      return size - 1;
    }

    /// <summary>
    ///   Helper method to check if a given key is being pressed.
    /// </summary>
    /// <param name="key">The key to check if it is being held down.</param>
    /// <param name="keys">The collection of keys that hold the information about which keys are being held down. If none are provided, ModifierKeys is being used.</param>
    /// <returns>
    ///   <see langword="true" /> if the key is down; <see langword="false" /> otherwise.
    /// </returns>
    private bool IsKeyDown( Keys key, Keys keys = Keys.None ) {
      if( Keys.None == keys ) {
        keys = ModifierKeys;
      }

      return ( ( keys & key ) != 0 );
    }
    #endregion

    #region Drawing Methods
    /// <summary>
    ///   Redraws the timeline.
    /// </summary>
    private void Redraw() {
      // Clear the buffer
      GraphicsContainer.Clear( BackgroundColor );

      DrawBackground();
      DrawTracks( _tracks.SelectMany( t => t ) );
      DrawTracks( _trackSurrogates );
      DrawSelectionRectangle();

      // Draw labels after the tracks to draw over elements that are partially moved out of the viewing area
      DrawTrackLabels();

      DrawPlayhead();
    }

    /// <summary>
    ///   Redraws the control and then invokes a refresh to have it redrawn on screen.
    /// </summary>
    private void RedrawAndRefresh() {
      Redraw();
      Refresh();
    }

    /// <summary>
    ///   Draws the background of the control.
    /// </summary>
    private void DrawBackground() {

      Rectangle trackAreaBounds = GetTrackAreaBounds();

      // Draw horizontal grid.
      // Calculate the Y position of the first line.
      int firstLineY = (int)( TrackHeight * _renderingScale.Y + trackAreaBounds.Y + _renderingOffset.Y );
      // Calculate the distance between each following line.
      int actualRowHeight = (int)( ( TrackHeight + TrackSpacing ) * _renderingScale.Y );
      actualRowHeight = Math.Max( 1, actualRowHeight );
      // Draw the actual lines.
      for( int y = firstLineY; y < Height; y += actualRowHeight ) {
        GraphicsContainer.DrawLine( new Pen( Color.FromArgb( GridAlpha, Color.White ) ), trackAreaBounds.X, y, trackAreaBounds.Width, y );
      }

      // The dinstance between the minor ticks.
      float minorTickDistance = _renderingScale.X;
      int minorTickOffset = (int)( _renderingOffset.X % minorTickDistance );

      // The distance between the regular ticks.
      int tickDistance = (int)( 10f * _renderingScale.X );
      tickDistance = Math.Max( 1, tickDistance );

      // The distance between minute ticks
      int minuteDistance = tickDistance * 6;

      // Draw a vertical grid. Every 10 ticks, we place a line.
      int tickOffset = (int)( _renderingOffset.X % tickDistance );
      int minuteOffset = (int)( _renderingOffset.X % minuteDistance );

      // Calculate the distance between each column line.
      int columnWidth = (int)( 10 * _renderingScale.X );
      columnWidth = Math.Max( 1, columnWidth );

      // Should we draw minor ticks?
      if( minorTickDistance > 2.0f ) {
        for( float x = minorTickOffset; x < Width; x += minorTickDistance ) {
          GraphicsContainer.DrawLine(
            new Pen( Color.FromArgb( 30, Color.White ) ) {
              DashStyle = DashStyle.Dot
            }, trackAreaBounds.X + x, trackAreaBounds.Y, trackAreaBounds.X + x, trackAreaBounds.Height );
        }
      }

      // We start one tick distance after the offset to draw the first line that is actually in the display area
      // The one that is only tickOffset pixels away it behind the track labels.
      for( int x = tickOffset + tickDistance; x < Width; x += columnWidth ) {
        int alpha = GridAlpha;
        // Every 60 ticks, we put a brighter, thicker line.
        if( ( x - minuteOffset ) % minuteDistance == 0 ) {
          alpha = Math.Min( 255, alpha *= 2 );
        }
        GraphicsContainer.DrawLine( new Pen( Color.FromArgb( alpha, Color.White ) ), trackAreaBounds.X + x, trackAreaBounds.Y, trackAreaBounds.X + x, trackAreaBounds.Height );
      }
    }

    /// <summary>
    ///   Draw a list of tracks onto the timeline.
    /// </summary>
    /// <param name="tracks">The tracks to draw.</param>
    private void DrawTracks( IEnumerable<ITimelineTrack> tracks ) {

      Rectangle trackAreaBounds = GetTrackAreaBounds();

      // Generate colors for the tracks.
      List<Color> colors = ColorHelper.GetRandomColors( _tracks.Count );

      foreach( ITimelineTrack track in tracks ) {
        // The extent of the track, including the border
        RectangleF trackExtent = GetTrackExtents( track );

        // Don't draw track elements that aren't within the target area.
        if( !trackAreaBounds.IntersectsWith( trackExtent.ToRectangle() ) ) {
          continue;
        }

        // The index of this track (or the one it's a substitute for).
        int trackIndex = TrackIndexForTrack( track );

        // Determine colors for this track
        Color trackColor = colors[ trackIndex ];
        Color borderColor = Color.FromArgb( 128, Color.Black );

        if( _selectedTracks.Contains( track ) ) {
          borderColor = Color.WhiteSmoke;
        }

        // Draw the main track area.
        if( track is TrackSurrogate ) {
          // Draw surrogates with a transparent brush.
          GraphicsContainer.FillRectangle( new SolidBrush( Color.FromArgb( 128, trackColor ) ), trackExtent );
        } else {
          GraphicsContainer.FillRectangle( new SolidBrush( trackColor ), trackExtent );
        }

        // Compensate for border size
        trackExtent.X += TrackBorderSize / 2f;
        trackExtent.Y += TrackBorderSize / 2f;
        trackExtent.Height -= TrackBorderSize;
        trackExtent.Width -= TrackBorderSize;

        GraphicsContainer.DrawRectangle( new Pen( borderColor, TrackBorderSize ), trackExtent.X, trackExtent.Y, trackExtent.Width, trackExtent.Height );
      }
    }

    /// <summary>
    ///   Draw the labels next to each track.
    /// </summary>
    private void DrawTrackLabels() {
      foreach( ITimelineTrack track in _tracks.SelectMany( t => t ) ) {
        RectangleF trackExtents = GetTrackExtents( track );
        RectangleF labelRect = new RectangleF( 0, trackExtents.Y, TrackLabelWidth, trackExtents.Height );
        GraphicsContainer.FillRectangle( new SolidBrush( Color.FromArgb( 50, 50, 50 ) ), labelRect );
        GraphicsContainer.DrawString( track.Name, _labelFont, Brushes.LightGray, labelRect );
      }
    }

    /// <summary>
    ///   Draw a playhead on the timeline.
    ///   The playhead indicates a time value.
    /// </summary>
    private void DrawPlayhead() {
      // Only draw a playhead if we have a clock set.
      if( null != Clock ) {
        // Calculate the position of the playhead.
        Rectangle trackAreaBounds = GetTrackAreaBounds();

        // Draw a background for the playhead. This also overdraws elements that drew into the playhead area.
        GraphicsContainer.FillRectangle( Brushes.Black, 0, 0, Width, _playheadExtents.Height );

        float playheadOffset = (float)( trackAreaBounds.X + ( Clock.Value * 0.001f ) * _renderingScale.X ) + _renderingOffset.X;
        // Don't draw when not in view.
        if( playheadOffset < trackAreaBounds.X || playheadOffset > trackAreaBounds.X + trackAreaBounds.Width ) {
          return;
        }

        // Draw the playhead as a single line.
        GraphicsContainer.DrawLine( Pens.SpringGreen, playheadOffset, trackAreaBounds.Y, playheadOffset, trackAreaBounds.Height );

        GraphicsContainer.FillRectangle( Brushes.SpringGreen, playheadOffset - _playheadExtents.Width / 2, 0, _playheadExtents.Width, _playheadExtents.Height );
      }
    }

    /// <summary>
    ///   Draws the selection rectangle the user is drawing.
    /// </summary>
    private void DrawSelectionRectangle() {
      GraphicsContainer.DrawRectangle(
        new Pen( Color.LightGray, 1 ) {
          DashStyle = DashStyle.Dot
        }, _selectionRectangle.ToRectangle() );
    }

    /// <summary>
    ///   Retrieve the index of a given track.
    ///   If the track is a surrogate, returns the index of the track it's a substitute for.
    /// </summary>
    /// <param name="track">The track for which to retrieve the index.</param>
    /// <returns>The index of the track or the index the track is a substitute for.</returns>
    private int TrackIndexForTrack( ITimelineTrack track ) {
      ITimelineTrack trackToLookFor = track;
      if( track is TrackSurrogate ) {
        trackToLookFor = ( (TrackSurrogate)track ).SubstituteFor;
      }
      return _tracks.FindIndex( t => t.Contains( trackToLookFor ) );
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
    #endregion

    #region Event Handler
    /// <summary>
    ///   Invoked when the control is resized.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected override void OnResize( EventArgs e ) {
      base.OnResize( e );

      InitializePixelMap();
      RedrawAndRefresh();
    }

    /// <summary>
    ///   Invoked when the control is repainted
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint( PaintEventArgs e ) {
      base.OnPaint( e );

      e.Graphics.DrawImage( PixelMap, 0, 0 );
    }

    /// <summary>
    ///   Invoked when the cursor is moved over the control.
    /// </summary>
    /// <param name="e"></param>
    /// <exception cref="InvalidOperationException">Selection origin not set. This shouldn't happen.</exception>
    protected override void OnMouseMove( MouseEventArgs e ) {
      base.OnMouseMove( e );

      // Store the current mouse position.
      PointF location = new PointF( e.X, e.Y );
      // Check if there is a track at the current mouse position.
      ITimelineTrack focusedTrack = TrackHitTest( location );

      // Is the left mouse button pressed?
      if( ( e.Button & MouseButtons.Left ) != 0 ) {
        if( CurrentMode == BehaviorMode.MovingSelection ) {
          // Indicate ability to move though cursor.
          Cursor = Cursors.SizeWE;

          foreach( TrackSurrogate selectedTrack in _trackSurrogates ) {
            // Calculate the movement delta.
            PointF delta = PointF.Subtract( location, new SizeF( _dragOrigin ) );
            float length = selectedTrack.End - selectedTrack.Start;
            // Then apply the delta to the track.
            // For that, we first get the original position from the original (non-surrogate) item and
            // then apply the delta to that value to get the offset for the surrogate.
            selectedTrack.Start = selectedTrack.SubstituteFor.Start + ( delta.X * ( 1 / _renderingScale.X ) );
            selectedTrack.End = selectedTrack.Start + length;
          }

          // Force a redraw.
          RedrawAndRefresh();

        } else if( CurrentMode == BehaviorMode.ResizingSelection ) {
          // Indicate ability to resize though cursor.
          Cursor = Cursors.SizeWE;

          foreach( TrackSurrogate selectedTrack in _trackSurrogates ) {
            // Calculate the movement delta.
            PointF delta = PointF.Subtract( location, new SizeF( _dragOrigin ) );
            // Apply the delta to the start or end of the timline track,
            // depending on the edge where the user originally started the resizing operation.
            if( ( _activeEdge & RectangleHelper.Edge.Left ) != 0 ) {
              selectedTrack.Start = selectedTrack.SubstituteFor.Start + ( delta.X * ( 1 / _renderingScale.X ) );
            } else if( ( _activeEdge & RectangleHelper.Edge.Right ) != 0 ) {
              selectedTrack.End = selectedTrack.SubstituteFor.End + ( delta.X * ( 1 / _renderingScale.X ) );
            }
          }

          // Force a redraw.
          RedrawAndRefresh();

        } else if( CurrentMode == BehaviorMode.Selecting ) {
          if( _selectionOrigin == PointF.Empty ) {
            throw new InvalidOperationException( "Selection origin not set. This shouldn't happen." );
          }

          // Set the appropriate cursor for a selection action.
          Cursor = Cursors.Cross;

          // Construct the correct rectangle spanning from the selection origin to the current cursor position.
          _selectionRectangle = RectangleHelper.Normalize( _selectionOrigin, location ).ToRectangle();

          RedrawAndRefresh();
        }

      } else if( ( e.Button & MouseButtons.Middle ) != 0 ) {
        // Pan the view
        // Calculate the movement delta.
        PointF delta = PointF.Subtract( location, new SizeF( _panOrigin ) );
        // Now apply the delta to the rendering offsets to pan the view.
        _renderingOffset = PointF.Add( _renderingOffsetBeforePan, new SizeF( delta ) );

        // Make sure to stay within bounds.
        _renderingOffset.X = Math.Max( -ScrollbarH.Max, Math.Min( 0, _renderingOffset.X ) );
        _renderingOffset.Y = Math.Max( -ScrollbarV.Max, Math.Min( 0, _renderingOffset.Y ) );

        // Update scrollbar positions. This will invoke a redraw.
        ScrollbarH.Value = (int)( -_renderingOffset.X );
        ScrollbarV.Value = (int)( -_renderingOffset.Y );

      } else {
        // No mouse button is being pressed
        if( null != focusedTrack ) {
          RectangleF trackExtents = GetTrackExtents( focusedTrack );
          RectangleHelper.Edge isPointOnEdge = RectangleHelper.IsPointOnEdge( trackExtents, location, 3f, RectangleHelper.EdgeTest.Horizontal );

          // Select the appropriate size cursor for the cursor position.
          // Even though we currently only support horizontal cases, we respect all possible return values here.
          switch( isPointOnEdge ) {
            case RectangleHelper.Edge.Top:
            case RectangleHelper.Edge.Bottom:
              Cursor = Cursors.SizeNS;
              break;
            case RectangleHelper.Edge.Right:
            case RectangleHelper.Edge.Left:
              Cursor = Cursors.SizeWE;
              break;
            case RectangleHelper.Edge.Top | RectangleHelper.Edge.Left:
            case RectangleHelper.Edge.Bottom | RectangleHelper.Edge.Right:
              Cursor = Cursors.SizeNWSE;
              break;
            case RectangleHelper.Edge.Top | RectangleHelper.Edge.Right:
            case RectangleHelper.Edge.Bottom | RectangleHelper.Edge.Left:
              Cursor = Cursors.SizeNESW;
              break;
            case RectangleHelper.Edge.None:
              Cursor = Cursors.Arrow;
              break;
            default:
              Cursor = Cursors.Arrow;
              break;
          }

        } else {
          Cursor = Cursors.Arrow;
        }
      }
    }

    /// <summary>
    ///   Invoked when the user presses a mouse button over the control.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDown( MouseEventArgs e ) {
      base.OnMouseDown( e );

      // Store the current mouse position.
      PointF location = new PointF( e.X, e.Y );

      if( ( e.Button & MouseButtons.Left ) != 0 ) {
        // Check if there is a track at the current mouse position.
        ITimelineTrack focusedTrack = TrackHitTest( location );

        if( null != focusedTrack ) {
          // Was this track already selected?
          if( !_selectedTracks.Contains( focusedTrack ) ) {
            // Tell the track that it was selected.
            focusedTrack.Selected();
            // Store a reference to the selected track
            _selectedTracks.Clear();
            _selectedTracks.Add( focusedTrack );
          }
          // Store the current mouse position. It'll be used later to calculate the movement delta.
          _dragOrigin = location;
          // Create and store surrogates for selected timeline tracks.
          _trackSurrogates = SurrogateHelper.GetSurrogates( _selectedTracks );

          // Check whether the user wants to move or resize the selected tracks.
          RectangleF trackExtents = GetTrackExtents( focusedTrack );
          RectangleHelper.Edge isPointOnEdge = RectangleHelper.IsPointOnEdge( trackExtents, location, 3f, RectangleHelper.EdgeTest.Horizontal );
          if( isPointOnEdge != RectangleHelper.Edge.None ) {
            CurrentMode = BehaviorMode.ResizingSelection;
            _activeEdge = isPointOnEdge;
          } else {
            CurrentMode = BehaviorMode.MovingSelection;
          }

        } else {
          // Reset the track selection.
          _selectedTracks.Clear();

          // Remember this location as the origin for the selection.
          _selectionOrigin = location;

          CurrentMode = BehaviorMode.Selecting;
        }

      } else if( ( e.Button & MouseButtons.Middle ) != 0 ) {
        _panOrigin = location;
        _renderingOffsetBeforePan = _renderingOffset;
      }

      RedrawAndRefresh();
    }

    /// <summary>
    ///   Invoked when the user releases the mouse cursor over the control.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseUp( MouseEventArgs e ) {
      base.OnMouseUp( e );

      // Store the current mouse position.
      PointF location = new PointF( e.X, e.Y );

      if( ( e.Button & MouseButtons.Left ) != 0 ) {
        if( CurrentMode == BehaviorMode.Selecting ) {
          // If we were selecting, it's now time to finalize the selection
          // Construct the correct rectangle spanning from the selection origin to the current cursor position.
          RectangleF selectionRectangle = RectangleHelper.Normalize( _selectionOrigin, location );

          foreach( ITimelineTrack track in _tracks.SelectMany( t => t ) ) {
            RectangleF boundingRectangle = GetTrackExtents( track );

            // Check if the track item is selected by the selection rectangle.
            if( SelectionHelper.IsSelected( selectionRectangle, boundingRectangle, ModifierKeys ) ) {
              // Add it to the selection.
              _selectedTracks.Add( track );
            }
          }

        } else if( CurrentMode == BehaviorMode.MovingSelection || CurrentMode == BehaviorMode.ResizingSelection ) {
          // The moving operation ended, apply the values of the surrogates to the originals
          foreach( TrackSurrogate surrogate in _trackSurrogates ) {
            surrogate.CopyTo( surrogate.SubstituteFor );
          }
          _trackSurrogates.Clear();

          RecalculateScrollbarBounds();
        }

        // Reset cursor
        Cursor = Cursors.Arrow;
        // Reset selection origin.
        _selectionOrigin = PointF.Empty;
        // And the selection rectangle itself.
        _selectionRectangle = RectangleF.Empty;
        // Reset mode.
        CurrentMode = BehaviorMode.Idle;

      } else if( ( e.Button & MouseButtons.Middle ) != 0 ) {
        _panOrigin = PointF.Empty;
        _renderingOffsetBeforePan = PointF.Empty;
      }

      RedrawAndRefresh();
    }

    /// <summary>
    ///   Invoked when a key is released.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnKeyUp( KeyEventArgs e ) {
      base.OnKeyUp( e );

      if( e.KeyCode == Keys.A && IsKeyDown( Keys.Control ) ) {
        // Ctrl+A - Select all
        _selectedTracks.Clear();
        foreach( ITimelineTrack track in _tracks.SelectMany( t => t ) ) {
          _selectedTracks.Add( track );
          track.Selected();
        }
        RedrawAndRefresh();
      }
    }
    #endregion

    #region Scrolling
    /// <summary>
    ///   Invoked when the vertical scrollbar is being scrolled.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScrollbarVScroll( object sender, ScrollEventArgs e ) {
      _renderingOffset.Y = -e.NewValue;
      RedrawAndRefresh();
    }

    /// <summary>
    ///   Invoked when the horizontal scrollbar is being scrolled.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScrollbarHScroll( object sender, ScrollEventArgs e ) {
      _renderingOffset.X = -e.NewValue;
      RedrawAndRefresh();
    }

    /// <summary>
    ///   Invoked when the user scrolls the mouse wheel.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseWheel( MouseEventArgs e ) {
      base.OnMouseWheel( e );

      if( IsKeyDown( Keys.Alt ) ) {
        // Zooming does not require a Redraw() call, as scrolling will fire off a Scroll event which will then cause a redraw anyway.

        // If Alt is down, we're zooming.
        float amount = e.Delta / 1200f;
        Rectangle trackAreaBounds = GetTrackAreaBounds();

        if( IsKeyDown( Keys.Control ) ) {
          // If Ctrl is down as well, we're zooming horizontally.
          _renderingScale.X += amount;
          // Don't zoom below 1%
          _renderingScale.X = Math.Max( 0.01f, _renderingScale.X );

          // We now also need to move the rendering offset so that the center of focus stays at the mouse cursor.
          _renderingOffset.X -= trackAreaBounds.Width * ( ( e.Location.X - trackAreaBounds.X ) / (float)trackAreaBounds.Width ) * amount;
          _renderingOffset.X = Math.Min( 0, _renderingOffset.X );

          // Update scrollbar position.
          ScrollbarH.Value = (int)( -_renderingOffset.X );

        } else {
          // If Ctrl isn't  down, we're zooming vertically.
          _renderingScale.Y += amount;
          // Don't zoom below 1%
          _renderingScale.Y = Math.Max( 0.01f, _renderingScale.Y );

          // We now also need to move the rendering offset so that the center of focus stays at the mouse cursor.
          _renderingOffset.Y -= trackAreaBounds.Height * ( ( e.Location.Y - trackAreaBounds.Y ) / (float)trackAreaBounds.Height ) * amount;
          _renderingOffset.Y = Math.Min( 0, _renderingOffset.Y );

          // Update scrollbar position.
          ScrollbarV.Value = (int)( -_renderingOffset.Y );
        }
        RecalculateScrollbarBounds();

      } else {
        // Scrolling does not require a Redraw() call, as scrolling will fire off a Scroll event which will then cause a redraw anyway.

        // If Alt isn't down, we're scrolling/panning.
        if( IsKeyDown( Keys.Control ) ) {
          // If Ctrl is down, we're scrolling horizontally.
          ScrollbarH.Value -= e.Delta / 10;
        } else {
          // If no modifier keys are down, we're scrolling vertically.
          ScrollbarV.Value -= e.Delta / 10;
        }
      }

      Refresh();
    }
    #endregion
  }
}