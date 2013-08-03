using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    ///   The currently selected track.
    /// </summary>
    private ITimelineTrack _selectedTrack = null;
    #endregion

    #region Interaction
    /// <summary>
    ///   The origin from where the selected track was moved (during a dragging operation).
    /// </summary>
    private float _selectedTrackOrigin;

    /// <summary>
    ///   The point at where a dragging operation started.
    /// </summary>
    private PointF _dragOrigin;
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

        if( track == _selectedTrack ) {
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
    private void TimelineMouseMove( object sender, MouseEventArgs e ) {
      // Is the left mouse button pressed?
      if( ( e.Button & MouseButtons.Left ) != 0 ) {
        // Store the current mouse position.
        PointF location = new PointF( e.X, e.Y );
        // Check if there is a track at the current mouse position.
        ITimelineTrack focusedTrack = TrackHitTest( location );

        if( null != focusedTrack && focusedTrack == _selectedTrack ) {
          // Indicate ability to move though cursor.
          Cursor = Cursors.SizeWE;
          // Calculate the movement delta.
          PointF delta = PointF.Subtract( location, new SizeF( _dragOrigin ) );
          float length = focusedTrack.End - focusedTrack.Start;
          // Then apply the delta to the track
          focusedTrack.Start = _selectedTrackOrigin + delta.X;
          focusedTrack.End =  focusedTrack.Start + length;

          // Force a redraw.
          Redraw();
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
        _selectedTrack = focusedTrack;
        // Store the current position of the track and the mouse position.
        // We'll use both later to move the track around.
        _selectedTrackOrigin = _selectedTrack.Start;
        _dragOrigin = location;

      } else {
        _selectedTrack = null;
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
    }
    #endregion
  }
}