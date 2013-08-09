namespace TimeBeam.Timing {
  /// <summary>
  ///   Describes an object that can be used as a time source for the timeline.
  /// </summary>
  public interface IClock {
    /// <summary>
    ///   Is this clock currently running?
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    ///   What is the value of the clock?
    ///   This value would usually indicate milliseconds.
    /// </summary>
    double Value { get; set;  }
  }
}