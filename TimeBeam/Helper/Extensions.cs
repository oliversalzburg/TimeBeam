using System;
using System.Drawing;

namespace TimeBeam.Helper {
  /// <summary>
  ///   Extension methods
  /// </summary>
  internal static class Extensions {
    /// <summary>
    ///   Clamp a value to a certain range.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="val">The value to be clamped.</param>
    /// <param name="min">The lower bound.</param>
    /// <param name="max">The upper bound</param>
    /// <returns></returns>
    public static T Clamp<T>( this T val, T min, T max ) where T : IComparable<T> {
      if( val.CompareTo( min ) < 0 ) {
        return min;
      } else if( val.CompareTo( max ) > 0 ) {
        return max;
      } else {
        return val;
      }
    }

    /// <summary>
    ///   Convert <see cref="Rectangle" /> to <see cref="RectangleF" />.
    /// </summary>
    /// <param name="input">The rectangle to convert.</param>
    /// <returns>A RectangleF with the values of the input rectangle.</returns>
    public static RectangleF ToRectangleF( this Rectangle input ) {
      return new RectangleF( input.X, input.Y, input.Width, input.Height );
    }

    /// <summary>
    ///   Convert <see cref="RectangleF" /> to <see cref="Rectangle" />.
    /// </summary>
    /// <param name="input">The rectangle to convert.</param>
    /// <returns>A Rectangle with the values of the input rectangle.</returns>
    public static Rectangle ToRectangle( this RectangleF input ) {
      return new Rectangle( (int)input.X, (int)input.Y, (int)input.Width, (int)input.Height );
    }
  }
}