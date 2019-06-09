using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoClient {
  
  private NetworkMessageStream networkStream;

  public void StartNetworked(int port, string address, string playerSessionId, MonoBehaviour parent, DemoSettingsClient settings){
    var spawnPoint = DemoSettings.GetSpawnPoint();
    var playerConfig = DemoSettings.GetPlatformCharacterConfig();
    var recieveByGuid = new Dictionary<string, MessageHandler>();
    networkStream = NetworkApi.OpenOutbound(port, address, parent);
    networkStream.Recieve.Add((opCode, message) => {
        if(opCode == OpCode.PLAYER_ACCEPTED) {
          MessageHandler recieve = null;
          var acceptMsg = (PlayerAcceptMessage)message;
          var isSelf = acceptMsg.playerSessionId.Equals(playerSessionId);
          Debug.Log("client (" + playerSessionId + ")" + " accepts (" + acceptMsg.playerGuid + ") as self? " + isSelf);
          if(acceptMsg.playerSessionId.Equals(playerSessionId)) {
            var spawned = GameObject.Instantiate(settings.playerPrefab, spawnPoint, Quaternion.identity);
            var playerComp = spawned.GetComponent<PlayerCompClient>();
            recieve = playerComp.RegisterNetwork(networkStream.Send, playerConfig, acceptMsg.PlayerGuid, acceptMsg.trustDistance);  
          } else {
            var spawned = GameObject.Instantiate(settings.otherPlayerPrefab, spawnPoint, Quaternion.identity);
            var playerComp = spawned.GetComponent<OtherPlayerCompClient>();
            recieve = playerComp.RegisterNetwork(playerConfig, settings.interpolation);
          }
          recieveByGuid[acceptMsg.PlayerGuid] = recieve;
        } else {
          MessageHandler reciever = null;
          var found = recieveByGuid.TryGetValue(PlayerGuidUtil.Get(message), out reciever);
          if(found){
            reciever?.Invoke(opCode, message);
          }
        }
        return 1;
    });
    var joinMessage = new PlayerJoinMessage();
    joinMessage.playerSessionId = playerSessionId;
    networkStream.Send(OpCode.PLAYER_JOIN, joinMessage);
  }

  public void EndNetworked(){
    networkStream?.Recieve.GetCancellation().Cancel();
  }
}
