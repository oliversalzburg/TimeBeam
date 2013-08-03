using System.Drawing;

namespace TimeBeam.Helper {
  /// <summary>
  ///   Contains helper methods to manipulate or generate rectangles.
  /// </summary>
  internal static class RectangleHelper {
    /// <summary>
    ///   Create a rectangle from two points. The smaller coordinates will define the upper left corner, the larger coordinates will define the lower right corner.
    /// </summary>
    /// <param name="start">The first point to use.</param>
    /// <param name="end">The second point to use.</param>
    /// <returns>A valid rectangle.</returns>
    internal static RectangleF Normalize( PointF start, PointF end ) {
      RectangleF result = new RectangleF();
      if( end.X < start.X ) {
        result.X = end.X;
        result.Width = start.X - result.X;
      } else {
        result.X = start.X;
        result.Width = end.X - result.X;
      }
      if( end.Y < start.Y ) {
        result.Y = end.Y;
        result.Height = start.Y - result.Y;
      } else {
        result.Y = start.Y;
        result.Height = end.Y - result.Y;
      }
      return result;
    }

    /// <summary>
    ///   Create a rectangle from two points. The smaller coordinates will define the upper left corner, the larger coordinates will define the lower right corner.
    /// </summary>
    /// <param name="start">The first point to use.</param>
    /// <param name="end">The second point to use.</param>
    /// <returns>A valid rectangle.</returns>
    internal static Rectangle Normalize( Point start, Point end ) {
      Rectangle result = new Rectangle();
      if( end.X < start.X ) {
        result.X = end.X;
        result.Width = start.X - result.X;
      } else {
        result.X = start.X;
        result.Width = end.X - result.X;
      }
      if( end.Y < start.Y ) {
        result.Y = end.Y;
        result.Height = start.Y - result.Y;
      } else {
        result.Y = start.Y;
        result.Height = end.Y - result.Y;
      }
      return result;
    }
  }
}