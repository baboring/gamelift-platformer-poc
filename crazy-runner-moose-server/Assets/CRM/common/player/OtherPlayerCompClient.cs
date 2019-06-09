using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerCompClient : MonoBehaviour
{
    private PlatformerCharacterConfig platformerConfig;
    private PlatformerCharacterInputState platformerInputState;
    private PlatformerCharacterState platformerState;
    
    void Start() {
        platformerInputState = new PlatformerCharacterInputState();
        platformerInputState.isJumpRequested = false;
        platformerInputState.walkDirection = PlatformerWalkDirection.None;

        platformerState = new PlatformerCharacterState();
    }

    public MessageHandler RegisterNetwork(PlatformerCharacterConfig config){
        this.platformerConfig = config;
        return (opCode, message) => {
            if (opCode == OpCode.PLAYER_POSITION){
                var positionMessage = (PositionMessage)message;
                transform.position = positionMessage.position;
            } else if (opCode == OpCode.PLAYER_INPUT){
                var inputMessage = (PlayerInputMessage)message;
                if (platformerInputState != null) {
                    inputMessage.state.CopyTo(platformerInputState);
                }
                transform.position = inputMessage.position.toVector();
            }
            return 1;
        };
    }

    void Update() {
        if(this.platformerConfig != null){
            PlatformerCharacterInput.Update(platformerInputState, platformerState);
            PlatformerCharacter.Update(transform, platformerState, platformerConfig);
        }
    }

}
