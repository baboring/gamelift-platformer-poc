using System;
using System.IO;


public class PlayerJoinMessage : MessageTransferable {
  
  public string playerSessionId;
  
  public void Deserialize(BinaryReader reader){
    playerSessionId = reader.ReadString();
  }

  public void Serialize(BinaryWriter writer){
    writer.Write(playerSessionId);
  }
}