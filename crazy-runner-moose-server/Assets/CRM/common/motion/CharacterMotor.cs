using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class CharacterMotorConfig {
  public float offset = 0.5f;
  public float gizmoDistance = 1.0f;
  public LayerMask layer;
  public int sideCheckCount = 2;
}

[Serializable] public class CharacterMotorState {
  public Dictionary<MotionDirection, bool> blocked;
  public Dictionary<MotionDirection, bool> wasBlocked;
  public Vector2 lastMotion;
  public LinkedList<MotionForce> forces;
  public Boolean isEnabled;
  public String id;
  public CharacterMotorState(string id) {
    this.id = id;
    blocked = GetFillBlockState();
    wasBlocked = GetFillBlockState();
    isEnabled = true;
    forces = new LinkedList<MotionForce>();
  }
  private static Dictionary<MotionDirection, bool> GetFillBlockState() {
    var blockState = new Dictionary<MotionDirection, bool>();
    blockState[MotionDirection.Up] = false;
    blockState[MotionDirection.Down] = false;
    blockState[MotionDirection.Left] = false;
    blockState[MotionDirection.Right] = false;
    return blockState;
  }
};

public class CharacterMotor {
  
  private static readonly MotionDirection[] SIDES = { MotionDirection.Up, MotionDirection.Down, MotionDirection.Left, MotionDirection.Right };
  private static readonly Vector3[] RAY_BUFFER = new Vector3[2];
  private static readonly RaycastHit2D[] HIT_BUFFER = new RaycastHit2D[100];

  private CharacterMotor() {}

  public static void Update (Transform toMove, CharacterMotorState state, CharacterMotorConfig config) {
    if (!state.isEnabled) return;
    Vector3 dir  = getForcesDirection(state);
    dir = dir * Time.deltaTime;
    state.wasBlocked[MotionDirection.Up] = state.blocked[MotionDirection.Up];
    state.wasBlocked[MotionDirection.Down] = state.blocked[MotionDirection.Down];
    state.wasBlocked[MotionDirection.Left] = state.blocked[MotionDirection.Left];
    state.wasBlocked[MotionDirection.Right] = state.blocked[MotionDirection.Right];
    foreach (MotionDirection side in SIDES) {
      state.blocked[side] = false;
      Vector3 sideDir = MotionUtil.ToVector(side);
      var isMovingInSideXDir = Math.Sign(dir.x) == Math.Sign(sideDir.x);
      var isMovingInSideYDir = Math.Sign(dir.y) == Math.Sign(sideDir.y);
      if(!isMovingInSideXDir && !isMovingInSideYDir) {
        continue;
      }
      for (int i = 0; i < config.sideCheckCount; i++) {
        GetCheckRay(side, config, toMove, RAY_BUFFER, i, dir.magnitude);
        Vector3 direction = (RAY_BUFFER[1] - RAY_BUFFER[0]);
        float distance = GetBlockDistance(RAY_BUFFER[0], direction, config.layer);
        if (distance < float.MaxValue) {
          float canMove = Mathf.Max(0, distance - .01f);
          state.blocked[side] = true;
          dir.x = isMovingInSideXDir ? Mathf.Sign(dir.x) * Mathf.Min(canMove, Mathf.Abs(dir.x)) : dir.x;
          dir.y = isMovingInSideYDir ? Mathf.Sign(dir.y) * Mathf.Min(canMove, Mathf.Abs(dir.y)) : dir.y;
          break;
        }
      }
    }
    state.lastMotion = dir;
    toMove.position = toMove.position + dir;
  }
  
  public static void OnDrawGizmos(CharacterMotorConfig config, Transform toMove) {
    Gizmos.color = Color.green;
    foreach (MotionDirection side in SIDES) {
      for (int i = 0; i < config.sideCheckCount; i++) {
        GetCheckRay(side, config, toMove, RAY_BUFFER, i, config.gizmoDistance);
        Gizmos.DrawLine(RAY_BUFFER[0], RAY_BUFFER[1]);
      }
    }
  }

  private static Vector2 getForcesDirection(CharacterMotorState state) {
    var node = state.forces.First;
    var direction = Vector2.zero;
    while(node != null) {
      direction = direction + (MotionUtil.ToVector(node.Value.direction) * node.Value.factor);
      node = node.Next;
    }
    return direction;
  }

  private static void GetCheckRay(MotionDirection side, CharacterMotorConfig config, Transform toMove, Vector3[] output, float index, float distance) {
    Vector3 sideDir = MotionUtil.ToVector(side);
    Vector3 origin = toMove.position;
    Vector3 direction = Vector3.zero;
    GetCheckValue(ref origin, ref direction, sideDir.x, config, index, Vector3.right, Vector3.up, distance);
    GetCheckValue(ref origin, ref direction, sideDir.y, config, index, Vector3.up, Vector3.right, distance);
    output[0] = origin;
    output[1] = origin + direction;
  }

  private static void GetCheckValue(ref Vector3 origin, ref Vector3 direction, float sideVal, CharacterMotorConfig config, float index, Vector3 dir, Vector3 opDir, float distance) {
    if (Mathf.Abs(sideVal) > .001f){
      origin += dir * (config.offset * Mathf.Sign(sideVal));
      direction = dir * (distance * Mathf.Sign(sideVal));
      float increment = (config.offset*2) / (config.sideCheckCount-1);
      origin += opDir*(increment*index - config.offset);
    }
  }

  private static float GetBlockDistance(Vector3 origin, Vector3 direction, LayerMask layer) {
    float distance = float.MaxValue;
    Debug.DrawRay(origin, direction, Color.red);
    int hitCount = Physics2D.RaycastNonAlloc(origin, direction, HIT_BUFFER, direction.magnitude, layer);
    for (int i = 0; i < hitCount; i++) {
      if (HIT_BUFFER[i].distance < distance) {
        distance = HIT_BUFFER[i].distance;
      }
    }
    return distance;
  }

}