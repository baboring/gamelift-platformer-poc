using UnityEngine;

public class AnimationTriggerer : MonoBehaviour {

  public Animator anim;

  public void Start() {
    anim = anim ?? GetComponent<Animator>();
  }

  public void Trigger(string triggerName) {
    anim.SetTrigger(triggerName);
  }

}