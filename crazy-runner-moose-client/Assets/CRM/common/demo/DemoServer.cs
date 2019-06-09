using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ServerRepData {
  public PlayerCompServer comp;
  public MessageHandler recieve;
  public string  guid;
  public string session;
}

public class DemoServer {
  
  private NetworkMessageMultiStream networkStream;
  private Dictionary<string, ServerRepData> serverReps = new Dictionary<string, ServerRepData>();
  private List<string> playerGuids = new List<string>();
  private Queue<string> abandonedPlayers = new Queue<string>();
  private Action<string, int> handlePlayerExit;

    public async Task<int> StartNetworked(Func<string, bool> acceptPlayer, Action<string, int> handlePlayerExit, MonoBehaviour parent, DemoSettingsServer settings) {
    this.handlePlayerExit = handlePlayerExit;
    parent.StartCoroutine(Update());
    var spawnPoint = DemoSettings.GetSpawnPoint();
    
    var platformerConfig = DemoSettings.GetPlatformCharacterConfig();
    networkStream = await NetworkApi.AcceptInbound(parent, settings.port);
    networkStream.Recieve.Add((opCode, guid, message) => {
      if(opCode == OpCode.PLAYER_JOIN) {
        var joinMessage = (PlayerJoinMessage)message;
        acceptPlayer(joinMessage.playerSessionId);
        var onCancel = networkStream.Recieve.GetConnectionCancellation(guid);
        onCancel.Token.Register(() => {
          lock(abandonedPlayers){
            abandonedPlayers.Enqueue(guid);
          }
        });
        playerGuids.Add(guid);
        var serverRep = GameObject.Instantiate(settings.playerPrefab, spawnPoint, Quaternion.identity);
        var serverRepComp = serverRep.GetComponent<PlayerCompServer>();
        var serverRepRecieve = serverRepComp.RegisterNetwork(guid, (toSendCode, toSendMessage) => {
          playerGuids.ForEach((targetGuid) => {
            try{
              networkStream.Send(toSendCode, targetGuid, toSendMessage);
            } catch(Exception e) {
              Debug.Log("failed sending to player: " + e.Message);
            }
          });
          return 1;
        }, platformerConfig, settings);
        ServerRepData repData = new ServerRepData();
        repData.comp = serverRepComp;
        repData.session = joinMessage.playerSessionId;
        repData.recieve = serverRepRecieve;
        repData.guid = guid;
        serverReps[guid] = repData;
        var acceptMsg  = new PlayerAcceptMessage();
        acceptMsg.trustDistance = settings.trustDistance;
        playerGuids.ForEach((existingPlayer) => {
          acceptMsg.playerGuid = guid;
          var isSendingToNewlyAccepted =  PlayerGuidUtil.IsFor(acceptMsg, existingPlayer);
          acceptMsg.playerSessionId = isSendingToNewlyAccepted ? joinMessage.playerSessionId : string.Empty;
          networkStream.Send(OpCode.PLAYER_ACCEPTED, existingPlayer, acceptMsg);
          if(!isSendingToNewlyAccepted){
            acceptMsg.playerSessionId = string.Empty;
            acceptMsg.playerGuid = existingPlayer;
            networkStream.Send(OpCode.PLAYER_ACCEPTED, guid, acceptMsg);
          }
        });
        
      } else {
        if(serverReps.ContainsKey(guid)) {
          serverReps[guid].recieve(opCode, message);
        }
      }
      return 1;
    });
    return 1;
  }

  private IEnumerator<YieldInstruction> Update() {
    while(true){
      lock(abandonedPlayers){
        while(abandonedPlayers.Count > 0){
          var guid = abandonedPlayers.Dequeue();
          var value = serverReps[guid];
          GameObject.Destroy(value.comp.gameObject);
          serverReps.Remove(guid);
          playerGuids.Remove(guid);
          handlePlayerExit(value.session, playerGuids.Count);
        }
      }
      yield return new WaitForEndOfFrame();
    }
  }
  
  public void EndNetworked(){
    networkStream?.Recieve.GetAcceptCancellation().Cancel();
  }
}
