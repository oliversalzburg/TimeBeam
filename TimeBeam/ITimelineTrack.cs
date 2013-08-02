using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeBeam {
  /// <summary>
  /// Describes an item that can be placed on a track in the timeline.
  /// </summary>
  public interface ITimelineTrack {
    /// <summary>
    /// The beginning of the item.
    /// </summary>
    float Start { get; set; }
    /// <summary>
    /// The end of the item.
    /// </summary>
    float End { get; set; }

    /// <summary>
    /// Invoked when the user selects this item.
    /// </summary>
    void Selected();
  }
}
