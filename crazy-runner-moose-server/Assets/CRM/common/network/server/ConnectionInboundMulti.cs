using System.Collections.Generic;
using System.Threading;

public class InboundMultiMessageSender {
  
  private readonly Dictionary<string, MessageHandler> clientSenders = new Dictionary<string, MessageHandler>();
  
  public MessageHandler GetClientSender(string clientGuid) {
    MessageHandler clientSender = null;
    lock(this){
      clientSender = clientSenders[clientGuid];
    }
    return clientSender;
  }

  public void AddClientSender(string clientGuid, MessageHandler clientSender) {
    lock(this){
      clientSenders.Add(clientGuid, clientSender);
    }
  }
  public MessageHandlerIndexed GetSender() {
    return (ushort opCode, string clientId, MessageTransferable message) => {
      return GetClientSender(clientId)?.Invoke(opCode, message) ?? 0;
    };
  }
}

public class InboundMultiMessageSubscription : MultiMessageSubscription {
  
  private readonly Dictionary<string, MessageSubscription> clientSubscriptions = new Dictionary<string, MessageSubscription>();
  private readonly CancellationTokenSource acceptCancelation;

  private MessageHandlerIndexed subscriber;

    public InboundMultiMessageSubscription(CancellationTokenSource acceptCancelation){
    this.acceptCancelation = acceptCancelation;
  }

  

  public MessageSubscription GetClientSubscription(string clientGuid) {
    MessageSubscription clientSubscription = null;
    lock(this){
      clientSubscription = clientSubscriptions[clientGuid];
    }
    return clientSubscription;
  }

  public void AddClientSubscription(string clientGuid, MessageSubscription clientSubscription) {
    lock(this){
      clientSubscriptions.Add(clientGuid, clientSubscription);
      clientSubscription.GetCancellation().Token.Register(() => {
        lock(this){
          clientSubscriptions.Remove(clientGuid);
        }
        clientSubscription.Remove();
      });
      clientSubscription.Add((ushort opCode, MessageTransferable message) => {
        return subscriber?.Invoke(opCode, clientGuid, message) ?? 0;
      });
    }
  }

  void MultiMessageSubscription.Add(MessageHandlerIndexed subscriber) {
      lock(this){
        this.subscriber = subscriber;
      }
  }

  CancellationTokenSource MultiMessageSubscription.GetAcceptCancellation() => this.acceptCancelation;
  
  CancellationTokenSource MultiMessageSubscription.GetConnectionCancellation(string clientGuid) => GetClientSubscription(clientGuid).GetCancellation();
}