using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[Serializable]
public class GetSessionResponse {
  public string PlayerSessionId;
  public string PlayerId;
  public string GameSessionId;
  public string FleetId;
  public string CreationTime;
  public string Status;
  public string IpAddress;
  public int Port;
}
public class ClientStartController : MonoBehaviour {

  public DemoSettingsClient settings;
  public bool isLocal = false;
  public GameObject playerPrefab;

  private DemoClient client = new DemoClient();

  public void OnDestroy() {
    client.EndNetworked();  
  }
    
  async void Start() {
    try {
      var jsonClient = new JsonRestClient(this);
      var url = isLocal ? "http://localhost:8081/" : "https://testapi.betterdoggo.com/crm-gamelift/";
      var response = await jsonClient.Get<GetSessionResponse>(url);
      Debug.Log("contacting server @" + response.IpAddress + " on:" + response.Port);
      client.StartNetworked(response.Port, response.IpAddress, response.PlayerSessionId, this, settings);
    } catch(Exception e) {
      Debug.LogError("failed to start");
      Debug.LogError(e);
    }
  }

}
