using System.Collections.Generic;
using UnityEngine;

internal class MotionUtil{

  private static readonly Dictionary<MotionDirection, Vector2> DIRECTION_MAP = new Dictionary<MotionDirection, Vector2>(new MotionComparator()) {
    {MotionDirection.None, Vector2.zero},
    {MotionDirection.Down, Vector2.down},
    {MotionDirection.Up, Vector2.up},
    {MotionDirection.Left, Vector2.left},
    {MotionDirection.Right, Vector2.right},
  };

  public static Vector2 ToVector(MotionDirection motionDirection) {
    Vector2 direction;
    if (!DIRECTION_MAP.TryGetValue(motionDirection, out direction)) {
      Debug.LogError("huh... couldnt find direction for " + motionDirection);
    }
    return direction;
  }
}

internal class MotionComparator : IEqualityComparer<MotionDirection> {
  public bool Equals(MotionDirection x, MotionDirection y) {
    return (int)x == (int)y;
  }

  public int GetHashCode(MotionDirection obj) {
    return (int) obj;
  }
}