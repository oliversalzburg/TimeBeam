using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TimeBeam.Helper {
  /// <summary>
  /// Methods to help with the process of selecting items.
  /// </summary>
  internal static class SelectionHelper {
    /// <summary>
    /// Check if a given rectangle would be selected by another rectangle.
    /// By default, checks whether the selection rectangle intersects with the bounding rectangle.
    /// If Ctrl is pressed, the selection rectangle has to contain the complete bounding rectangle.
    /// </summary>
    /// <param name="selectionRectangle">The rectangle that represents the selection.</param>
    /// <param name="boundingRectangle">The rectangle that should be tested whether it is selected.</param>
    /// <param name="modifierKeys">The modifier keys that are currently pressed.</param>
    /// <returns><see langword="true"/> if the rectangle is selected; <see langword="false"/> otherwise.</returns>
    public static bool IsSelected( RectangleF selectionRectangle, RectangleF boundingRectangle, Keys modifierKeys ) {
      if( ( modifierKeys & Keys.Control ) != 0 ) {
        return selectionRectangle.Contains( boundingRectangle );
      } else {
        return selectionRectangle.IntersectsWith( boundingRectangle );
      }
    }
  }
}
