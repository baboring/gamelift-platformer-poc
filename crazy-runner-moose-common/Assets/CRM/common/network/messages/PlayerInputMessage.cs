using System;
using System.IO;
using UnityEngine;

public class PlatformerCharacterInputState : MessageTransferable {
    
    public bool wasJumpRequested;
    public PlatformerWalkDirection walkDirection;
    public bool isJumpRequested;
    
    public void Deserialize(BinaryReader reader) {
        isJumpRequested = reader.ReadBoolean();
        walkDirection = (PlatformerWalkDirection)reader.ReadInt32();
    }

    public void Serialize(BinaryWriter writer){
        writer.Write(isJumpRequested);
        writer.Write((int)walkDirection);
    }

    public void CopyTo(PlatformerCharacterInputState other){
      other.walkDirection = walkDirection;
      other.isJumpRequested = isJumpRequested;
    }

    public bool Equals(PlatformerCharacterInputState other){
      return walkDirection == other.walkDirection && isJumpRequested == other.isJumpRequested;
    }
}

public class PlayerInputMessage : PlayerGuidMessage {
  
  public PlatformerCharacterInputState state = new PlatformerCharacterInputState();
  public SerialVector position = new SerialVector();
  public string playerGuid;

    public string PlayerGuid => playerGuid;

    public void Deserialize(BinaryReader reader){
    position.Deserialize(reader);
    playerGuid = reader.ReadString();
    state.Deserialize(reader);
  }

  public void Serialize(BinaryWriter writer){
    position.Serialize(writer);
    writer.Write(playerGuid);
    state.Serialize(writer);
  }
}