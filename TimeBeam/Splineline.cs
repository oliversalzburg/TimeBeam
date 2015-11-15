using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TimeBeam.Helper;
using TimeBeam.Surrogates;

namespace TimeBeam {
  /// <summary>
  ///   Used to host splines on a timeline.
  /// </summary>
  public partial class Splineline : TimelineBase {
    #region Constructor
    /// <summary>
    ///   Construct a new timeline.
    /// </summary>
    public Splineline() {
      InitializeComponent();
    }
    #endregion

    /// <summary>
    ///   Recalculates appropriate values for scrollbar bounds.
    /// </summary>
    protected override void RecalculateScrollbarBounds() {
      float smallestY = ( _tracks.Min( t => t.TrackElements.Any() ? t.TrackElements.OfType<ISplineSegment>().Min( te => Math.Min( te.StartValue, te.EndValue ) ) : 0 ) * _renderingScale.Y );
      float largestY = ( _tracks.Max( t => t.TrackElements.Any() ? t.TrackElements.OfType<ISplineSegment>().Max( te => Math.Max( te.StartValue, te.EndValue ) ) : 0 ) * _renderingScale.Y );
      float fullHeight = Math.Abs( smallestY ) + Math.Abs( largestY );
      ScrollbarV.Min = (int)( fullHeight / -2 );
      ScrollbarV.Max = (int)( fullHeight / 2 );
      ScrollbarH.Max = (int)( _tracks.Max( t => t.TrackElements.Any() ? t.TrackElements.Max( te => te.End ) : 0 ) * _renderingScale.X );
      ScrollbarV.Refresh();
      ScrollbarH.Refresh();
    }

    /// <summary>
    ///   Draw a list of splines onto the timeline.
    /// </summary>
    /// <param name="tracks">The splines to draw.</param>
    /// <param name="graphics">The <see cref="Graphics"/> instance to draw into.</param>
    protected override void DrawTracks( IEnumerable<ITrackSegment> tracks, Graphics graphics ) {
      Rectangle trackAreaBounds = GetTrackAreaBounds();

      // Grab only the spline segments. All passed tracks should be spline segments, but just make sure anyway.
      // Also store them in a list to avoid multiple iterations over enumerable.
      List<ISplineSegment> splineSegments = tracks.OfType<ISplineSegment>().ToList();

      // Generate colors for the tracks.
      List<Color> colors = ColorHelper.GetRandomColors( splineSegments.Count() );

      foreach( ISplineSegment splineSegment in splineSegments ) {
        // The extent of the track segment, including the border.
        // Splines all share the same origin, so use index 0.
        RectangleF trackExtent = BoundsHelper.RectangleToTrackExtents( new RectangleF( splineSegment.Start, splineSegment.StartValue, splineSegment.End - splineSegment.Start, splineSegment.EndValue ), this, 0 );
        trackExtent.Offset( 0, trackAreaBounds.Height / 2f - trackExtent.Height / 2f );
        
        // Don't draw track segments that aren't within the target area.
        if( !trackAreaBounds.IntersectsWith( trackExtent.ToRectangle() ) ) {
          continue;
        }

        // The index of this track segment (or the one it's a substitute for).
        int trackIndex = TrackIndexForTrack( splineSegment );

        // Determine colors for this track segment.
        Color trackColor = ColorHelper.AdjustColor( colors[ trackIndex ], 0, -0.1, -0.2 );

        if( _selectedTracks.Contains( splineSegment ) ) {
          trackColor = Color.WhiteSmoke;
        }

        // Draw the main track segment area.
        if( splineSegment is TrackSegmentSurrogate ) {
          // Draw surrogates with a transparent brush.
          graphics.DrawLine( new Pen( Color.FromArgb( 128, trackColor ) ), trackExtent.Left, trackExtent.Bottom, trackExtent.Right, trackExtent.Top );
        } else {
          graphics.DrawLine( new Pen( trackColor ), trackExtent.Left, trackExtent.Bottom, trackExtent.Right, trackExtent.Top );
        }
      }
    }

    /// <summary>
    ///   Draws the background of the control.
    /// </summary>
    /// <param name="graphics">The <see cref="Graphics"/> instance to draw into.</param>
    protected override void DrawBackground(Graphics graphics) {
      //base.DrawBackground( graphics );

      Rectangle trackAreaBounds = GetTrackAreaBounds();

      float origin = trackAreaBounds.Height / 2f + trackAreaBounds.Top + _renderingOffset.Y;

      graphics.DrawLine( Pens.White, trackAreaBounds.Left, origin, trackAreaBounds.Right, origin );

      float step = _renderingScale.Y * 10f;
      Pen pen = new Pen( Color.FromArgb( 40, 40, 40 ) );
      for( float y = step; y < trackAreaBounds.Height / 2f; y += step ) {
        graphics.DrawLine( pen, trackAreaBounds.Left, origin - y, trackAreaBounds.Right, origin - y );  
        graphics.DrawLine( pen, trackAreaBounds.Left, origin + y, trackAreaBounds.Right, origin + y ); 
      }
    }
  }
}