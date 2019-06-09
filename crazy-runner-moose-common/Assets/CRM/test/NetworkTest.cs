using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTest : MonoBehaviour {
  public KeyCode sendKey = KeyCode.Space;
  public KeyCode closeKey = KeyCode.X;
  public KeyCode serverCloseKey = KeyCode.Z;
  public int hellos = 0;
  private NetworkMessageMultiStream inbound;
  private NetworkMessageStream outbound;

  async void Start() {
    inbound = await NetworkApi.AcceptInbound(this, 9080);
    inbound.Recieve.Add(OnInBoundRecieve);
    outbound = NetworkApi.OpenOutbound(9080, "127.0.0.1", this);
    outbound.Recieve.Add(OnOutboundRecieve);
  }

   
  private int OnOutboundRecieve(ushort opCode, MessageTransferable data) {
    Debug.Log("outboundback: " + opCode);
    return 1;
  }

  private int OnInBoundRecieve(ushort opCode, string clientGuid, MessageTransferable data) {
    Debug.Log("address: " + clientGuid);
    if(opCode == OpCode.PLAYER_JOIN) {
      Debug.Log(((PlayerJoinMessage)data).playerSessionId);
    }
    return 1;
  }

public void Update(){
  if(Input.GetKeyUp(closeKey)){
    outbound.Recieve.GetCancellation().Cancel();
  }
  if(Input.GetKeyUp(serverCloseKey)) {
    inbound.Recieve.GetAcceptCancellation().Cancel();
  }
  if(Input.GetKey(sendKey)){
    var data = new PlayerJoinMessage();
    data.playerSessionId = "1234";
    outbound?.Send(OpCode.PLAYER_JOIN, data);
    var data2 = new PlayerJoinMessage();
    data2.playerSessionId = "xyz";
    outbound?.Send(OpCode.PLAYER_JOIN, data2);
  }
}

}
