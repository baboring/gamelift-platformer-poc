
using System.IO;
using System.Threading;

public interface MessageTransferable {
  void Deserialize(BinaryReader reader);
  void Serialize(BinaryWriter writer);
}

public delegate int MessageHandlerIndexed(ushort opCode, string clientGuid, MessageTransferable data);
public delegate int MessageHandler(ushort opCode, MessageTransferable data);

public interface MessageSubscription {
  void Add(MessageHandler subscriber);
  void Remove();
  CancellationTokenSource GetCancellation(); 
}

public interface MultiMessageSubscription {
  void Add(MessageHandlerIndexed subscriber);
  CancellationTokenSource GetAcceptCancellation(); 
  CancellationTokenSource GetConnectionCancellation(string clientGuid);
}

public class NetworkMessageStream {
  public MessageHandler Send { get;}
  public MessageSubscription Recieve { get;}

  public NetworkMessageStream(MessageHandler send, MessageSubscription recieve){
    this.Send = send;
    this.Recieve = recieve;
  }
}

public class NetworkMessageMultiStream {
  public MessageHandlerIndexed Send { get;}
  public MultiMessageSubscription Recieve { get;}

  public NetworkMessageMultiStream(MessageHandlerIndexed send, MultiMessageSubscription recieve){
    this.Send = send;
    this.Recieve = recieve;
  }
}