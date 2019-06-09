using System;

[Serializable] public class MotionForce {
  public MotionDirection direction = MotionDirection.None;
  public float factor = 0.0f;
  public string identifier;

  public MotionForce(string identifier) {
    this.identifier = identifier;
  }
};