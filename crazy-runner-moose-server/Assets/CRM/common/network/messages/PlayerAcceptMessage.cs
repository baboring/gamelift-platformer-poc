using System;
using System.IO;


public class PlayerAcceptMessage : PlayerGuidMessage {
  
  public string playerSessionId;
  public string playerGuid;
  public float trustDistance;

  public string PlayerGuid => playerGuid;

  public void Deserialize(BinaryReader reader){
    playerSessionId = reader.ReadString();
    playerGuid = reader.ReadString();
    trustDistance = reader.ReadSingle();
  }

  public void Serialize(BinaryWriter writer){
    writer.Write(playerSessionId);
    writer.Write(playerGuid);
    writer.Write(trustDistance);
  }
}