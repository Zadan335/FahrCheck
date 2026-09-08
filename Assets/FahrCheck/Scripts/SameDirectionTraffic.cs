using UnityEngine;
namespace FahrCheck
{
    public sealed class SameDirectionTraffic : MonoBehaviour
    {
        public CourseSession session;
        public Transform[] cars;
        public float[] positions;
        public float cruiseSpeed=4.7f;
        float[] speeds;
        void Start(){speeds=new float[cars.Length];for(int i=0;i<speeds.Length;i++)speeds[i]=cruiseSpeed;if(RuntimeVerification.Active&&RuntimeVerification.SuppressLaneTraffic){foreach(var car in cars)car.gameObject.SetActive(false);enabled=false;}}
        void FixedUpdate()
        {
            if(!session.IsDriving)return;
            for(int i=0;i<cars.Length;i++)
            {
                float z=positions[i];bool blocked=false;
                if(TrafficSafety.ObstacleAhead(3,z,1,session.car.transform.position.x,session.car.transform.position.z,15))blocked=true;
                foreach(float other in positions)if(other>z&&other-z<10)blocked=true;
                speeds[i]=Mathf.MoveTowards(speeds[i],blocked?0:cruiseSpeed,(blocked?8:2.2f)*Time.fixedDeltaTime);
                float next=z+speeds[i]*Time.fixedDeltaTime;
                if(z<RuleLogic.StopLineZ&&next>=RuleLogic.StopLineZ&&RuleLogic.AspectAt(session.ElapsedSeconds)!=SignalAspect.Green){blocked=true;speeds[i]=0;}
                if(z<EncounterRules.CrossingEntryZ&&next>=EncounterRules.CrossingEntryZ&&EncounterRules.PedestrianHasPriority(session.encounters.PedestrianXAt(session.ElapsedSeconds),session.encounters.PedestrianIntent)){blocked=true;speeds[i]=0;}
                if(!blocked)positions[i]=next>370?0:next;
                cars[i].GetComponent<Rigidbody>().MovePosition(new Vector3(3,.2f,positions[i]));
            }
        }
    }
}
