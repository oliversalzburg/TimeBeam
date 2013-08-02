using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeBeam.Helper {
  /// <summary>
  /// Extension methods
  /// </summary>
  internal static class Extensions {
    /// <summary>
    /// Clamp a value to a certain range.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="val">The value to be clamped.</param>
    /// <param name="min">The lower bound.</param>
    /// <param name="max">The upper bound</param>
    /// <returns></returns>
    public static T Clamp<T>( this T val, T min, T max ) where T : IComparable<T> {
      if( val.CompareTo( min ) < 0 ) return min;
      else if( val.CompareTo( max ) > 0 ) return max;
      else return val;
    }
  }
}
