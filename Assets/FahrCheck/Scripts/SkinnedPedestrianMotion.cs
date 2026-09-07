using UnityEngine;
namespace FahrCheck {
 public sealed class SkinnedPedestrianMotion : MonoBehaviour {
  public Animator animator; Vector3 previous; bool ready;
  void OnEnable(){previous=transform.position;ready=false;}
  void Update(){
   if(animator==null || Time.deltaTime<=0)return;
   float speed=ready?Vector3.Distance(previous,transform.position)/Time.deltaTime:0;previous=transform.position;ready=true;
   // Teleports are scenario resets, not footsteps. Encounters own the world position.
   animator.SetFloat("Speed",speed>8?0:speed,.15f,Time.deltaTime);
  }
 }
}
