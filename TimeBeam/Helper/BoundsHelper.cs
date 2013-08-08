using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TimeBeam.Helper {
  internal static class BoundsHelper {
    /// <summary>
    ///   Calculate the bounding rectangle for a track in global space.
    /// </summary>
    /// <param name="track">The track for which to calculate the bounding rectangle.</param>
    /// <returns>The bounding rectangle for the given track.</returns>
    internal static RectangleF GetTrackExtents( ITimelineTrack track, Timeline timeline, int trackIndex ) {
      return RectangleToTrackExtents( new RectangleF( track.Start, 0, track.End - track.Start, 0 ), timeline, trackIndex );
    }

    internal static RectangleF RectangleToTrackExtents( RectangleF rect, Timeline timeline, int assumedTrackIndex ) {
      Rectangle trackAreaBounds = timeline.GetTrackAreaBounds();

      int actualRowHeight = (int)( ( timeline.TrackHeight + timeline.TrackSpacing ) * timeline.RenderingScale.Y );
      // Calculate the Y offset for the track.
      int trackOffsetY = (int)( trackAreaBounds.Y + ( actualRowHeight * assumedTrackIndex ) + timeline.RenderingOffset.Y );

      // Calculate the X offset for track.
      int trackOffsetX = (int)( trackAreaBounds.X + ( rect.X * timeline.RenderingScale.X ) + timeline.RenderingOffset.X );

      // The extent of the track, including the border
      RectangleF trackExtent = new RectangleF( trackOffsetX, trackOffsetY, rect.Width * timeline.RenderingScale.X, timeline.TrackHeight * timeline.RenderingScale.Y );
      return trackExtent;
    }
  }
}
