using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TimeBeam.Helper;
using TimeBeam.Surrogates;

namespace TimeBeam {
  /// <summary>
  ///   The main host control.
  /// </summary>
  public partial class Timeline : UserControl {
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
    private readonly List<ITimelineTrack> _tracks = new List<ITimelineTrack>();

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
    private PointF? _selectionOrigin;

    /// <summary>
    ///   The currently active edge of the tracks in focus (if any).
    /// </summary>
    private RectangleHelper.Edge _activeEdge = RectangleHelper.Edge.None;
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
      InitializePixelMap();

      // Set up the font to use to draw the track labels
      float emHeightForLabel = EmHeightForLabel( "WM_g^~", TrackHeight );
      _labelFont = new Font( DefaultFont.FontFamily, emHeightForLabel );

      // Attach mouse wheel scroll handler.
      MouseWheel += TimelineMouseWheel;
    }
    #endregion

    /// <summary>
    ///   Add a track to the timeline.
    /// </summary>
    /// <param name="track">The track to add.</param>
    public void AddTrack( ITimelineTrack track ) {
      _tracks.Add( track );
      RecalculateScrollbarBounds();
      RedrawAndRefresh();
    }

    #region Helpers
    /// <summary>
    ///   Recalculates appropriate values for scrollbar bounds.
    /// </summary>
    private void RecalculateScrollbarBounds() {
      ScrollbarV.Max = _tracks.Count * ( TrackHeight + TrackSpacing );
      ScrollbarH.Max = (int)_tracks.Max( t => t.End );
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
      trackArea.Y = 0;
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
      // Offset the next track to the appropriate position.
      int trackOffset = ( TrackHeight + TrackSpacing ) * trackIndex + (int)_renderingOffset.Y;

      // The extent of the track, including the border
      RectangleF trackExtent = new RectangleF( trackAreaBounds.X + track.Start + _renderingOffset.X, trackOffset, track.End - track.Start, TrackHeight );
      return trackExtent;
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
    #endregion

    #region Drawing Methods
    /// <summary>
    ///   Redraws the timeline.
    /// </summary>
    private void Redraw() {
      // Clear the buffer
      GraphicsContainer.Clear( BackgroundColor );

      DrawBackground();
      DrawTracks( _tracks );
      DrawTracks( _trackSurrogates );

      // Draw labels after the tracks to draw over elements that are partially moved out of the viewing area
      DrawTrackLabels();
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
      for( int y = TrackHeight + (int)_renderingOffset.Y; y < Height; y += ( TrackHeight + TrackSpacing ) ) {
        GraphicsContainer.DrawLine( new Pen( Color.FromArgb( GridAlpha, Color.White ) ), trackAreaBounds.X, y, trackAreaBounds.Width, y );
      }

      // Draw a vertical grid. Every 10 ticks, we place a line.
      int tickOffset = (int)_renderingOffset.X % 10;
      int minuteOffset = (int)_renderingOffset.X % 60;
      for( int x = tickOffset; x < Width; x += 10 ) {
        int alpha = GridAlpha;
        // Every 60 ticks, we put a brighter, thicker line.
        if( ( x - minuteOffset ) % 60 == 0 ) {
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
        //trackExtent.Offset( trackAreaBounds.X, trackAreaBounds.Y );

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
          // Draw surrogates with a hatched brush.
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

    private void DrawTrackLabels() {
      foreach( ITimelineTrack track in _tracks ) {
        RectangleF trackExtents = GetTrackExtents( track );
        RectangleF labelRect = new RectangleF( 0, trackExtents.Y, TrackLabelWidth, trackExtents.Height );
        GraphicsContainer.FillRectangle( new SolidBrush( Color.FromArgb( 50, 50, 50 ) ), labelRect );
        string label = "<No Name>";
        GraphicsContainer.DrawString( label, _labelFont, Brushes.LightGray, labelRect );
      }
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
      return _tracks.IndexOf( trackToLookFor );
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

    /// <summary>
    ///   Check if a track is located at the given position.
    /// </summary>
    /// <param name="test">The point to test for.</param>
    /// <returns>
    ///   The <see cref="ITimelineTrack" /> if there is one under the given point; <see langword="null" /> otherwise.
    /// </returns>
    private ITimelineTrack TrackHitTest( PointF test ) {
      foreach( ITimelineTrack track in _tracks ) {
        // The extent of the track, including the border
        RectangleF trackExtent = GetTrackExtents( track );

        if( trackExtent.Contains( test ) ) {
          return track;
        }
      }

      return null;
    }

    #region Event Handler
    /// <summary>
    ///   Invoked when the control is resized.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimelineResize( object sender, EventArgs e ) {
      InitializePixelMap();
      RedrawAndRefresh();
    }

    /// <summary>
    ///   Invoked when the control is repainted
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimelinePaint( object sender, PaintEventArgs e ) {
      e.Graphics.DrawImage( PixelMap, 0, 0 );
    }

    /// <summary>
    ///   Invoked once the timeline has loaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimelineLoad( object sender, EventArgs e ) {
      RedrawAndRefresh();
    }

    /// <summary>
    ///   Invoked when the cursor is moved over the control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="InvalidOperationException">Selection origin not set. This shouldn't happen.</exception>
    private void TimelineMouseMove( object sender, MouseEventArgs e ) {
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
            selectedTrack.Start = selectedTrack.SubstituteFor.Start + delta.X;
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
              selectedTrack.Start = selectedTrack.SubstituteFor.Start + delta.X;
            } else if( ( _activeEdge & RectangleHelper.Edge.Right ) != 0 ) {
              selectedTrack.End = selectedTrack.SubstituteFor.End + delta.X;
            }
          }

          // Force a redraw.
          RedrawAndRefresh();

        } else if( CurrentMode == BehaviorMode.Selecting ) {
          if( !_selectionOrigin.HasValue ) {
            throw new InvalidOperationException( "Selection origin not set. This shouldn't happen." );
          }

          // Set the appropriate cursor for a selection action.
          Cursor = Cursors.Cross;

          // Construct the correct rectangle spanning from the selection origin to the current cursor position.
          Rectangle selectionRectangle = RectangleHelper.Normalize( _selectionOrigin.Value, location ).ToRectangle();

          Redraw();
          GraphicsContainer.DrawRectangle(
            new Pen( Color.LightGray, 1 ) {
              DashStyle = DashStyle.Dot
            }, selectionRectangle );
          Refresh();
        }

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
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimelineMouseDown( object sender, MouseEventArgs e ) {
      // Store the current mouse position.
      PointF location = new PointF( e.X, e.Y );
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

      RedrawAndRefresh();
    }

    /// <summary>
    ///   Invoked when the user releases the mouse cursor over the control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimelineMouseUp( object sender, MouseEventArgs e ) {
      // Store the current mouse position.
      PointF location = new PointF( e.X, e.Y );

      if( CurrentMode == BehaviorMode.Selecting ) {
        // If we were selecting, it's now time to finalize the selection
        // Construct the correct rectangle spanning from the selection origin to the current cursor position.
        RectangleF selectionRectangle = RectangleHelper.Normalize( _selectionOrigin.Value, location );

        int trackOffset = 0;
        for( int trackIndex = 0; trackIndex < _tracks.Count; trackIndex++ ) {
          ITimelineTrack track = _tracks[ trackIndex ];
          RectangleF boundingRectangle = GetTrackExtents( track );

          // Check if the track item is selected by the selection rectangle.
          if( SelectionHelper.IsSelected( selectionRectangle, boundingRectangle, ModifierKeys ) ) {
            // Add it to the selection.
            _selectedTracks.Add( track );
          }
          trackOffset += ( TrackBorderSize * 2 ) + TrackHeight;
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
      _selectionOrigin = null;
      // Reset mode.
      CurrentMode = BehaviorMode.Idle;

      RedrawAndRefresh();
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
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimelineMouseWheel( object sender, MouseEventArgs e ) {
      ScrollbarV.Value -= e.Delta / 10;
      Refresh();
    }
    #endregion
  }
}