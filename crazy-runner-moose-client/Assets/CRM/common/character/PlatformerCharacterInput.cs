using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class PlatformerCharacterInput {
  
  private PlatformerCharacterInput() {}

  public static void Update(PlatformerCharacterInputState inputState, PlatformerCharacterState characterState) {
    if(inputState.isJumpRequested){
      characterState.jumpRequest = inputState.wasJumpRequested? PlatformerJumpRequest.Continued : PlatformerJumpRequest.Requested;
      inputState.wasJumpRequested = true;
    } else {
      characterState.jumpRequest = PlatformerJumpRequest.None;
      inputState.wasJumpRequested = false;
    }
    characterState.walkRequest = inputState.walkDirection;
  }

}