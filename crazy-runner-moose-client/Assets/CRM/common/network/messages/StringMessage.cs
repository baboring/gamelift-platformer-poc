using System;
using System.IO;


public class StringMessage : MessageTransferable {
  
  public string payload;
  
  public void Deserialize(BinaryReader reader){
    payload = reader.ReadString();
  }

  public void Serialize(BinaryWriter writer){
    writer.Write(payload);
  }
}