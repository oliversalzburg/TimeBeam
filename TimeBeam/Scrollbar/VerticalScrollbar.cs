using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TimeBeam.Scrollbar {
  /// <summary>
  ///   A scrollbar that is oriented vertically.
  /// </summary>
  public sealed partial class VerticalScrollbar : AbstractScrollbar {
    /// <summary>
    ///   Construct a new vertical scrollbar.
    /// </summary>
    public VerticalScrollbar() : base( ScrollOrientation.VerticalScroll ) {
      InitializeComponent();
    }

    /// <summary>
    ///   Redraws the scrollbar into the backbuffer.
    /// </summary>
    protected override void Redraw() {
      RecalculateThumbBounds();

      // Clear the buffer
      GraphicsContainer.Clear( BackgroundColor );

      // Get the correct thumb center point for the current value.
      float thumbCenter = ValueToPosition( Value );

      // Calculate the position of the thumb
      ThumbBounds.Y = (int)( thumbCenter - ThumbBounds.Height / 2f );

      // Determine the margin that applied in regards to the width.
      int margin = ( Width - ThumbBounds.Width );
      // We now subtract the same margin from the height.
      // TODO: It would be nicer to have a set margin that is subtracted from the control height and included in all calculations.
      Rectangle visibleBounds = ThumbBounds;
      visibleBounds.Y += margin / 2;
      visibleBounds.Height -= margin;

      GraphicsContainer.FillRectangle( ForegroundBrush, visibleBounds );
    }

    /// <summary>
    ///   Calculate the position (of the center of the thumb) for a given value.
    /// </summary>
    /// <param name="value">A given value within the defined bounds.</param>
    /// <returns>The center point of the thumb for the given value.</returns>
    private int ValueToPosition( int value ) {
      int range = ( Max - Min );
      if( 0 == range ) return 0;

      // Start by defining the value on a scale of 0 to 1.
      float relativeValue = (float)( value + Math.Abs( Min ) ) / ( Math.Abs( Max ) + Math.Abs( Min ) );
      // Subtract 0.5 to get the offset for our value from the center of the bar
      float centerOffset = relativeValue - 0.5f;

      // The center of the scrollbar.
      float halfHeight = Height / 2f;
      // Now we find the center of the thumb by adding the offset to the center of height.
      float thumbCenter = halfHeight + ( centerOffset * ( Height - ThumbBounds.Height ) );

      return (int)thumbCenter;
    }

    /// <summary>
    ///   Calculate a value within the defined bounds for a given value on the bar.
    /// </summary>
    /// <param name="position">The position (in pixels) on the bar.</param>
    /// <returns>The value that corresponds to the given position on the bar.</returns>
    private int PositionToValue( int position ) {
      // To calculate a value from a position on the bar, we first need the constrained bar height.
      // This is the height of the bar with the height of the thumb taken into account.
      int constrainedHeight = Height - ThumbBounds.Height;

      // Now calculate the relative position (on a scale from 0 to 1) within the bounds where the thumb is taken into account.
      int halfThumbHeight = ThumbBounds.Height / 2;
      float relativePosition = (float)( position - halfThumbHeight ) / constrainedHeight;

      // Use the relative position to calculate a value...
      int assumedValue = (int)( relativePosition * ( Max - Min ) );
      // ...and limit the value to be within the given bounds.
      int limitedValue = Math.Max( Min, Math.Min( Max, assumedValue ) );

      return limitedValue;
    }

    /// <summary>
    ///   Recalculates the size of the thumb on the bar depending on the extent of the possible values.
    /// </summary>
    private void RecalculateThumbBounds() {
      // The smallest possible size for the thumb should be 10% of the height or MinThumbExtent; whatever is bigger.
      float minHeight = Math.Max( MinThumbExtent, Height * 0.1f );
      float naiveHeight = (float)Height - ( Max - Min );
      ThumbBounds.Height = (int)Math.Max( minHeight, naiveHeight );

      // Add a 10% margin on left and right.
      ThumbBounds.Width = (int)( Width * 0.8f );
      ThumbBounds.X = (int)( Width * 0.1f );
    }

    /// <summary>
    ///   Invoked when the control was initially loaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void VerticalScrollbarLoad( object sender, EventArgs e ) {
      Redraw();
      Refresh();
    }

    /// <summary>
    ///   Invoked when the user moves the mouse over the control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void VerticalScrollbarMouseMove( object sender, MouseEventArgs e ) {
      if( ( e.Button & MouseButtons.Left ) != 0 ) {
        int delta = e.Y - ScrollDeltaOrigin;
        int oldValue = Value;
        Value = PositionToValue( ScrollOrigin + delta );
        InvokeScrollEvent( new ScrollEventArgs( ScrollEventType.ThumbTrack, oldValue, Value, ScrollOrientation.VerticalScroll ) );
        Redraw();
        Refresh();
      }
    }

    /// <summary>
    ///   Invoked when the user presses a mouse button while focus is on the control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void VerticalScrollbarMouseDown( object sender, MouseEventArgs e ) {
      ScrollDeltaOrigin = e.Y;
      ScrollOrigin = ValueToPosition( Value );
    }
  }
}