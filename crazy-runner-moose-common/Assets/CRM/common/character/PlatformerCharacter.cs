using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformerWalkDirection { None, Left, Right }

[Serializable] public class PlatformerCharacterConfig {
  public bool canWallGrab = true;
  public float walkSpeed = 5.0f;
  public int jumpCount = 2;
  public CurvedMotionForceConfig jumpConfig;
  public CharacterMotorConfig motorConfig;
}

public enum PlatformerJumpRequest { None, Requested, Continued}

public class PlatformerCharacterState {
    public int jumpsRemaining;
    public bool hadWallGrab;
    public bool hasWallGrab;
    public GravityForceState gravityState;
    public CurvedMotionForceState jumpState;
    public CharacterMotorState motorState;
    public PlatformerWalkDirection walkRequest = PlatformerWalkDirection.None;
    public PlatformerJumpRequest jumpRequest = PlatformerJumpRequest.None;
    public MotionForce jumpForce = new MotionForce("platformer-jump");
    public MotionForce walkForce = new MotionForce("platformer-walk");

    public PlatformerCharacterState() {
      this.motorState = new CharacterMotorState("platformer-character");
      this.gravityState = new GravityForceState(motorState);
      this.jumpState = new CurvedMotionForceState(jumpForce);
      jumpForce.direction = MotionDirection.Up;
      motorState.forces.AddFirst(jumpForce);
      motorState.forces.AddFirst(walkForce);
      motorState.forces.AddFirst(gravityState.force);
    }
};

public class PlatformerCharacter {
  private static readonly MotionDirection[] WALLGRAB_SIDES = {MotionDirection.Left, MotionDirection.Right};
  private static readonly MotionDirection[] JUMP_RESET_SIDES = {MotionDirection.Down};
  
  private static readonly Dictionary<PlatformerWalkDirection, MotionDirection> WALK_MAP = new Dictionary<PlatformerWalkDirection, MotionDirection>() {
    {PlatformerWalkDirection.Left, MotionDirection.Left},
    {PlatformerWalkDirection.Right, MotionDirection.Right},
    {PlatformerWalkDirection.None, MotionDirection.None}
  };
  private PlatformerCharacter() {}

  public static void OnGUI(PlatformerCharacterState state){
  if (Application.isEditor) {
    var style = GUI.skin.label;
    style.fontSize =20;
    GUI.Label(new Rect(10, 10, 400, 100), "WALK: " + state.walkRequest, style);
    GUI.Label(new Rect(10, 35, 400, 100), "JUMP STATE: " + state.jumpRequest, style);
    GUI.Label(new Rect(10, 60, 400, 100), "JUMPS LEFT: " + state.jumpsRemaining, style);
    GUI.Label(new Rect(10, 85, 400, 100), "GRAVITY: " + state.gravityState.curvedForceState.currentState, style);
  }
}

  public static void Update (Transform transform, PlatformerCharacterState state, PlatformerCharacterConfig config) {
    state.hasWallGrab = HasWallGrab(state, config);
    if(HasJumpReset(state, config)){
      state.jumpsRemaining = config.jumpCount;
    }
    if(state.jumpRequest == PlatformerJumpRequest.Requested &&state.jumpsRemaining > 0) {
      state.jumpsRemaining = state.jumpsRemaining - 1;
      state.jumpState.requestedState = MotionPlaybackState.Playing;
    } else if(state.jumpRequest == PlatformerJumpRequest.None && state.jumpState.currentState == MotionPlaybackState.Playing) {
      state.jumpState.requestedState = MotionPlaybackState.Stopped;
    }
    state.walkForce.direction = WALK_MAP[state.walkRequest];
    state.walkForce.factor = config.walkSpeed;
    CurvedMotionForce.Update(config.jumpConfig, state.jumpState);
    GravityMotionComp.Update(state.gravityState);
    if(state.jumpState.currentState == MotionPlaybackState.Playing || state.hasWallGrab){
      state.gravityState.curvedForceState.delta = 0;
      state.gravityState.force.factor = 0;
      state.gravityState.curvedForceState.currentState = MotionPlaybackState.Stopped;
      state.gravityState.curvedForceState.requestedState = MotionPlaybackState.Playing;
    }
    state.hadWallGrab = state.hasWallGrab;
    CharacterMotor.Update(transform, state.motorState, config.motorConfig);
  }

  private static bool HasJumpReset(PlatformerCharacterState state, PlatformerCharacterConfig config) {
    var isJumpGrab = state.jumpRequest == PlatformerJumpRequest.Requested && state.hasWallGrab;
    var hasWallGrabChange = state.hasWallGrab && !state.hadWallGrab;
    var hasFloorChange = !state.motorState.wasBlocked[MotionDirection.Down] && state.motorState.blocked[MotionDirection.Down];
    return hasWallGrabChange || hasFloorChange || isJumpGrab;
  }

  private static bool HasWallGrab(PlatformerCharacterState state, PlatformerCharacterConfig config) {
    var hasWallGrab = false;
    if(config.canWallGrab) {
      foreach(var side in WALLGRAB_SIDES){
        hasWallGrab = state.motorState.blocked[side];
        if(hasWallGrab) {
          break;
        }
      }
    }
    return hasWallGrab;
  }
  
  public static void OnDrawGizmos(PlatformerCharacterConfig config, Transform transform) {
        CharacterMotor.OnDrawGizmos(config.motorConfig, transform);
    }

}