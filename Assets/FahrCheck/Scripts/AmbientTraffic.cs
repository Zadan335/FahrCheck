using UnityEngine;
namespace FahrCheck
{
    // Oncoming traffic keeps spacing, stops for the shared phase and for the zebra pedestrian.
    public sealed class AmbientTraffic : MonoBehaviour
    {
        public CourseSession session;
        public Transform[] cars;
        public float[] positions;
        public float SpeedMetresPerSecond = 5.5f;
        float[] speeds;
        void Start(){speeds=new float[cars.Length];for(int i=0;i<speeds.Length;i++)speeds[i]=SpeedMetresPerSecond;}
        void FixedUpdate()
        {
            if (!session.IsDriving) return;
            for (int i=0;i<cars.Length;i++)
            {
                float z=positions[i];
                bool blocked=false;
                if(TrafficSafety.ObstacleAhead(-3,z,-1,session.car.transform.position.x,session.car.transform.position.z,14))blocked=true;
                foreach(float other in positions) if (other<z && z-other<8) blocked=true;
                speeds[i]=Mathf.MoveTowards(speeds[i],blocked?0:SpeedMetresPerSecond,(blocked?8:2.5f)*Time.fixedDeltaTime);
                float next=z-speeds[i]*Time.fixedDeltaTime;
                if (z>=145 && next<145 && RuleLogic.AspectAt(session.ElapsedSeconds)!=SignalAspect.Green) {blocked=true;speeds[i]=0;}
                if (z>=251 && next<251 && EncounterRules.PedestrianHasPriority(session.encounters.PedestrianXAt(session.ElapsedSeconds),session.encounters.PedestrianIntent)) {blocked=true;speeds[i]=0;}
                if (z>=199 && next<199 && Mathf.Abs(session.encounters.PriorityXAt(session.ElapsedSeconds))<12) {blocked=true;speeds[i]=0;}
                if (!blocked) positions[i]=next < 0 ? 370 : next;
                cars[i].GetComponent<Rigidbody>().MovePosition(new Vector3(-3,.2f,positions[i]));
            }
        }
    }
}
