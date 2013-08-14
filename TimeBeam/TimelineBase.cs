using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TimeBeam.Events;
using TimeBeam.Helper;
using TimeBeam.Surrogates;
using TimeBeam.Timing;

namespace TimeBeam {
  /// <summary>
  ///   The main host control.
  /// </summary>
  public partial class TimelineBase : Control {
    /// <summary>
    ///   How far does the user have to move the mouse (while holding down the left mouse button) until dragging operations kick in?
    ///   Technically, this defines the length of the movement vector.
    /// </summary>
    private const float DraggingThreshold = 3f;

    #region Events
    /// <summary>
    ///   Invoked when the selection of track segments changed.
    ///   Inspect <see cref="SelectedTracks"/> to see the current selection.
    /// </summary>
    public EventHandler<SelectionChangedEventArgs> SelectionChanged;

    /// <summary>
    ///   Invoke the <see cref="SelectionChanged" /> event.
    /// </summary>
    /// <param name="eventArgs">The arguments to pass with the event.</param>
    private void InvokeSelectionChanged( SelectionChangedEventArgs eventArgs = null ) {
      if( null != SelectionChanged ) {
        SelectionChanged.Invoke( this, eventArgs ?? SelectionChangedEventArgs.Empty );
      }
    }
    #endregion

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
    ///   How wide/high the border on a track segment should be.
    ///   This border allows you to interact with an item.
    /// </summary>
    [Description( "How wide/high the border on a track segment should be." )]
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

    internal PointF RenderingOffset {
      get { return _renderingOffset; }
    }

    /// <summary>
    ///   When the timeline is scrolled (panned) around, this offset represents the panned distance.
    /// </summary>
    private PointF _renderingOffset = PointF.Empty;

    internal PointF RenderingScale {
      get { return _renderingScale; }
    }

    /// <summary>
    ///   The scale at which to render the timeline.
    ///   This enables us to "zoom" the timeline in and out.
    /// </summary>
    protected PointF _renderingScale = new PointF( 1, 1 );

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
    protected readonly List<ITrack> _tracks = new List<ITrack>();

    /// <summary>
    ///   The currently selected tracks.
    /// </summary>
    protected readonly List<ITrackSegment> _selectedTracks = new List<ITrackSegment>();

    /// <summary>
    ///   Which tracks are currently selected?
    /// </summary>
    public IEnumerable<ITrackSegment> SelectedTracks { get { return _selectedTracks; } }
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
    private List<ITrackSegment> _trackSurrogates = new List<ITrackSegment>();

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
        Invalidate();
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
      ResizingSelection,

      /// <summary>
      ///   The user is almost moving selected items.
      /// </summary>
      RequestMovingSelection,

      /// <summary>
      ///   The user is almost resizing the selected tracks.
      /// </summary>
      RequestResizingSelection,

      /// <summary>
      ///   The user is scrubbing the playhead.
      /// </summary>
      TimeScrub
    }
    #endregion

    #region Constructor
    /// <summary>
    ///   Construct a new timeline.
    /// </summary>
    public TimelineBase() {
      InitializeComponent();
      SetStyle(
        ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.UserPaint, true );

      // Set up the font to use to draw the track labels
      float emHeightForLabel = EmHeightForLabel( "WM_g^~", TrackHeight );
      _labelFont = new Font( DefaultFont.FontFamily, emHeightForLabel - 2 );
    }
    #endregion

    /// <summary>
    ///   Add a track segment to the timeline.
    /// </summary>
    /// <param name="trackSegment">The track segment to add.</param>
    public void AddTrack( ITrackSegment trackSegment ) {
      _tracks.Add( new SingleElementToTrackWrapper( trackSegment ) );
      RecalculateScrollbarBounds();
      Invalidate();
    }

    /// <summary>
    ///   Add a track to the timeline which contains multiple other track segments.
    /// </summary>
    /// <param name="track"></param>
    public void AddTrack( ITrack track ) {
      _tracks.Add( track );
      RecalculateScrollbarBounds();
      Invalidate();
    }

    /// <summary>
    ///   Invoked when the external clock is updated.
    /// </summary>
    public void Tick() {
      if( Clock.IsRunning ) {
        Invalidate();
      }
    }

    #region Helpers
    /// <summary>
    ///   Recalculates appropriate values for scrollbar bounds.
    /// </summary>
    protected virtual void RecalculateScrollbarBounds() {
      ScrollbarV.Max = (int)( ( _tracks.Count * ( TrackHeight + TrackSpacing ) ) * _renderingScale.Y );
      ScrollbarH.Max = (int)( _tracks.Max( t => t.TrackElements.Any() ? t.TrackElements.Max( te => te.End ) : 0 ) * _renderingScale.X );
      ScrollbarV.Refresh();
      ScrollbarH.Refresh();
    }

