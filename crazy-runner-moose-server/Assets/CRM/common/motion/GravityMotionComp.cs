using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityForceState {
  public readonly CurvedMotionForceState curvedForceState;
  public readonly CharacterMotorState motorState;
  public readonly MotionForce force = new MotionForce("gravity");

  public GravityForceState(CharacterMotorState motorState) {
    force.direction = MotionDirection.Down;
    this.curvedForceState = new CurvedMotionForceState(force);
    this.curvedForceState.requestedState = MotionPlaybackState.Playing;
    this.motorState = motorState;
  }
}

public class GravityMotionComp : MonoBehaviour
{
  public CurvedMotionForceConfig config;
  private static GravityMotionComp instance;

  void Awake() {
      instance = this;
  }

  void OnDestroy() {
    if(instance == this){
      instance = null;
    }
  }

  public static void Update(GravityForceState state) {
    var isBlocked = state.motorState.blocked[MotionDirection.Down];
    var wasBlocked = state.motorState.wasBlocked[MotionDirection.Down];
    if(!isBlocked && wasBlocked) {
      state.curvedForceState.delta = 0;
      state.curvedForceState.requestedState = MotionPlaybackState.Playing;
    }
    CurvedMotionForce.Update(instance.config, state.curvedForceState);
  }

}
