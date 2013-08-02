using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TimeBeam.Scrollbar {
  /// <summary>
  ///   A scrollbar that is oriented horizontally.
  /// </summary>
  public sealed partial class HorizontalScrollbar : AbstractScrollbar {
    /// <summary>
    ///   Construct a new horizontal scrollbar.
    /// </summary>
    public HorizontalScrollbar() {
      InitializeComponent();
    }

    /// <summary>
    ///   Redraws the scrollbar into the backbuffer.
    /// </summary>
    protected override void Redraw() {
      // Clear the buffer
      GraphicsContainer.Clear( BackgroundColor );

      // Start by defining the value on a scale of 0 to 1.
      float relativeValue = (float)Value / ( Max - Min );
      // Subtract 0.5 to get the offset for our value from the center of the bar
      float centerOffset = relativeValue - 0.5f;

      // The center of the scrollbar.
      float halfWidth = Width / 2f;
      // Now we find the center of the thumb by adding the offset to the center of width.
      float thumbCenter = halfWidth + ( centerOffset * ( Width - ThumbBounds.Width ) );

      // Calculate the position of the thumb
      ThumbBounds.X = (int)( thumbCenter - ThumbBounds.Width / 2f );

      GraphicsContainer.FillRectangle( ForegroundBrush, ThumbBounds );
    }

    /// <summary>
    /// Calculate a value within the defined bounds for a given value on the bar.
    /// </summary>
    /// <param name="position">The position (in pixels) on the bar.</param>
    /// <returns>The value that corresponds to the given position on the bar.</returns>
    private int PositionToValue( int position ) {
      // To calculate a value from a position on the bar, we first need the constrained bar width.
      // This is the width of the bar with the width of the thumb taken into account.
      int constrainedWidth = Width - ThumbBounds.Width;

      // Now calculate the relative position (on a scale from 0 to 1) within the bounds where the thumb is taken into account.
      int halfThumbWidth = ThumbBounds.Width / 2;
      float relativePosition = (float)( position - halfThumbWidth ) / constrainedWidth;
      
      // Use the relative position to calculate a value...
      int assumedValue = (int)( relativePosition * ( Max - Min ) );
      // ...and limit the value to be within the given bounds.
      int limitedValue = Math.Max( Min, Math.Min( Max, assumedValue ) );
      
      return limitedValue;
    }

    /// <summary>
    /// Recalculates the size of the thumb on the bar depending on the extent of the possible values.
    /// </summary>
    private void RecalculateThumbBounds() {
      // The smallest possible size for the thumb should be 10% of the width
      float minWidth = Width * 0.1f;
      float naiveWidth = (float)Width - ( Max - Min );
      ThumbBounds.Width = (int)Math.Max( minWidth, naiveWidth );
    }

    /// <summary>
    /// Invoked when the control was initially loaded
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HorizontalScrollbar_Load( object sender, System.EventArgs e ) {
      RecalculateThumbBounds();
      Redraw();
      Refresh();
    }

    private void HorizontalScrollbar_MouseMove( object sender, MouseEventArgs e ) {
      if( ( e.Button & MouseButtons.Left ) != 0 ) {
        Value = PositionToValue( e.X );
        RecalculateThumbBounds();
        Redraw();
        Refresh();
      }
    }
  }
}