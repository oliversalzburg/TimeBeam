using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TimeBeam.Helper {
  internal static class BoundsHelper {
    /// <summary>
    ///   Calculate the bounding rectangle for a track in screen-space.
    /// </summary>
    /// <param name="track">
    ///   The track for which to calculate the bounding rectangle.
    /// </param>
    /// <param name="timeline">The timeline the track lives on.</param>
    /// <returns>The bounding rectangle for the given track.</returns>
    internal static RectangleF GetTrackExtents( ITimelineTrack track, Timeline timeline ) {
      int trackIndex = timeline.TrackIndexForTrack( track );
      return RectangleToTrackExtents( new RectangleF( track.Start, 0, track.End - track.Start, 0 ), timeline, trackIndex );
    }

    /// <summary>
    ///   Calculate the bounding rectangle in screen-space that would hold a track of the same extents as the given rectangle.
    /// </summary>
    /// <param name="rect">A rectangle which left and right edge represent the start and end of a track item. The top and bottom edge are ignored.</param>
    /// <param name="timeline">The timeline the assumed track would live on. Used to determine the top and bottom edge of the bounding rectangle.</param>
    /// <param name="assumedTrackIndex">The assumed index of the track. Used to determine the top edge of the bounding rectangle.</param>
    /// <returns>A bounding rectangle that would hold a track of the same extents as the given rectangle.</returns>
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

    /// <summary>
    ///   Checks if a given rectangle intersects with any of the target rectangles.
    /// </summary>
    /// <param name="test">The rectangle that should be tested against the other rectangles.</param>
    /// <param name="subjects">The "other" rectangles.</param>
    /// <returns>
    ///   <see langword="true" /> if there is an intersection; <see langword="false" /> otherwise
    /// </returns>
    internal static bool IntersectsAny( RectangleF test, IEnumerable<RectangleF> subjects ) {
      return subjects.Any( subject => subject.IntersectsWith( test ) );
    }
  }
}