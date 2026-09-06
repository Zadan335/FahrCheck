using UnityEngine;
namespace FahrCheck
{
    public sealed class PedestrianAnimator : MonoBehaviour
    {
        public Transform hips, head, leftThigh, rightThigh, leftShin, rightShin, leftArm, rightArm, leftForearm, rightForearm;
        Vector3 previous; float phase, blend;
        void Start() { previous=transform.position; }
        void LateUpdate()
        {
            float distance=Vector3.ProjectOnPlane(transform.position-previous,Vector3.up).magnitude; previous=transform.position;
            if(Time.deltaTime<=0) return;
            blend=Mathf.MoveTowards(blend,distance/Time.deltaTime>.1f?1:0,Time.deltaTime*5);
            phase+=distance*7.5f;
            float swing=Mathf.Sin(phase)*28*blend;
            leftThigh.localRotation=Quaternion.Euler(swing,0,0); rightThigh.localRotation=Quaternion.Euler(-swing,0,0);
            leftShin.localRotation=Quaternion.Euler(Mathf.Max(0,-Mathf.Sin(phase))*42*blend,0,0); rightShin.localRotation=Quaternion.Euler(Mathf.Max(0,Mathf.Sin(phase))*42*blend,0,0);
            leftArm.localRotation=Quaternion.Euler(-swing*.7f,0,6); rightArm.localRotation=Quaternion.Euler(swing*.7f,0,-6);
            leftForearm.localRotation=rightForearm.localRotation=Quaternion.Euler(-15-10*blend,0,0);
            hips.localPosition=new Vector3(0,.95f+Mathf.Abs(Mathf.Sin(phase))* .025f*blend,0);
            head.localRotation=Quaternion.Euler(0,blend<.1f?Mathf.Sin(Time.time*1.8f)*30:0,0);
        }
    }
}
