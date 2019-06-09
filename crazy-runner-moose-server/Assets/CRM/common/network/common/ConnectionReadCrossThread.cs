using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ConnectionReadCrossThread: MessageSubscription {
  
  private class MessageData {
    public ushort opCode;
    public MessageTransferable data;
  }

  private const int MAX_RECIEVE_QUEUE = 255;
  private readonly Queue<MessageData> msgDataPending = new Queue<MessageData>();
  private readonly Stack<MessageData> msgDataCache = new Stack<MessageData>();
  private readonly MessageTransferLookUp lookUp;
  private readonly CancellationTokenSource cancellation;
  private MessageHandler subscriber;

  public ConnectionReadCrossThread(MonoBehaviour parent, MessageTransferLookUp lookUp, CancellationTokenSource cancellation){
    this.lookUp = lookUp;
    this.cancellation = cancellation;
    parent.StartCoroutine(Update());
  }

  public void Recieve(ushort oPcode, MessageTransferable data) {
    lock(this) {
      while(msgDataPending.Count > MAX_RECIEVE_QUEUE) {
        var dropped = msgDataPending.Dequeue();
        if(msgDataCache.Count < MAX_RECIEVE_QUEUE) {
          msgDataCache.Push(dropped);
        }
      }
      var messageData = (msgDataCache.Count > 0) ? msgDataCache.Pop() : (new MessageData());
      messageData.data = data;
      messageData.opCode = oPcode;
      msgDataPending.Enqueue(messageData);
    }
  }

  public IEnumerator<YieldInstruction> Update() {
    var count = 0;
    while(true) {
      var shouldBreak = false;
      lock(this){
        shouldBreak = cancellation.Token.IsCancellationRequested;
      }
      if(shouldBreak){
        break;
      }
      if(subscriber != null) {
        do {
          MessageData messageData = null;
          lock (this){
            messageData = msgDataPending.Count > 0  ? msgDataPending.Dequeue() : null;
            count = msgDataPending.Count;
          }
          if(messageData != null) {
            subscriber.Invoke(messageData.opCode, messageData.data);
            lookUp.Return(messageData.opCode, messageData.data);
            messageData.data = null;
            messageData.opCode = 0;
            lock(this) {
              if(msgDataCache.Count < MAX_RECIEVE_QUEUE) {
                msgDataCache.Push(messageData);
              }
            }
          }
        } while(count > 0);
      }
      yield return new WaitForEndOfFrame();
    }
  }

  void MessageSubscription.Add(MessageHandler subscriber){
      this.subscriber = subscriber;
  }

  CancellationTokenSource MessageSubscription.GetCancellation() => cancellation;

  void MessageSubscription.Remove(){
      this.subscriber = null;
  }
}