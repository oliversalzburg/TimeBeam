namespace TimeBeam.Surrogates {
  /// <summary>
  ///   A substitute for another track segment on the timeline.
  /// </summary>
  internal class TrackSegmentSurrogate : ITrackSegment {
    /// <summary>
    ///   The object this surrogate is a substitute for.
    /// </summary>
    public ITrackSegment SubstituteFor { get; set; }

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
    ///   Construct a new surrogate for another <see cref="ITrackSegment" />.
    /// </summary>
    /// <param name="substituteFor">The ITrackSegment we're substituting for.</param>
    public TrackSegmentSurrogate( ITrackSegment substituteFor ) {
      SubstituteFor = substituteFor;
      Start = substituteFor.Start;
      End = substituteFor.End;
      Name = substituteFor.Name;
    }

    /// <summary>
    ///   Copies all properties of the surrogate to another <see cref="ITrackSegment" />.
    /// </summary>
    /// <param name="target">The target track segment to copy the properties to.</param>
    public void CopyTo( ITrackSegment target ) {
      target.Start = Start;
      target.End = End;
    }
  }
}