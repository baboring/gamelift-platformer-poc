using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDemoClientComp : MonoBehaviour {
  
  public DemoSettingsClient settings;
  public string guid;
  private DemoClient client = new DemoClient();

  public void StartCommunications(int port) {
    client.StartNetworked(port, "127.0.0.1", guid, this, settings);
  }

  void OnDestroy(){
    client.EndNetworked();
  }
}