    /// <summary>
    ///   Calculate the rectangle within which track segment should be drawn.
    /// </summary>
    /// <returns>The rectangle within which all tracks should be drawn.</returns>
    internal Rectangle GetTrackAreaBounds() {
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
    ///   Check if a track segment is located at the given position.
    /// </summary>
    /// <param name="test">The point to test for.</param>
    /// <returns>
    ///   The <see cref="ITrackSegment" /> if there is one under the given point; <see langword="null" /> otherwise.
    /// </returns>
    private ITrackSegment TrackHitTest( PointF test ) {
      foreach( ITrackSegment track in _tracks.SelectMany( t => t.TrackElements ) ) {
        // The extent of the track segment, including the border
        RectangleF trackExtent = BoundsHelper.GetTrackExtents( track, this );

        if( trackExtent.Contains( test ) ) {
          return track;
        }
      }

      return null;
    }

    /// <summary>
    ///   Check if a track label is located at the given position.
    /// </summary>
    /// <param name="test">The point to test for.</param>
    /// <returns>The index of the track the hit label belongs to, if one was hit; -1 otherwise.</returns>
    private int TrackLabelHitTest( PointF test ) {
      if( test.X > 0 && test.X < TrackLabelWidth ) {
        return TrackIndexAtPoint( test );
      } else {
        return -1;
      }
    }

    /// <summary>
    ///   Get the index of the track that sits at a certain point.
    /// </summary>
    /// <param name="test">The point where to look for a track segment.</param>
    /// <returns>The index of the track if one was found; -1 otherwise.</returns>
    private int TrackIndexAtPoint( PointF test ) {
      for( int index = 0; index < _tracks.Count; index++ ) {
        ITrack track = _tracks[ index ];
        RectangleF trackExtent = BoundsHelper.GetTrackExtents( track.TrackElements.First(), this );

        if( trackExtent.Top < test.Y && trackExtent.Bottom > test.Y ) {
          return index;
        }
      }
      return -1;
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
      Graphics graphics = Graphics.FromHwnd( this.Handle );
      SizeF measured = graphics.MeasureString( label, currentFont );
      while( measured.Height < maxHeight ) {
        size += 1;
        currentFont = new Font( DefaultFont.FontFamily, size );
        measured = graphics.MeasureString( label, currentFont );
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

    /// <summary>
    ///   Set the clock to a position that relates to a given position on the playhead area.
    ///   Current rendering offset and scale will be taken into account.
    /// </summary>
    /// <param name="location">The location on the playhead area.</param>
    private void SetClockFromMousePosition( PointF location ) {
      Rectangle trackAreaBounds = GetTrackAreaBounds();
      // Calculate a clock value for the current X coordinate.
      float clockValue = ( location.X - _renderingOffset.X - trackAreaBounds.X ) * ( 1 / _renderingScale.X ) * 1000f;
      Clock.Value = clockValue;
    }
    #endregion

    #region Drawing Methods
    /// <summary>
    ///   Redraws the timeline.
    ///   Should only be called from WM_PAINT aka OnPaint.
    /// </summary>
    private void Redraw( Graphics graphics ) {
      // Clear the buffer
      graphics.Clear( BackgroundColor );

      DrawBackground( graphics );
      DrawTracks( _tracks.SelectMany( t => t.TrackElements ), graphics );
      DrawTracks( _trackSurrogates, graphics );
      DrawSelectionRectangle( graphics );

      // Draw labels after the tracks to draw over elements that are partially moved out of the viewing area
      DrawTrackLabels( graphics );

      DrawPlayhead( graphics );

      ScrollbarH.Refresh();
      ScrollbarV.Refresh();
    }

    /// <summary>
    ///   Draws the background of the control.
    /// </summary>
    private void DrawBackground( Graphics graphics ) {

      Rectangle trackAreaBounds = GetTrackAreaBounds();

      // Draw horizontal grid.
      // Grid is white so just take the alpha as the white value.
      Pen gridPen = new Pen( Color.FromArgb( GridAlpha, GridAlpha, GridAlpha ) );
      // Calculate the Y position of the first line.
      int firstLineY = (int)( TrackHeight * _renderingScale.Y + trackAreaBounds.Y + _renderingOffset.Y );
      // Calculate the distance between each following line.
      int actualRowHeight = (int)( ( TrackHeight ) * _renderingScale.Y + TrackSpacing );
      actualRowHeight = Math.Max( 1, actualRowHeight );
      // Draw the actual lines.
      for( int y = firstLineY; y < Height; y += actualRowHeight ) {
        graphics.DrawLine( gridPen, trackAreaBounds.X, y, trackAreaBounds.Width, y );
      }

      // The distance between the minor ticks.
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
      if( minorTickDistance > 5.0f ) {
        using( Pen minorGridPen = new Pen( Color.FromArgb( 30, 30, 30 ) ) {
          DashStyle = DashStyle.Dot
        } ) {
          for( float x = minorTickOffset; x < Width; x += minorTickDistance ) {
            graphics.DrawLine( minorGridPen, trackAreaBounds.X + x, trackAreaBounds.Y, trackAreaBounds.X + x, trackAreaBounds.Height );
          }
        }
      }

      // We start one tick distance after the offset to draw the first line that is actually in the display area
      // The one that is only tickOffset pixels away it behind the track labels.
      int minutePenColor = (int)( 255 * Math.Min( 255, GridAlpha * 2 ) / 255f );
      Pen brightPen = new Pen( Color.FromArgb( minutePenColor, minutePenColor, minutePenColor ) );
      for( int x = tickOffset + tickDistance; x < Width; x += columnWidth ) {
        // Every 60 ticks, we put a brighter, thicker line.
        Pen penToUse;
        if( ( x - minuteOffset ) % minuteDistance == 0 ) {
          penToUse = brightPen;
        } else {
          penToUse = gridPen;
        }

        graphics.DrawLine( penToUse, trackAreaBounds.X + x, trackAreaBounds.Y, trackAreaBounds.X + x, trackAreaBounds.Height );
      }

      gridPen.Dispose();
      brightPen.Dispose();
    }

    /// <summary>
    ///   Draw a list of tracks onto the timeline.
    /// </summary>
    /// <param name="tracks">The tracks to draw.</param>
    protected virtual void DrawTracks( IEnumerable<ITrackSegment> tracks, Graphics graphics ) {

      Rectangle trackAreaBounds = GetTrackAreaBounds();

      // Generate colors for the tracks.
      List<Color> colors = ColorHelper.GetRandomColors( tracks.Count() );

      foreach( ITrackSegment track in tracks ) {
        // The extent of the track segment, including the border.
        RectangleF trackExtent = BoundsHelper.GetTrackExtents( track, this );

        // Don't draw track segments that aren't within the target area.
        if( !trackAreaBounds.IntersectsWith( trackExtent.ToRectangle() ) ) {
          continue;
        }

        // The index of this track segment (or the one it's a substitute for).
        int trackIndex = TrackIndexForTrack( track );

        // Determine colors for this track segment.
        Color trackColor = ColorHelper.AdjustColor( colors[ trackIndex ], 0, -0.1, -0.2 );
        Color borderColor = Color.FromArgb( 128, Color.Black );

        if( _selectedTracks.Contains( track ) ) {
          borderColor = Color.WhiteSmoke;
        }

        // Draw the main track segment area.
        if( track is TrackSegmentSurrogate ) {
          // Draw surrogates with a transparent brush.
          graphics.FillRectangle( new SolidBrush( Color.FromArgb( 128, trackColor ) ), trackExtent );
        } else {
          graphics.FillRectangle( new SolidBrush( trackColor ), trackExtent );
        }

        // Compensate for border size
        trackExtent.X += TrackBorderSize / 2f;
        trackExtent.Y += TrackBorderSize / 2f;
        trackExtent.Height -= TrackBorderSize;
        trackExtent.Width -= TrackBorderSize;

        graphics.DrawRectangle( new Pen( borderColor, TrackBorderSize ), trackExtent.X, trackExtent.Y, trackExtent.Width, trackExtent.Height );
      }
    }

    /// <summary>
    ///   Draw the labels next to each track.
    /// </summary>
    private void DrawTrackLabels( Graphics graphics ) {
      foreach( ITrack track in _tracks ) {
        if( !track.TrackElements.Any() ) continue;
        // We just need the height and Y-offset, so we get the extents of the first track segment.
        RectangleF trackExtents = BoundsHelper.GetTrackExtents( track.TrackElements.First(), this );
        RectangleF labelRect = new RectangleF( 0, trackExtents.Y, TrackLabelWidth, trackExtents.Height );
        graphics.FillRectangle( new SolidBrush( Color.FromArgb( 30, 30, 30 ) ), labelRect );
        graphics.DrawString( track.Name, _labelFont, Brushes.LightGray, labelRect );
      }
    }

    /// <summary>
    ///   Draw a playhead on the timeline.
    ///   The playhead indicates a time value.
    /// </summary>
    private void DrawPlayhead( Graphics graphics ) {
      // Only draw a playhead if we have a clock set.
      if( null != Clock ) {
        // Calculate the position of the playhead.
        Rectangle trackAreaBounds = GetTrackAreaBounds();

        // Draw a background for the playhead. This also overdraws elements that drew into the playhead area.
        graphics.FillRectangle( Brushes.Black, 0, 0, Width, _playheadExtents.Height );

        float playheadOffset = (float)( trackAreaBounds.X + ( Clock.Value * 0.001f ) * _renderingScale.X ) + _renderingOffset.X;
        // Don't draw when not in view.
        if( playheadOffset < trackAreaBounds.X || playheadOffset > trackAreaBounds.X + trackAreaBounds.Width ) {
          return;
        }

        // Draw the playhead as a single line.
        graphics.DrawLine( Pens.SpringGreen, playheadOffset, trackAreaBounds.Y, playheadOffset, trackAreaBounds.Height );

        graphics.FillRectangle( Brushes.SpringGreen, playheadOffset - _playheadExtents.Width / 2, 0, _playheadExtents.Width, _playheadExtents.Height );
      }
    }

    /// <summary>
    ///   Draws the selection rectangle the user is drawing.
    /// </summary>
    private void DrawSelectionRectangle( Graphics graphics ) {
      graphics.DrawRectangle(
        new Pen( Color.LightGray, 1 ) {
          DashStyle = DashStyle.Dot
        }, _selectionRectangle.ToRectangle() );
    }

    /// <summary>
    ///   Retrieve the track index of a given track segment.
    ///   If the track segment is a surrogate, returns the index of the track segment it's a substitute for.
    /// </summary>
    /// <param name="trackSegment">The track segment for which to retrieve the index.</param>
    /// <returns>The index of the track segment or the index the track segment is a substitute for.</returns>
    internal int TrackIndexForTrack( ITrackSegment trackSegment ) {
      ITrackSegment trackSegmentToLookFor = trackSegment;
      if( trackSegment is TrackSegmentSurrogate ) {
        trackSegmentToLookFor = ( (TrackSegmentSurrogate)trackSegment ).SubstituteFor;
      }
      return _tracks.FindIndex( t => t.TrackElements.Contains( trackSegmentToLookFor ) );
    }
    #endregion

    #region Event Handler
    /// <summary>
    ///   Invoked when the control is repainted
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint( PaintEventArgs e ) {
      base.OnPaint( e );

      Redraw( e.Graphics );
    }

    /// <summary>
    ///   Invoked when the cursor is moved over the control.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseMove( MouseEventArgs e ) {
      base.OnMouseMove( e );

      // Store the current mouse position.
      PointF location = new PointF( e.X, e.Y );
      // Check if there is a track segment at the current mouse position.
      ITrackSegment focusedTrackSegment = TrackHitTest( location );

      // Is the left mouse button pressed?
      if( ( e.Button & MouseButtons.Left ) != 0 ) {
        if( CurrentMode == BehaviorMode.MovingSelection ) {
          // Indicate ability to move though cursor.
          Cursor = Cursors.SizeWE;

          // Calculate the movement delta.
          PointF delta = PointF.Subtract( location, new SizeF( _dragOrigin ) );
          // Adjust the delta so that all selected tracks can be moved without collisions.
          delta = AcceptableMovementDelta( delta );

          // Apply the delta to all selected tracks
          foreach( TrackSegmentSurrogate selectedTrack in _trackSurrogates ) {
            // Store the length of this track segment
            float length = selectedTrack.SubstituteFor.End - selectedTrack.SubstituteFor.Start;

            // Calculate the proposed new start for the track segment depending on the given delta.
            float proposedStart = Math.Max( 0, selectedTrack.SubstituteFor.Start + ( delta.X * ( 1 / _renderingScale.X ) ) );
            // Snap to next full value
            if( !IsKeyDown( Keys.Alt ) ) {
              proposedStart = (float)Math.Round( proposedStart );
            }

            selectedTrack.Start = proposedStart;
            selectedTrack.End = proposedStart + length;
          }

          // Force a redraw.
          Invalidate();

        } else if( CurrentMode == BehaviorMode.ResizingSelection ) {
          // Indicate ability to resize though cursor.
          Cursor = Cursors.SizeWE;

          // Calculate the movement delta.
          PointF delta = PointF.Subtract( location, new SizeF( _dragOrigin ) );

          foreach( TrackSegmentSurrogate selectedTrack in _trackSurrogates ) {
            // Initialize the proposed start and end with the current track segment values for now.
            float proposedStart = selectedTrack.SubstituteFor.Start;
            float proposedEnd = selectedTrack.SubstituteFor.End;

            // Apply the delta to the start or end of the timeline track segment,
            // depending on the edge where the user originally started the resizing operation.
            if( ( _activeEdge & RectangleHelper.Edge.Left ) != 0 ) {
              // Adjust the delta so that all selected tracks can be resized without collisions.
              delta = AcceptableResizingDelta( delta, true );
              proposedStart = Math.Max( 0, proposedStart + delta.X * ( 1 / _renderingScale.X ) );
              // Snap to next full value
              if( !IsKeyDown( Keys.Alt ) ) {
                proposedStart = (float)Math.Round( proposedStart );
              }

            } else if( ( _activeEdge & RectangleHelper.Edge.Right ) != 0 ) {
              // Adjust the delta so that all selected tracks can be resized without collisions.
              delta = AcceptableResizingDelta( delta, false );
              proposedEnd = Math.Max( 0, proposedEnd + ( delta.X * ( 1 / _renderingScale.X ) ) );
              // Snap to next full value
              if( !IsKeyDown( Keys.Alt ) ) {
                proposedEnd = (float)Math.Round( proposedEnd );
              }
            }

            selectedTrack.Start = proposedStart;
            selectedTrack.End = proposedEnd;
          }

          // Force a redraw.
          Invalidate();

        } else if( CurrentMode == BehaviorMode.Selecting ) {
          // Set the appropriate cursor for a selection action.
          Cursor = Cursors.Cross;

          // Construct the correct rectangle spanning from the selection origin to the current cursor position.
          _selectionRectangle = RectangleHelper.Normalize( _selectionOrigin, location ).ToRectangle();

          Invalidate();

        } else if( CurrentMode == BehaviorMode.RequestMovingSelection || CurrentMode == BehaviorMode.RequestResizingSelection ) {
          // A previous action would like a dragging operation to start.

          // Calculate the movement delta.
          PointF delta = PointF.Subtract( location, new SizeF( _dragOrigin ) );

          // Check if the user has moved the mouse far enough to trigger the dragging operation.
          if( Math.Sqrt( delta.X * delta.X + delta.Y * delta.Y ) > DraggingThreshold ) {
            // Start the requested dragging operation.
            if( CurrentMode == BehaviorMode.RequestMovingSelection ) {
              CurrentMode = BehaviorMode.MovingSelection;
            } else if( CurrentMode == BehaviorMode.RequestResizingSelection ) {
              CurrentMode = BehaviorMode.ResizingSelection;
            }

            // Create and store surrogates for selected timeline tracks.
            _trackSurrogates = SurrogateHelper.GetSurrogates( _selectedTracks );
          }

        } else if( CurrentMode == BehaviorMode.TimeScrub ) {
          SetClockFromMousePosition( location );
          Invalidate();
        }

      } else if( ( e.Button & MouseButtons.Middle ) != 0 ) {
        // Pan the view
        // Calculate the movement delta.
        PointF delta = PointF.Subtract( location, new SizeF( _panOrigin ) );
        // Now apply the delta to the rendering offsets to pan the view.
        _renderingOffset = PointF.Add( _renderingOffsetBeforePan, new SizeF( delta ) );

        // Make sure to stay within bounds.
        _renderingOffset.X = Math.Max( -ScrollbarH.Max, Math.Min( -ScrollbarH.Min, _renderingOffset.X ) );
        _renderingOffset.Y = Math.Max( -ScrollbarV.Max, Math.Min( -ScrollbarV.Min, _renderingOffset.Y ) );

        // Update scrollbar positions. This will invoke a redraw.
        ScrollbarH.Value = (int)( -_renderingOffset.X );
        ScrollbarV.Value = (int)( -_renderingOffset.Y );

      } else {
        // No mouse button is being pressed
        if( null != focusedTrackSegment ) {
          RectangleF trackExtents = BoundsHelper.GetTrackExtents( focusedTrackSegment, this );
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
    ///   Calculate which movement delta would be acceptable to apply to all currently selected tracks.
    /// </summary>
    /// <param name="delta">The suggested movement delta.</param>
    /// <returns>The adjusted movement delta that is acceptable for all selected tracks.</returns>
    private PointF AcceptableMovementDelta( PointF delta ) {
      PointF lastDelta;
      do {
        lastDelta = delta;

        foreach( TrackSegmentSurrogate selectedTrack in _trackSurrogates ) {
          // Store the length of this track segment
          float length = selectedTrack.SubstituteFor.End - selectedTrack.SubstituteFor.Start;

          // Calculate the proposed new start for the track segment depending on the given delta.
          float proposedStart = Math.Max( 0, selectedTrack.SubstituteFor.Start + ( delta.X * ( 1 / _renderingScale.X ) ) );

          // If the movement on this track segment would move it to the start of the timeline,
          // cap the movement for all tracks.
          // TODO: It could be interesting to enable this anyway through a modifier key.
          if( proposedStart <= 0 ) {
            delta.X = -selectedTrack.SubstituteFor.Start * _renderingScale.X;
            proposedStart = Math.Max( 0, selectedTrack.SubstituteFor.Start + ( delta.X * ( 1 / _renderingScale.X ) ) );
          }

          // Get the index of the selected track segment to use it as a basis for calculating the proposed new bounding box.
          int trackIndex = TrackIndexForTrack( selectedTrack );
          // Use the calculated values to get a full screen-space bounding box for the proposed track segment location.
          RectangleF proposed = BoundsHelper.RectangleToTrackExtents(
            new RectangleF {
              X = proposedStart,
              Width = length,
            }, this, trackIndex );

          TrackSegmentSurrogate trackSegment = selectedTrack;
          IOrderedEnumerable<ITrackSegment> sortedTracks =
            // All track segments on the same track as the selected one.
            _tracks[ trackIndex ].TrackElements
              // Remove the selected tracks and the one we're the substitute for.
                                 .Where( t => t != trackSegment.SubstituteFor && !_selectedTracks.Contains( t ) )
              // Sort all by their position on the track.
                                 .OrderBy( t => t.Start );

          if( BoundsHelper.IntersectsAny( proposed, sortedTracks.Select( t => BoundsHelper.GetTrackExtents( t, this ) ) ) ) {
            // Let's grab a list of the tracks so we can iterate by index.
            List<ITrackSegment> sortedTracksList = sortedTracks.ToList();
            // If delta points towards left, walk the sorted tracks from the left (respecting the proposed start)
            // and try to find a non-colliding window between track segments.
            if( delta.X < 0 ) {
              for( int elementIndex = 0; elementIndex < sortedTracksList.Count(); elementIndex++ ) {
                // If the right edge of the element is left of our proposed start, then it's too far left to be interesting.
                if( sortedTracksList[ elementIndex ].End < proposedStart ) {
                  continue;
                }

                // The right edge of the element is right of our proposed start. So this is at least the first one we intersect with.
                // So we'll move our proposed start at the end of that element. However, this could cause another collision at the end of our element.
                proposedStart = sortedTracksList[ elementIndex ].End;

                // Are we at the last element? Then there's no need to check further, we can always snap here.
                if( elementIndex == sortedTracksList.Count - 1 ) {
                  break;
                }

                // Does the next segment in line collide with the end of our selected track segment?
                if( sortedTracksList[ elementIndex + 1 ].Start < proposedStart + length ) {
                  continue;
                }

                break;
              }

            } else if( delta.X > 0 ) {
              // If delta points right, walk the sorted tracks from the right and do the same thing as above.
              for( int elementIndex = sortedTracksList.Count() - 1; elementIndex >= 0; elementIndex-- ) {
                // If the left edge of the element is right of our proposed end, then it's too far right to be interesting.
                if( sortedTracksList[ elementIndex ].Start > proposedStart + length ) {
                  continue;
                }

                // The left edge of the element is left of our proposed end. So this is at least the first one we intersect with.
                // So we'll move our proposed end at the start of that element. However, this could cause another collision at the start of our element.
                proposedStart = sortedTracksList[ elementIndex ].Start - length;

                // Are we at the first element? Then there's no need to check further, we can always snap here.
                // We can always snap because we're moving right and this is the first element that is not ourself.
                // So we were placed in front of it anyway and we're now just moving closer to it.
                if( elementIndex == 0 ) {
                  break;
                }

                // Does the next segment in line collide with the start of our selected track segment?
                if( sortedTracksList[ elementIndex - 1 ].End > proposedStart ) {
                  continue;
                }

                break;
              }
            }

            if( delta.X < 0 ) {
              delta.X = Math.Max( delta.X, ( proposedStart - selectedTrack.SubstituteFor.Start ) * _renderingScale.X );
            } else {
              delta.X = Math.Min( delta.X, ( proposedStart - selectedTrack.SubstituteFor.Start ) * _renderingScale.X );
            }
          }

          // If the delta is nearing zero, bail out.
          if( Math.Abs( delta.X ) < 0.001f ) {
            delta.X = 0;
            return delta;
          }
        }
      } while( !lastDelta.Equals( delta ) );

      return delta;
    }

    /// <summary>
    ///   Calculate which resizing delta would be acceptable to apply to all currently selected tracks.
    /// </summary>
    /// <param name="delta">The suggested resizing delta.</param>
    /// <param name="adjustStart">
    ///   Set to <see langword="true" /> if the left (start) edge of the focused element is being resized. Set to
    ///   <see langword="false" /> if the right (end) edge of the focus element is being resized.
    /// </param>
    /// <returns>The adjusted resizing delta that is acceptable for all selected tracks.</returns>
    private PointF AcceptableResizingDelta( PointF delta, bool adjustStart ) {
      foreach( TrackSegmentSurrogate selectedTrack in _trackSurrogates ) {
        // Calculate the proposed new start and end for the track segment depending on the adjustStart parameter and given delta..
        float proposedStart = ( !adjustStart ) ? selectedTrack.SubstituteFor.Start : Math.Max( 0, selectedTrack.SubstituteFor.Start + ( delta.X * ( 1 / _renderingScale.X ) ) );
        float proposedEnd = ( adjustStart ) ? selectedTrack.SubstituteFor.End : Math.Max( 0, selectedTrack.SubstituteFor.End + ( delta.X * ( 1 / _renderingScale.X ) ) );
        // Get the index of the selected track segment to use it as a basis for calculating the proposed new bounding box.
        int trackIndex = TrackIndexForTrack( selectedTrack );
        // Use the calculated values to get a full screen-space bounding box for the proposed track segment location.
        RectangleF proposed = BoundsHelper.RectangleToTrackExtents(
          new RectangleF {
            X = proposedStart,
            Width = proposedEnd - proposedStart,
          }, this, trackIndex );

        // If the movement on this track segment would move it to the start of the timeline,
        // cap the movement for all tracks.
        // TODO: It could be interesting to enable this anyway through a modifier key.
        if( adjustStart && proposedStart <= 0 ) {
          delta.X = -selectedTrack.SubstituteFor.Start * _renderingScale.X;
          return delta;
        }

        TrackSegmentSurrogate trackSegment = selectedTrack;
        IOrderedEnumerable<ITrackSegment> sortedTracks =
          // All track segments on the same track as the selected one.
          _tracks[ trackIndex ].TrackElements
            // Add all track segment surrogates on the same track (except ourself).
                               .Concat( _trackSurrogates.Where( t => t != trackSegment && TrackIndexForTrack( t ) == trackIndex ) )
            // Remove the selected track segments and the one we're the substitute for.
                               .Where( t => t != trackSegment.SubstituteFor && !_selectedTracks.Contains( t ) )
            // Sort all by their position on the track.
                               .OrderBy( t => t.Start );

        if( BoundsHelper.IntersectsAny( proposed, sortedTracks.Select( t => BoundsHelper.GetTrackExtents( t, this ) ) ) ) {
          // Let's grab a list of the tracks so we can iterate by index.
          List<ITrackSegment> sortedTracksList = sortedTracks.ToList();
          if( adjustStart ) {
            for( int elementIndex = sortedTracksList.Count() - 1; elementIndex >= 0; elementIndex-- ) {
              if( sortedTracksList[ elementIndex ].Start >= selectedTrack.SubstituteFor.Start ) {
                continue;
              }

              proposedStart = sortedTracksList[ elementIndex ].End;
              break;
            }

            // Write back delta depending on calculated start value.
            if( delta.X < 0 ) {
              delta.X = Math.Max( delta.X, ( proposedStart - selectedTrack.SubstituteFor.Start ) * _renderingScale.X );
            } else {
              delta.X = Math.Min( delta.X, ( proposedStart - selectedTrack.SubstituteFor.Start ) * _renderingScale.X );
            }
          } else {
            for( int elementIndex = 0; elementIndex < sortedTracksList.Count(); elementIndex++ ) {
              if( sortedTracksList[ elementIndex ].End <= selectedTrack.SubstituteFor.Start ) {
                continue;
              }

              proposedEnd = sortedTracksList[ elementIndex ].Start;
              break;
            }

            // Write back delta depending on calculated end value.
            if( delta.X < 0 ) {
              delta.X = Math.Max( delta.X, ( proposedEnd - selectedTrack.SubstituteFor.End ) * _renderingScale.X );
            } else {
              delta.X = Math.Min( delta.X, ( proposedEnd - selectedTrack.SubstituteFor.End ) * _renderingScale.X );
            }

          }
        }

        // If the delta is nearing zero, bail out.
        if( Math.Abs( 0 - delta.X ) < 0.001f ) {
          return delta;
        }
      }

      return delta;
    }

    /// <summary>
    ///   Invoked when the user presses a mouse button over the control.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDown( MouseEventArgs e ) {
      base.OnMouseDown( e );

      Focus();

      // Store the current mouse position.
      PointF location = new PointF( e.X, e.Y );

      if( ( e.Button & MouseButtons.Left ) != 0 ) {
        // Check if there is a track segment at the current mouse position.
        ITrackSegment focusedTrackSegment = TrackHitTest( location );

        if( null != focusedTrackSegment ) {
          // Was this track segment already selected?
          if( !_selectedTracks.Contains( focusedTrackSegment ) ) {
            // Notify everyone that the track segment was selected.
            InvokeSelectionChanged( new SelectionChangedEventArgs( focusedTrackSegment.Yield(), null ) );
            // Clear the selection, unless the user is picking
            if( !IsKeyDown( Keys.Control ) ) {
              InvokeSelectionChanged( new SelectionChangedEventArgs( null, _selectedTracks ) );
              _selectedTracks.Clear();
            }

            // Add track segment to selection.
            _selectedTracks.Add( focusedTrackSegment );

            // If the track segment was already selected and Ctrl is down
            // then the user is picking and we want to remove the track segment from the selection.
          } else if( IsKeyDown( Keys.Control ) ) {
            _selectedTracks.Remove( focusedTrackSegment );
            InvokeSelectionChanged( new SelectionChangedEventArgs( null, focusedTrackSegment.Yield() ) );
          }

          // Store the current mouse position. It'll be used later to calculate the movement delta.
          _dragOrigin = location;

          // Check whether the user wants to move or resize the selected tracks.
          RectangleF trackExtents = BoundsHelper.GetTrackExtents( focusedTrackSegment, this );
          RectangleHelper.Edge isPointOnEdge = RectangleHelper.IsPointOnEdge( trackExtents, location, 3f, RectangleHelper.EdgeTest.Horizontal );
          if( isPointOnEdge != RectangleHelper.Edge.None ) {
            CurrentMode = BehaviorMode.RequestResizingSelection;
            _activeEdge = isPointOnEdge;
          } else {
            CurrentMode = BehaviorMode.RequestMovingSelection;
          }

        } else if( location.Y < _playheadExtents.Height ) {
          CurrentMode = BehaviorMode.TimeScrub;
          SetClockFromMousePosition( location );

        } else {
          // Clear the selection, unless the user is picking
          if( !IsKeyDown( Keys.Control ) ) {
            InvokeSelectionChanged( new SelectionChangedEventArgs( null, _selectedTracks ) );
            _selectedTracks.Clear();
          }

          // Remember this location as the origin for the selection.
          _selectionOrigin = location;

          CurrentMode = BehaviorMode.Selecting;
        }

      } else if( ( e.Button & MouseButtons.Middle ) != 0 ) {
        _panOrigin = location;
        _renderingOffsetBeforePan = _renderingOffset;
      }

      Invalidate();
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
          // Are we on the track label column?
          int trackIndex = TrackLabelHitTest( location );
          if( -1 < trackIndex ) {
            ITrack track = _tracks[ trackIndex ];

            // SingleElementToTrackWrapper instances are implicitly created by the timeline itself.
            // There is no need to invoke the method, as the single track segment it contains will be notified by the loop below.
            if( !( track is SingleElementToTrackWrapper ) ) {
              InvokeSelectionChanged( new SelectionChangedEventArgs( track.Yield(), null ) );
            }

            foreach( ITrackSegment trackElement in track.TrackElements ) {
              // Toggle track segment in and out of selection.
              if( _selectedTracks.Contains( trackElement ) ) {
                _selectedTracks.Remove( trackElement );
                InvokeSelectionChanged( new SelectionChangedEventArgs( null, trackElement.Yield() ) );
              } else {
                _selectedTracks.Add( trackElement );
                InvokeSelectionChanged( new SelectionChangedEventArgs( trackElement.Yield(), null ) );
              }
            }

          } else {
            // If we were selecting, it's now time to finalize the selection
            // Construct the correct rectangle spanning from the selection origin to the current cursor position.
            RectangleF selectionRectangle = RectangleHelper.Normalize( _selectionOrigin, location );

            foreach( ITrackSegment track in _tracks.SelectMany( t => t.TrackElements ) ) {
              RectangleF boundingRectangle = BoundsHelper.GetTrackExtents( track, this );

              // Check if the track segment is selected by the selection rectangle.
              if( SelectionHelper.IsSelected( selectionRectangle, boundingRectangle, ModifierKeys ) ) {
                // Toggle track segment in and out of selection.
                if( _selectedTracks.Contains( track ) ) {
                  _selectedTracks.Remove( track );
                  InvokeSelectionChanged( new SelectionChangedEventArgs( null, track.Yield() ) );
                } else {
                  _selectedTracks.Add( track );
                  InvokeSelectionChanged( new SelectionChangedEventArgs( track.Yield(), null ) );
                }
              }
            }
          }

        } else if( CurrentMode == BehaviorMode.MovingSelection || CurrentMode == BehaviorMode.ResizingSelection ) {
          // The moving operation ended, apply the values of the surrogates to the originals
          foreach( TrackSegmentSurrogate surrogate in _trackSurrogates ) {
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

      Invalidate();
    }

    /// <summary>
    ///   Invoked when a key is released.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnKeyUp( KeyEventArgs e ) {
      base.OnKeyUp( e );

      if( e.KeyCode == Keys.A && IsKeyDown( Keys.Control ) ) {
        // Ctrl+A - Select all
        InvokeSelectionChanged( new SelectionChangedEventArgs( null, _selectedTracks ) );
        _selectedTracks.Clear();
        foreach( ITrackSegment track in _tracks.SelectMany( t => t.TrackElements ) ) {
          _selectedTracks.Add( track );
        }
        InvokeSelectionChanged( new SelectionChangedEventArgs( _selectedTracks, null ) );
        Invalidate();

      } else if( e.KeyCode == Keys.D && IsKeyDown( Keys.Control ) ) {
        // Ctrl+D - Deselect all
        InvokeSelectionChanged( new SelectionChangedEventArgs( null, _selectedTracks ) );
        _selectedTracks.Clear();
        Invalidate();
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
      Invalidate();
    }

    /// <summary>
    ///   Invoked when the horizontal scrollbar is being scrolled.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScrollbarHScroll( object sender, ScrollEventArgs e ) {
      _renderingOffset.X = -e.NewValue;
      Invalidate();
    }

    /// <summary>
    ///   Invoked when the user scrolls the mouse wheel.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseWheel( MouseEventArgs e ) {
      base.OnMouseWheel( e );

      if( IsKeyDown( Keys.Alt ) ) {
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
        // If Alt isn't down, we're scrolling/panning.
        if( IsKeyDown( Keys.Control ) ) {
          // If Ctrl is down, we're scrolling horizontally.
          ScrollbarH.Value -= e.Delta / 10;
        } else {
          // If no modifier keys are down, we're scrolling vertically.
          ScrollbarV.Value -= e.Delta / 10;
        }
      }

      Invalidate();
    }
    #endregion
  }
}