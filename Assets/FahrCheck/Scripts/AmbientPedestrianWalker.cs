using UnityEngine;
namespace FahrCheck
{
    public sealed class AmbientPedestrianWalker : MonoBehaviour
    {
        public CourseSession session;
        public Vector3 start,end;
        public float speed=.9f,phase;
        Rigidbody body;
        void Awake(){body=GetComponent<Rigidbody>();}
        void FixedUpdate()
        {
            if(session==null||!session.IsDriving)return;
            float length=Vector3.Distance(start,end);
            float along=Mathf.PingPong(phase+session.ElapsedSeconds*speed,length);
            bool returning=Mathf.Repeat(phase+session.ElapsedSeconds*speed,length*2)>length;
            Vector3 position=Vector3.Lerp(start,end,along/length);
            Vector3 direction=(returning?start-end:end-start).normalized;
            // The supplied character mesh faces local -Z.
            body.MovePosition(position);body.MoveRotation(Quaternion.LookRotation(-direction,Vector3.up));
        }
    }
}
