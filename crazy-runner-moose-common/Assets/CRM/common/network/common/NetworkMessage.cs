using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class NetworkMessageSerializer {
  
  public class MessageSerializationData {
    public MessageHandler serialize;
    public byte[] buffer = new byte[ushort.MaxValue];
  } 
  
  public static MessageSerializationData Get(MessageTransferLookUp lookUp){
    var opCodeBuf = new byte[2];
    var lenBuf = new byte[2];
    var data = new MessageSerializationData();
    MemoryStream m = new MemoryStream(data.buffer);
    var writer = new BinaryWriter(m);
    MessageHandler serialize  = (ushort opCode, MessageTransferable toSerialize) => {
      m.Position = 4;
      lookUp.Serialize((ushort)opCode, writer, toSerialize);
      var newPosition = m.Position;
      var length = newPosition - 4;
      m.Position = 0;
      writer.Write((ushort)length);
      writer.Write(opCode);
      return (ushort)(newPosition);
    };
    data.serialize = serialize;
    return data;
  }


}


