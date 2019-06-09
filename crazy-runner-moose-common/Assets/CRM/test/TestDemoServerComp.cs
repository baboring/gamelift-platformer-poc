using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestDemoServerComp : MonoBehaviour {
  
  public DemoSettingsServer settings;
  private DemoServer server;


  async void Start() {
    server = new DemoServer();
    await server.StartNetworked((_) => true, (ses, remaining) => Debug.Log("session exited: " + ses + ", remaining:" + remaining), this, settings);
    var clients = GameObject.FindObjectsOfType<TestDemoClientComp>();
    Array.ForEach(clients, (client) => client.StartCommunications(settings.port));
  }

  void OnDestroy(){
    server.EndNetworked();
  }
}
