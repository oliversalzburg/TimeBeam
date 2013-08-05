using System;
using System.Drawing;

namespace TimeBeam.Helper {
  /// <summary>
  ///   Contains helper methods to manipulate or generate rectangles.
  /// </summary>
  internal static class RectangleHelper {
    /// <summary>
    ///   The edges of a rectangle.
    /// </summary>
    [Flags]
    internal enum Edge {
      /// <summary>
      ///   No edge
      /// </summary>
      None = 0,

      /// <summary>
      ///   The top edge.
      /// </summary>
      Top = 1,

      /// <summary>
      ///   The right edge.
      /// </summary>
      Right = 2,

      /// <summary>
      ///   The bottom edge.
      /// </summary>
      Bottom = 4,

      /// <summary>
      ///   The left edge.
      /// </summary>
      Left = 8
    }

    /// <summary>
    ///   Which edges to perform tests on.
    /// </summary>
    [Flags]
    public enum EdgeTest {
      /// <summary>
      ///   Vertical edges.
      /// </summary>
      Vertical,
      //Horizontal edges.
      Horizontal
    }

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

    /// <summary>
    ///   Determines whether a given point is inside and close to the edge of a given rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle within which the point should be located.</param>
    /// <param name="test">The point to test for.</param>
    /// <param name="tolerance">
    ///   The tolerance. This is subtracted from the bounds of the rectangle to create a border within which the
    ///   <paramref
    ///     name="test" />
    ///   point must be located.
    /// </param>
    /// <param name="toTest">The edges that should be tested.</param>
    /// <returns>A combination of Edge flags depending on which edges the point is close to.</returns>
    internal static Edge IsPointOnEdge( RectangleF rectangle, PointF test, float tolerance, EdgeTest toTest ) {
      // If the point is not within the rectangle, then there is no need for further tests.
      if( !rectangle.Contains( test ) ) {
        return Edge.None;
      }

      Edge result = Edge.None;
      // Test vertical edges.
      if( ( toTest & EdgeTest.Vertical ) != 0 ) {
        if( test.Y >= rectangle.Y && test.Y <= rectangle.Y + tolerance ) {
          result |= Edge.Top;
        }
        if( test.Y <= rectangle.Y + rectangle.Height && test.Y >= rectangle.Y + rectangle.Height - tolerance ) {
          result |= Edge.Bottom;
        }
      }
      // Test horizontal edges.
      if( ( toTest & EdgeTest.Horizontal ) != 0 ) {
        if( test.X <= rectangle.X + rectangle.Width && test.X >= rectangle.X + rectangle.Width - tolerance ) {
          result |= Edge.Right;
        }
        if( test.X >= rectangle.X && test.X <= rectangle.X + tolerance ) {
          result |= Edge.Left;
        }
      }

      return result;
    }
  }
}