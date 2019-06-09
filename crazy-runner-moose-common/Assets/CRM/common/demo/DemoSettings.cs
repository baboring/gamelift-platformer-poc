using UnityEngine;

public class DemoSettings : MonoBehaviour {
  public PlatformerCharacterConfig config;
  public Transform spawnPoint;
  
  public static PlatformerCharacterConfig GetPlatformCharacterConfig(){
    return GameObject.FindObjectOfType<DemoSettings>().config;
  }

  public static Vector3 GetSpawnPoint(){
    return GameObject.FindObjectOfType<DemoSettings>().spawnPoint.position;
  }
}