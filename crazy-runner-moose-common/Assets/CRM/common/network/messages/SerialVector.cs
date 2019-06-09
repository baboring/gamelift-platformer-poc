using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SerialVector : MessageTransferable {
  public float x;
  public float y;
  public float z;
  
  public override string ToString(){
    return x + " " + y + " " + z;
  }
  public SerialVector(float x, float y, float z){
    this.x = x; 
    this.y = y;
    this.z = z;
  }

  public SerialVector() {}

  public SerialVector(Vector3 from){
    FromVector(from);
  }

  public Vector3 toVector(){
    return new Vector3(x, y, z);
  }

  public void FromVector(Vector3 from){
    this.x = from.x;
    this.y = from.y;
    this.z = from.z;
  }

  public void Deserialize(BinaryReader reader) {
      x = reader.ReadSingle();
      y = reader.ReadSingle();
      z = reader.ReadSingle();
  }

  public void Serialize(BinaryWriter writer){
      writer.Write(x);
      writer.Write(y);
      writer.Write(z);
  }
}
