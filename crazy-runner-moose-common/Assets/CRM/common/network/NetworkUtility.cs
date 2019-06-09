using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetworkDebug {
  public static void LogBytes(byte[] bytes, int offset, int count) {
      String s = "";
      for(int i = offset; i < offset+count; i++){
          s = s + ", " + bytes[i];
      }
      Debug.Log(s);
  }
}