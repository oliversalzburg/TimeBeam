using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using TimeBeam.Helper;

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
    private List<ITimelineTrack> _selectedTracks = new List<ITimelineTrack>();
    #endregion

    #region Interaction
    /// <summary>
    /// What mode is the timeline currently in?
    /// </summary>
    public BehaviorMode CurrentMode { get; private set; }

    /// <summary>
    ///   The origin from where the selected track was moved (during a dragging operation).
    /// </summary>
    private float _selectedTrackOrigin;

    /// <summary>
    ///   The point at where a dragging operation started.
    /// </summary>
    private PointF _dragOrigin;

    /// <summary>
    /// The point where the user started drawing up a selection rectangle.
    /// </summary>
    private PointF? _selectionOrigin;
    #endregion

    #region Enums
    /// <summary>
    /// Enumerates states the timeline can be in.
    /// These are usually invoked through user interaction.
    /// </summary>
    public enum BehaviorMode {
      /// <summary>
      /// The timeline is idle or not using any more specific state.
      /// </summary>
      Idle,

      /// <summary>
      /// The user is currently in the process of selecting items on the timeline.
      /// </summary>
      Selecting,

      /// <summary>
      /// The user is currently moving selected items.
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

      // Generate colors for the tracks.
      List<Color> colors = ColorHelper.GetRandomColors( _tracks.Count );

      int trackOffset = 0;
      for( int trackIndex = 0; trackIndex < _tracks.Count; trackIndex++ ) {
        ITimelineTrack track = _tracks[ trackIndex ];

        // Determine colors for this track
        Color trackColor = colors[ trackIndex ];
        Color borderColor = Color.Black;

        if( track == _selectedTracks ) {
          borderColor = Color.WhiteSmoke;
        }

        // The extent of the track, including the border
        RectangleF trackExtent = new RectangleF( track.Start, trackOffset, track.End - track.Start, TrackHeight );

        // Draw the main track area.
        GraphicsContainer.FillRectangle( new SolidBrush( trackColor ), trackExtent );

        // Compensate for border size
        trackExtent.X += TrackBorderSize / 2f;
        trackExtent.Y += TrackBorderSize / 2f;
        trackExtent.Height -= TrackBorderSize;
        trackExtent.Width -= TrackBorderSize;

        GraphicsContainer.DrawRectangle( new Pen( borderColor, TrackBorderSize ), trackExtent.X, trackExtent.Y, trackExtent.Width, trackExtent.Height );

        // Offset the next track to the appropriate position.
        trackOffset += TrackHeight + TrackSpacing;
      }
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
          // Calculate the movement delta.
          PointF delta = PointF.Subtract( location, new SizeF( _dragOrigin ) );
          float length = _selectedTracks[ 0 ].End - _selectedTracks[ 0 ].Start;
          // Then apply the delta to the track
          _selectedTracks[ 0 ].Start = _selectedTrackOrigin + delta.X;
          _selectedTracks[ 0 ].End = _selectedTracks[ 0 ].Start + length;

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
          Rectangle selectionRectangle = new Rectangle();
          if( location.X < _selectionOrigin.Value.X ) {
            selectionRectangle.X = (int)location.X;
            selectionRectangle.Width = (int)( _selectionOrigin.Value.X - selectionRectangle.X );
          } else {
            selectionRectangle.X = (int)_selectionOrigin.Value.X;
            selectionRectangle.Width = (int)( location.X - selectionRectangle.X );
          }
          if( location.Y < _selectionOrigin.Value.Y ) {
            selectionRectangle.Y = (int)location.Y;
            selectionRectangle.Height = (int)( _selectionOrigin.Value.Y - selectionRectangle.Y );
          } else {
            selectionRectangle.Y = (int)_selectionOrigin.Value.Y;
            selectionRectangle.Height = (int)( location.Y - selectionRectangle.Y );
          }

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
        // Tell the track that it was selected.
        focusedTrack.Selected();
        // Store a reference to the selected track
        _selectedTracks.Add(  focusedTrack );
        // Store the current position of the track and the mouse position.
        // We'll use both later to move the track around.
        _selectedTrackOrigin = _selectedTracks[0].Start;
        _dragOrigin = location;

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