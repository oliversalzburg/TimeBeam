using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TimeBeam.Helper {
  /// <summary>
  ///   Contains methods that help in calculation regarding the bounding area of
  ///   elements.
  /// </summary>
  internal static class BoundsHelper {
    /// <summary>
    ///   Calculate the bounding rectangle for a track segment in screen-space.
    /// </summary>
    /// <param name="trackSegment">
    ///   The track segment for which to calculate the bounding rectangle.
    /// </param>
    /// <param name="timeline">The timeline the track segment lives on.</param>
    /// <returns>The bounding rectangle for the given track segment.</returns>
    internal static RectangleF GetTrackExtents( ITrackSegment trackSegment, TimelineBase timeline ) {
      int trackIndex = timeline.TrackIndexForTrack( trackSegment );
      return RectangleToTrackExtents( new RectangleF( trackSegment.Start, 0, trackSegment.End - trackSegment.Start, 0 ), timeline, trackIndex );
    }

    /// <summary>
    ///   Calculate the bounding rectangle in screen-space that would hold a track segment of the same extents as the given rectangle.
    /// </summary>
    /// <param name="rect">A rectangle which left and right edge represent the start and end of a track segment. The top and bottom edge are ignored.</param>
    /// <param name="timeline">The timeline the assumed track segment would live on. Used to determine the top and bottom edge of the bounding rectangle.</param>
    /// <param name="assumedTrackIndex">The assumed index of the track segment. Used to determine the top edge of the bounding rectangle.</param>
    /// <returns>A bounding rectangle that would hold a track segment of the same extents as the given rectangle.</returns>
    internal static RectangleF RectangleToTrackExtents( RectangleF rect, TimelineBase timeline, int assumedTrackIndex ) {
      Rectangle trackAreaBounds = timeline.GetTrackAreaBounds();

      int actualRowHeight = (int)( ( timeline.TrackHeight ) * timeline.RenderingScale.Y + timeline.TrackSpacing );
      // Calculate the Y offset for the track segment.
      int trackOffsetY = (int)( trackAreaBounds.Y + ( actualRowHeight * assumedTrackIndex ) + timeline.RenderingOffset.Y );

      // Calculate the X offset for track segment.
      int trackOffsetX = (int)( trackAreaBounds.X + ( rect.X * timeline.RenderingScale.X ) + timeline.RenderingOffset.X );

      // The extent of the track segment, including the border.
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