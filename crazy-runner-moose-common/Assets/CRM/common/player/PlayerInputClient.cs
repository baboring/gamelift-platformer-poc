using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerInputDirection {
  public KeyCode key;
  public PlatformerWalkDirection direction;
}
[Serializable]
public class PlayerInputConfig {
  public PlayerInputDirection[] directions;
  public KeyCode jumpCode;
}

public class PlayerInputState {
  public readonly LinkedList<PlatformerWalkDirection> directions = new LinkedList<PlatformerWalkDirection>();
  public PlatformerCharacterInputState platformerState;
  public PlatformerCharacterInputState oldState;

    public PlayerInputState(PlatformerCharacterInputState platformerState) {
      this.oldState = new PlatformerCharacterInputState();
      this.platformerState = platformerState;
      this.platformerState.CopyTo(oldState);
    }
}

public class PlayerInputClient {
  
  private PlayerInputClient() {}

  public static void Update(PlayerInputConfig playerInputConfig, PlayerInputState playerInputState) {
    var wasJumpRequested = playerInputState.platformerState.isJumpRequested;
    playerInputState.platformerState.isJumpRequested = Input.GetKey(playerInputConfig.jumpCode);
    foreach(var pair in playerInputConfig.directions) {
      if(Input.GetKeyDown(pair.key)) {
        playerInputState.directions.AddFirst(pair.direction);
      }
      if(Input.GetKeyUp(pair.key)) {
        playerInputState.directions.Remove(pair.direction);
      }
    }
    var head = playerInputState.directions.First;
    var oldDirection = playerInputState.platformerState.walkDirection;
    playerInputState.platformerState.walkDirection = head == null ? PlatformerWalkDirection.None : head.Value;
  }

}