using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCompServer : MonoBehaviour
{
  private PlatformerCharacterConfig platformerConfig;
  private PlatformerCharacterInputState platformerInputState;
  private PlatformerCharacterState platformerState;
  private MessageHandler networkSend;
  
  void Start() {
    platformerInputState = new PlatformerCharacterInputState();
    platformerInputState.isJumpRequested = false;
    platformerInputState.walkDirection = PlatformerWalkDirection.None;

    platformerState = new PlatformerCharacterState();
  }

  public MessageHandler RegisterNetwork(string guid, MessageHandler send, PlatformerCharacterConfig config, DemoSettingsServer settings){
    var trustSquared = settings.trustDistance * settings.trustDistance;
    var broadcastInputMsg = new PlayerInputMessage();
    broadcastInputMsg.playerGuid = guid;
    this.platformerConfig = config;
    this.networkSend = send;
    StartCoroutine(ServerUpdate(send, settings.tickRate, guid));
    return (opCode, message) => {
      if(opCode == OpCode.PLAYER_INPUT){
        var updatedState = (PlayerInputMessage)message;
        updatedState.state.CopyTo(platformerInputState);
        var actualPosition = transform.position;
        var clientPosition = updatedState.position.toVector();
        var deltaPosition = actualPosition - clientPosition;
        if(deltaPosition.sqrMagnitude < trustSquared){
          transform.position = clientPosition;
        } else {
          transform.position = (deltaPosition.normalized * settings.trustDistance) + actualPosition;
        }
        updatedState.state.CopyTo(broadcastInputMsg.state);
        broadcastInputMsg.position.FromVector(transform.position);
        send(OpCode.PLAYER_INPUT, broadcastInputMsg);
      }
      return 1;
    };
  }

  private IEnumerator<YieldInstruction> ServerUpdate( MessageHandler send, float tickRate, string guid){
    PositionMessage posMsg = new PositionMessage();
    posMsg.playerGuid = guid;
    while(true) {
      yield return new WaitForSeconds(tickRate);
      posMsg.position = transform.position;
      send(OpCode.PLAYER_POSITION, posMsg);
    }
  }


  void Update() {
      if(networkSend != null){
          PlatformerCharacterInput.Update(platformerInputState, platformerState);
          PlatformerCharacter.Update(transform, platformerState, platformerConfig);
      }
  }

}
