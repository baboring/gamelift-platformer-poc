using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerCompClient : MonoBehaviour
{
    Vector3 invalid = new Vector3(float.MinValue+1, float.MinValue+1, float.MinValue+1);
    private float interpolation;
    private PlatformerCharacterConfig platformerConfig;
    private PlatformerCharacterInputState platformerInputState;
    private PlatformerCharacterState platformerState;
    private Vector3 serverPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);

    void Start() {
        platformerInputState = new PlatformerCharacterInputState();
        platformerInputState.isJumpRequested = false;
        platformerInputState.walkDirection = PlatformerWalkDirection.None;

        platformerState = new PlatformerCharacterState();
    }

    public MessageHandler RegisterNetwork(PlatformerCharacterConfig config, float interpolation){
        this.interpolation = interpolation;
        this.platformerConfig = config;
        return (opCode, message) => {
            if (opCode == OpCode.PLAYER_POSITION){
                var positionMessage = (PositionMessage)message;
                serverPosition = positionMessage.position;
            } else if (opCode == OpCode.PLAYER_INPUT){
                var inputMessage = (PlayerInputMessage)message;
                if (platformerInputState != null) {
                    inputMessage.state.CopyTo(platformerInputState);
                }
                serverPosition = inputMessage.position.toVector();
            }
            return 1;
        };
    }

    void Update() {
        if(this.platformerConfig != null){
            PlatformerCharacterInput.Update(platformerInputState, platformerState);
            PlatformerCharacter.Update(transform, platformerState, platformerConfig);
            if(serverPosition.x > invalid.x){
                transform.position = Vector3.Lerp(transform.position, serverPosition, Time.deltaTime * interpolation);
            }
        }
    }

}
