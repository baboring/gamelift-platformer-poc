

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class MessageTuple : Tuple<ushort, Type> {
    public MessageTuple(ushort item1, Type item2) : base(item1, item2){}
}

public class MessageTransferLookUp {
  private class MessageTransfer {
    private static readonly int MAX_CACHE_SIZE = 100;
    private readonly Stack<MessageTransferable> cache = new Stack<MessageTransferable>();
    private readonly Type transferrableType;

    public MessageTransfer(Type transferrableType){
      this.transferrableType = transferrableType;
    }

    public void Serialize(BinaryWriter writer, MessageTransferable instance, ushort opCode) {
      if(instance.GetType() == transferrableType){
        instance.Serialize(writer);
      } else {
        throw new Exception("serialization of wrong type for " + opCode);
      }
    }

    public MessageTransferable Deserialize(BinaryReader reader) {
      var instance = RecyclePop();
      instance.Deserialize(reader);
      return instance;
    }

    public void RecyclePush(MessageTransferable instance) {
      if(instance.GetType() == transferrableType){
        lock(this){
          if(cache.Count < MAX_CACHE_SIZE) {
            cache.Push(instance);
          }
        }
      } else {
        throw new Exception("wrong type pushed to recycle");
      }
    }

    private static readonly Type[] EMPTY_TYPES = new Type[0];
    private static readonly object[] EMPTY_ARGS = new object[0];
    private MessageTransferable GetNew() {
      return (MessageTransferable)transferrableType.GetConstructor(EMPTY_TYPES).Invoke(EMPTY_ARGS);
    }

    public MessageTransferable RecyclePop() {
      MessageTransferable val = null;
      lock(this){
        val =  cache.Count > 0 ? cache.Pop() : GetNew();
      }
      return val;
    }

  }

  private readonly Dictionary<ushort, MessageTransfer> lookup;
  
  public MessageTransferLookUp(MessageTuple[] items){
    lookup = new Dictionary<ushort, MessageTransfer>();
    foreach(var tuple in items) {
      lookup.Add(tuple.Item1, new MessageTransfer(tuple.Item2));
    }
  }

  public void Serialize(ushort opCode, BinaryWriter writer, MessageTransferable instance) {
    lookup[opCode].Serialize(writer, instance, opCode);
  }

  public MessageTransferable Deserialize(ushort opCode, BinaryReader reader) {
    return lookup[opCode].Deserialize(reader);
  }

  public void Return(ushort opCode, MessageTransferable instance) {
    lookup[opCode].RecyclePush(instance);
  }

}
