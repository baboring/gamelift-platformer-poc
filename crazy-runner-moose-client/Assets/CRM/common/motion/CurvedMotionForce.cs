using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MotionPlaybackState {Stopped, Playing, Complete}

public class CurvedMotionForceState {
  public float delta;
  public MotionPlaybackState currentState = MotionPlaybackState.Stopped;
  public MotionPlaybackState requestedState = MotionPlaybackState.Stopped;
  public readonly MotionForce force;
  public CurvedMotionForceState(MotionForce force){
    this.force = force;
  }
}

[Serializable]
public class CurvedMotionForceConfig{
    public float factor = 1.0f;
    public float duration = 1.0f;
    public AnimationCurve curve;
    public bool isStopedOnComplete;
}

public class CurvedMotionForce
{
  public static void Update(CurvedMotionForceConfig config, CurvedMotionForceState state) {
    if(state.currentState != state.requestedState) {
      if(state.requestedState == MotionPlaybackState.Playing) {
        state.delta = 0;
      }
      else if(state.requestedState == MotionPlaybackState.Complete) {
        state.delta = config.duration;
        if(config.isStopedOnComplete){
          state.requestedState = MotionPlaybackState.Stopped;
        }
      }
      if(state.requestedState == MotionPlaybackState.Stopped) {
        state.force.factor = 0;
        state.delta = 0;
      }
      state.currentState = state.requestedState;
    }
    if(state.currentState == MotionPlaybackState.Playing){
      state.delta = Math.Min(state.delta + Time.deltaTime, config.duration);
      var percent = state.delta / config.duration;
      state.force.factor = config.curve.Evaluate(percent) * config.factor;
      if(state.delta >= config.duration){
        state.requestedState = MotionPlaybackState.Complete;
      }
    }
  }

}
