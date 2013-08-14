using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeBeam.Surrogates;

namespace TimeBeam.Helper {
  /// <summary>
  /// Helper methods to deal with surrogates.
  /// </summary>
  internal static class SurrogateHelper {
    /// <summary>
    /// Get a list of <see cref="ITrackSegment"/> surrogates for a series of <see cref="ITrackSegment"/> instance.
    /// </summary>
    /// <param name="tracks">The tracks for which to generate surrogates.</param>
    /// <returns>A list of surrogates for the given input series.</returns>
    public static List<ITrackSegment> GetSurrogates( IEnumerable<ITrackSegment> tracks ) {
      return new List<ITrackSegment>( tracks.Select( track => new TrackSegmentSurrogate( track ) ).ToList() );
    }
  }
}
