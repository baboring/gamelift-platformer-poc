using System;
using UnityEngine;


public class AnimationEventHandler : MonoBehaviour {
  public event Action<string> OnAnimEvent;
  public void HandleEvent(string s) {
      OnAnimEvent?.Invoke(s);
  }
}

