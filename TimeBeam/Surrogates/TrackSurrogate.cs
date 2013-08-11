namespace TimeBeam.Surrogates {
  /// <summary>
  ///   A substitute for another track on the timeline.
  /// </summary>
  internal class TrackSurrogate : ITimelineTrack {
    /// <summary>
    ///   The object this surrogate is a substitute for.
    /// </summary>
    public ITimelineTrack SubstituteFor { get; set; }

    /// <summary>
    ///   The beginning of the item.
    /// </summary>
    public float Start { get; set; }

    /// <summary>
    ///   The end of the item.
    /// </summary>
    public float End { get; set; }

    /// <summary>
    ///   The name of the item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///   Construct a new surrogate for another <see cref="ITimelineTrack" />.
    /// </summary>
    /// <param name="substituteFor">The ITimelineTrack we're substituting for.</param>
    public TrackSurrogate( ITimelineTrack substituteFor ) {
      SubstituteFor = substituteFor;
      Start = substituteFor.Start;
      End = substituteFor.End;
      Name = substituteFor.Name;
    }

    /// <summary>
    ///   Copies all properties of the surrogate to another <see cref="ITimelineTrack" />.
    /// </summary>
    /// <param name="target">The target timeline track to copy the properties to.</param>
    public void CopyTo( ITimelineTrack target ) {
      target.Start = Start;
      target.End = End;
    }

    /// <summary>
    ///   Invoked when the user selects this item.
    /// </summary>
    public void Selected() {
      // Nothing to do.
    }

    public void Deselected() {
      // Nothing to do.
    }
  }
}