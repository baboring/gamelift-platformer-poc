using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCompClient : MonoBehaviour
{
    [SerializeField] public PlayerInputConfig inputConfig;
    
    private PlatformerCharacterConfig platformerConfig;
    private PlayerInputState playerInputState;
    private float trustDistance;
    private PlatformerCharacterInputState platformerInputState;
    private PlatformerCharacterState platformerState;
    private MessageHandler networkSend;
    
    private readonly PlayerInputMessage inputMessage = new PlayerInputMessage();
    
    void Start() {
        platformerState = new PlatformerCharacterState();        
    }

    public MessageHandler RegisterNetwork(MessageHandler send, PlatformerCharacterConfig config, string guid, float trustDistance){
        this.trustDistance = trustDistance;
        platformerInputState = new PlatformerCharacterInputState();
        platformerInputState.isJumpRequested = false;
        platformerInputState.walkDirection = PlatformerWalkDirection.None;
        playerInputState = new PlayerInputState(platformerInputState);
        inputMessage.playerGuid = guid;
        inputMessage.state = playerInputState.platformerState;
        this.platformerConfig = config;
        this.networkSend = send;
        return (opCode, message) => {
            if(opCode == OpCode.PLAYER_POSITION){
                UpdateForServerPosition(((PositionMessage)message).position);
            } else if(opCode == OpCode.PLAYER_INPUT) {
                UpdateForServerPosition(((PlayerInputMessage)message).position.toVector());
            }
            return 1;
        };
    }

    private void UpdateForServerPosition(Vector3 serverPosition){
        var clientPosition = transform.position;
        var delta = serverPosition - clientPosition;
        if(delta.magnitude > trustDistance){
            transform.position = clientPosition + (delta.normalized * trustDistance);
        }
    }


    void Update() {
        if(networkSend != null){
            PlayerInputClient.Update(inputConfig, playerInputState);
            PlatformerCharacterInput.Update(platformerInputState, platformerState);
            PlatformerCharacter.Update(transform, platformerState, platformerConfig);
            if(!playerInputState.platformerState.Equals(playerInputState.oldState)){
                inputMessage.position.FromVector(transform.position);
                networkSend(OpCode.PLAYER_INPUT, inputMessage);
                playerInputState.platformerState.CopyTo(playerInputState.oldState);
            }
        }
    }

    void OnDrawGizmos() {
        if(platformerConfig != null){
            PlatformerCharacter.OnDrawGizmos(platformerConfig, transform);
        }
    }

    public void OnGUI() {
        PlatformerCharacter.OnGUI(platformerState);
    }
}
