using System;
using System.IO;
using UnityEngine;

public class PositionMessage : PlayerGuidMessage {
  
  public Vector3 position;
  public string playerGuid;

  public string PlayerGuid => playerGuid;

  public void Deserialize(BinaryReader reader){
    playerGuid = reader.ReadString();
    position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
  }

  public void Serialize(BinaryWriter writer){
    writer.Write(playerGuid);
    writer.Write(position.x);
    writer.Write(position.y);
    writer.Write(position.z);
  }
}