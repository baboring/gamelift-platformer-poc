using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


public class ActivatableTCPListener : TcpListener {
    public ActivatableTCPListener(IPAddress localaddr, int port) : base(localaddr, port){}

    public bool IsActive() => Active;
}

public class CancelableTcpClient {

  private readonly TcpClient client;
   private readonly CancellationTokenSource tokenSource;

  public CancelableTcpClient(TcpClient client){
    this.client = client;
    this.tokenSource = new CancellationTokenSource();
    tokenSource.Token.Register(() => {
      if(client.Connected) {
        Debug.Log("closing connection via cancellation request");
        client.Close();
      }
    });
  }

  public int Read(byte[] readBuffer, int start, int count){
    tokenSource.Token.ThrowIfCancellationRequested();
    int len = 0;
    try {
      len = client.GetStream().Read(readBuffer, start, count);
    } catch (Exception io){
      Debug.Log("read aborted");
      Debug.Log(io);
      tokenSource.Cancel();
    }
    tokenSource.Token.ThrowIfCancellationRequested();
    return len;
  }

  public void Write(byte[] writeBuffer, int start, int count){
    tokenSource.Token.ThrowIfCancellationRequested();
    try {
      client.GetStream().Write(writeBuffer, start, count);
    } catch (Exception io){
      Debug.Log("write aborted");
      Debug.Log(io);
      tokenSource.Cancel();
    }
    tokenSource.Token.ThrowIfCancellationRequested();
  }
  public CancellationTokenSource GetCancellationTokenSource() => tokenSource;
}

public class CancelableTcpListener{
  
  private readonly ActivatableTCPListener listener;
  private readonly CancellationTokenSource tokenSource;
  private readonly Dictionary<string, CancelableTcpClient> clients = new Dictionary<string, CancelableTcpClient>();
  
  public CancelableTcpListener(ActivatableTCPListener listener){
    this.listener = listener;
    tokenSource = new CancellationTokenSource();
    tokenSource.Token.Register(() => {
      if(listener.IsActive()){
        listener.Stop();
      }
      new List<CancelableTcpClient>(clients.Values).ForEach((c) => c.GetCancellationTokenSource().Cancel());
      clients.Clear();
    });
  }

  public void Start(){
    tokenSource.Token.ThrowIfCancellationRequested();
    listener.Start();
  }

  public string Accept(){
    if(!listener.IsActive()){
      tokenSource.Cancel();
    }
    tokenSource.Token.ThrowIfCancellationRequested();
    TcpClient client = null;
    try {
      client = listener.AcceptTcpClient();
    } catch(Exception io){
      Debug.Log("accept aborted");
      Debug.Log(io);
      tokenSource.Cancel();
    }
    tokenSource.Token.ThrowIfCancellationRequested();
    var cancelClient = new CancelableTcpClient(client);
    var guid = Guid.NewGuid().ToString();
    clients.Add(guid, cancelClient);
    cancelClient.GetCancellationTokenSource().Token.Register(() => {
      if(clients.ContainsKey(guid)){
        clients.Remove(guid);
      }
    });
    return guid;
  }

  public CancelableTcpClient GetClient(string guid){
    tokenSource.Token.ThrowIfCancellationRequested();
    return clients[guid];
  }

  public CancellationTokenSource GetCancellationTokenSource() => tokenSource;
}