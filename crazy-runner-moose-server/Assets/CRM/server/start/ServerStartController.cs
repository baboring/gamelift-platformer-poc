using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aws.GameLift.Server;
using Aws.GameLift;
using Aws.GameLift.Server.Model;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

[Serializable]
public class GetSessionResponse {
  public string PlayerSessionId;
  public string PlayerId;
  public string GameSessionId;
  public string FleetId;
  public string CreationTime;
  public string Status;
  public string IpAddress;
  public int Port;
}

public class ServerStartController : MonoBehaviour {

  public DemoSettingsServer settings;
  private List<GameSession> startRequests = new List<GameSession>();
  private DemoServer server;
  

  void Start() {
    LogOutcome("init", GameLiftServerAPI.InitSDK());
    var logParams = new LogParameters(new List<string>());
    var parameters = new ProcessParameters(OnStartGameSession, OnUpdateGameSession, OnProcessTerminated, OnHealthCheck, 1900, logParams);
    LogOutcome("ready",GameLiftServerAPI.ProcessReady(parameters));
  }

  public void OnDestroy(){
    server?.EndNetworked();
  }

  public void Update(){
    startRequests.ForEach(ActivateSession);
    startRequests.Clear();
  }

  private async void ActivateSession(GameSession gameSession) {
    try {
      Debug.Log("server start game session: " + gameSession.GameSessionId);
      server = new DemoServer();
      await server.StartNetworked(AcceptPlayer, HandlePlayerExit, this, settings);
      LogOutcome("activate", GameLiftServerAPI.ActivateGameSession());
    } catch (Exception e) {
      Debug.LogError("Exception on server update");
      Debug.LogError(e);
    }
  }

  private bool AcceptPlayer(string playerSessionId) {
    var result = GameLiftServerAPI.AcceptPlayerSession(playerSessionId);
    return result.Success;
  }

  private void HandlePlayerExit(string playerSessionId, int remaining){
    LogOutcome("player term", GameLiftServerAPI.RemovePlayerSession(playerSessionId));
    if(remaining == 0) {
      LogOutcome("game term", GameLiftServerAPI.TerminateGameSession());
      LogOutcome("ending", GameLiftServerAPI.ProcessEnding());
      Application.Quit(0);
    }
  }

  private bool OnHealthCheck(){
      return true;
  }

  private void OnProcessTerminated(){
      Debug.Log("server process terminated");
  }

  private void OnUpdateGameSession(UpdateGameSession updateGameSession){
      Debug.Log("server game session updated: " + updateGameSession.GameSession.GameSessionId);
  }

  private void OnStartGameSession(GameSession gameSession) {
    startRequests.Add(gameSession);
  }

  private static void LogOutcome(string ident, GenericOutcome outcome) {
    Debug.Log(ident + ": " + outcome.Success);
    if(outcome.Error != null) {
      Debug.LogError(outcome.Error.ErrorName + ": " + outcome.Error.ErrorMessage);
    }
  }

}
