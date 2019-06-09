using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


class InboundCrossThread {
  private readonly TaskCompletionSource<NetworkMessageMultiStream> task;
  private readonly Queue<Action<MonoBehaviour>> scheduled = new Queue<Action<MonoBehaviour>>();
  private readonly CancellationTokenSource cancellation;
  private readonly MonoBehaviour parent;
  private NetworkMessageMultiStream stream;
  
  public InboundCrossThread(TaskCompletionSource<NetworkMessageMultiStream> task, MonoBehaviour parent, CancellationTokenSource cancellation){
    this.task = task;
    this.cancellation = cancellation;
    this.parent = parent;
    parent.StartCoroutine(Update());
  }

  public void AcceptStream(NetworkMessageMultiStream stream){
    lock(this){
      this.stream = stream;
    }
  }

  public void Schedule(Action<MonoBehaviour> toSchedule){
    lock(this){
      scheduled.Enqueue(toSchedule);
    }
  }

  private IEnumerator<YieldInstruction> Update() {
    NetworkMessageMultiStream stream = null;
    while(stream == null){
      yield return new WaitForEndOfFrame();
      lock(this){
        stream = this.stream;
      }
    }
    task.SetResult(stream);
    while(!cancellation.IsCancellationRequested){
      yield return new WaitForEndOfFrame();
      lock(this){
        while(scheduled.Count > 0) {
          var action = scheduled.Dequeue();
          action?.Invoke(parent);
        }
      }
    }
  }
}

class ConnectionInBound {
  
  public static Task<NetworkMessageMultiStream> Accept(
    int port,
    MonoBehaviour parent,
    MessageTransferLookUp lookUp
  ) {
    IPAddress localAddr = IPAddress.Parse("0.0.0.0");
    var server = new CancelableTcpListener(new ActivatableTCPListener(localAddr, port));
    var taskSource = new TaskCompletionSource<NetworkMessageMultiStream>();
    var crossThread = new InboundCrossThread(taskSource, parent, server.GetCancellationTokenSource());
    Debug.Log("Delegating server port: " + port);
    Task.Factory
      .StartNew(() => Connect(server, crossThread, lookUp), server.GetCancellationTokenSource().Token)
      .ContinueWith((task) => {
        if(task.IsCanceled){
          Debug.Log("Server dispatch has been shutdown");
        } else if(task.IsFaulted){
          Debug.LogError("Exception in connection accept thread");
          Debug.LogError(task.Exception);
        } else {
          Debug.Log("not sure how this task shutdown");
        }
      });
      return taskSource.Task;
  }

  private static void Connect(
    CancelableTcpListener server, 
    InboundCrossThread crossThread,
    MessageTransferLookUp lookUp
  ) { 
    InboundMultiMessageSender sender = new InboundMultiMessageSender();
    InboundMultiMessageSubscription subscription = new InboundMultiMessageSubscription(server.GetCancellationTokenSource());
    server.Start();
    Debug.Log("Server dispatching connections");
    crossThread.AcceptStream(new NetworkMessageMultiStream(sender.GetSender(), subscription));
    while(true) {
      var guid = server.Accept();
      Debug.Log("Server connection established: " + guid);
      var client = server.GetClient(guid);
      crossThread.Schedule((parent) => {
        var clientSender = ConnectionWrite.Write(client, lookUp);
        var clientSubscription = ConnectionRead.Read(client, parent, lookUp);
        sender.AddClientSender(guid, clientSender);
        subscription.AddClientSubscription(guid, clientSubscription);
      });
    }
  }
}