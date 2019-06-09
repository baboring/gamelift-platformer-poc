using System;
using UnityEngine;

[Serializable]
public class DemoSettingsServer {
  
  public GameObject playerPrefab;
  public float tickRate = 0.5f;
  public int port = 1900;
  public float trustDistance = 1.0f;

}