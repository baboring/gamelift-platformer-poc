using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public delegate void OnClientConnect(int connectionInex, MessageHandler outboundHandler); 
public delegate void OnServerStart(CancellationTokenSource stopServer);

public class ServerCrossThread {
  
  public class AcceptData {
    public int connectionIndex;
    public MessageHandler sender;
  }
  
  private readonly OnClientConnect onConnect;
  private readonly OnServerStart onStart;
  private readonly Queue<AcceptData> accepted = new Queue<AcceptData>();
  
  private CancellationTokenSource cancelSource;
  private bool isStartNotifyRequested;

  public ServerCrossThread(OnClientConnect onConnect, OnServerStart onStart, MonoBehaviour parent) {
    this.onConnect = onConnect;
    this.onStart = onStart;
    parent.StartCoroutine(Update());
  }

  public void FlagStart(CancellationTokenSource cancelSource){
    lock(this){
      this.cancelSource = cancelSource;
      this.isStartNotifyRequested = true;
    }
  }
  
  public void Accept(int connectionIndex, MessageHandler sender) {
    var data = new AcceptData();
    data.connectionIndex = connectionIndex;
    data.sender = sender;
    lock(this){
      accepted.Enqueue(data);
    }
  }

  public IEnumerator<YieldInstruction> Update() {
    var count = 0;
    while(cancelSource == null || !cancelSource.Token.IsCancellationRequested){
      bool isStartNotify = false;
      lock (this){
        isStartNotify = isStartNotifyRequested;
      }
      if(isStartNotify){
        onStart(cancelSource);
      }
      do {
        AcceptData acceptData = null;
        lock (this){
          acceptData = accepted.Count > 0  ? accepted.Dequeue() : null;
          count = accepted.Count;
        }
        if(acceptData != null) {
          onConnect?.Invoke(acceptData.connectionIndex, acceptData.sender);
        }
      } while(count > 0);
      yield return new WaitForEndOfFrame();
    }
  }
}