using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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
      MovingSelection
    }
    #endregion

    /// <summary>
    ///   Construct a new timeline.
    /// </summary>
    public Timeline() {
      InitializeComponent();
      InitializePixelMap();
    }

    /// <summary>
    ///   Add a track to the timeline.
    /// </summary>
    /// <param name="track">The track to add.</param>
    public void AddTrack( ITimelineTrack track ) {
      _tracks.Add( track );
      Redraw();
      Refresh();
    }

    #region Drawing Methods
    /// <summary>
    ///   Redraws the timeline.
    /// </summary>
    private void Redraw() {
      // Clear the buffer
      GraphicsContainer.Clear( BackgroundColor );

      DrawTracks( _trackSurrogates );
      DrawTracks( _tracks );
    }

    /// <summary>
    ///   Draw a list of tracks onto the timeline.
    /// </summary>
    /// <param name="tracks">The tracks to draw.</param>
    private void DrawTracks( IList<ITimelineTrack> tracks ) {
      // Generate colors for the tracks.
      List<Color> colors = ColorHelper.GetRandomColors( _tracks.Count );

      foreach( ITimelineTrack track in tracks ) {
        // The index of this track (or the one it's a substitute for).
        int trackIndex = TrackIndexForTrack( track );
        // Offset the next track to the appropriate position.
        int trackOffset = (TrackHeight + TrackSpacing) * trackIndex;

        // Determine colors for this track
        Color trackColor = colors[ trackIndex ];
        Color borderColor = Color.Black;

        if( _selectedTracks.Contains( track ) ) {
          borderColor = Color.WhiteSmoke;
        }

        // The extent of the track, including the border
        RectangleF trackExtent = new RectangleF( track.Start, trackOffset, track.End - track.Start, TrackHeight );

        // Draw the main track area.
        if( track is TrackSurrogate ) {
          // Draw surrogates with a hatched brush.
          GraphicsContainer.FillRectangle( new HatchBrush( HatchStyle.DiagonalCross, trackColor ), trackExtent );
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
    /// Retrieve the index of a given track.
    /// If the track is a surrogate, returns the index of the track it's a substitute for.
    /// </summary>
    /// <param name="track">The track for which to retrieve the index.</param>
    /// <returns>The index of the track or the index the track is a substitute for.</returns>
    private int TrackIndexForTrack( ITimelineTrack track ) {
      ITimelineTrack trackToLookFor = track;
      if( track is TrackSurrogate ) {
        trackToLookFor = ((TrackSurrogate)track).SubstituteFor;
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
      int trackOffset = 0;
      foreach( ITimelineTrack track in _tracks ) {
        // The extent of the track, including the border
        RectangleF trackExtent = new RectangleF( track.Start - TrackBorderSize, trackOffset, track.End + TrackBorderSize, TrackHeight + TrackBorderSize * 2 );

        if( trackExtent.Contains( test ) ) {
          return track;
        }

        // Offset the next track to the appropriate position.
        trackOffset += ( TrackBorderSize * 2 ) + TrackHeight;
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

    /// <summary>
    ///   Invoked once the timeline has loaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimelineLoad( object sender, EventArgs e ) {
      Redraw();
      Refresh();
    }

    /// <summary>
    ///   Invoked when the cursor is moved over the control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="InvalidOperationException">Selection origin not set. This shouldn't happen.</exception>
    private void TimelineMouseMove( object sender, MouseEventArgs e ) {
      // Is the left mouse button pressed?
      if( ( e.Button & MouseButtons.Left ) != 0 ) {
        // Store the current mouse position.
        PointF location = new PointF( e.X, e.Y );
        // Check if there is a track at the current mouse position.
        ITimelineTrack focusedTrack = TrackHitTest( location );

        //if( null != focusedTrack && focusedTrack == _selectedTracks ) {
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
          Redraw();
          Refresh();

        } else if( CurrentMode == BehaviorMode.Selecting ) {
          if( !_selectionOrigin.HasValue ) {
            throw new InvalidOperationException( "Selection origin not set. This shouldn't happen." );
          }

          // Set the appropriate cursor for a selection action.
          Cursor = Cursors.Cross;

          // Construct the correct rectangle spanning from the selection origin to the current cursor position.
          Rectangle selectionRectangle = RectangleHelper.Normalize( _selectionOrigin.Value, location ).ToRectangle();

          Redraw();
          GraphicsContainer.DrawRectangle( new Pen( Color.LightGray, 1 ), selectionRectangle );
          Refresh();

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

        CurrentMode = BehaviorMode.MovingSelection;

      } else {
        // Reset the track selection.
        _selectedTracks.Clear();

        // Remember this location as the origin for the selection.
        _selectionOrigin = location;

        CurrentMode = BehaviorMode.Selecting;
      }

      Redraw();
      Refresh();
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
          // Construct a rectangle that contains the whole track item.
          RectangleF boundingRectangle = RectangleHelper.Normalize( new PointF( track.Start, trackOffset ), new PointF( track.End, trackOffset + TrackHeight ) );
          // Check if the track item is contained in the selection rectangle.
          if( selectionRectangle.Contains( boundingRectangle ) ) {
            // Add it to the selection.
            _selectedTracks.Add( track );
          }
          trackOffset += ( TrackBorderSize * 2 ) + TrackHeight;
        }

      } else if( CurrentMode == BehaviorMode.MovingSelection ) {
        // The moving operation ended, apply the values of the surrogates to the originals
        foreach( TrackSurrogate surrogate in _trackSurrogates ) {
          surrogate.CopyTo( surrogate.SubstituteFor );
        }
        _trackSurrogates.Clear();
      }

      // Reset cursor
      Cursor = Cursors.Arrow;
      // Reset selection origin.
      _selectionOrigin = null;
      // Reset mode.
      CurrentMode = BehaviorMode.Idle;

      Redraw();
      Refresh();
    }
    #endregion
  }
}