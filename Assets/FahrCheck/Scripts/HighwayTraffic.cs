using UnityEngine;
namespace FahrCheck
{
    public sealed class HighwayTraffic : MonoBehaviour
    {
        public CourseSession session;
        public Transform[] cars;
        public float[] positions,laneX,cruiseSpeeds;
        public int[] directions;
        float[] speeds;
        void Start(){speeds=(float[])cruiseSpeeds.Clone();}
        public float ClosestAhead(float x,float z){float result=999;for(int i=0;i<cars.Length;i++)if(directions[i]>0&&Mathf.Abs(laneX[i]-x)<1.7f){float gap=positions[i]-z;if(gap>0)result=Mathf.Min(result,gap);}return result;}
        public bool OvertakingJustified(float x,float z){for(int i=0;i<cars.Length;i++)if(directions[i]>0&&laneX[i]>x&&laneX[i]-x<5){float gap=positions[i]-z;if(gap>0&&gap<60)return true;}return false;}
        void FixedUpdate()
        {
            if(!session.IsDriving||!session.Mission.Highway)return;
            for(int i=0;i<cars.Length;i++)
            {
                bool blocked=false;float direction=directions[i];
                if(TrafficSafety.ObstacleAhead(laneX[i],positions[i],directions[i],session.car.transform.position.x,session.car.transform.position.z,24))blocked=true;
                for(int j=0;j<cars.Length;j++)if(i!=j&&directions[j]==directions[i]&&Mathf.Abs(laneX[j]-laneX[i])<.5f&&TrafficSafety.ObstacleAhead(laneX[i],positions[i],directions[i],laneX[j],positions[j],20))blocked=true;
                speeds[i]=Mathf.MoveTowards(speeds[i],blocked?0:cruiseSpeeds[i],(blocked?12:3)*Time.fixedDeltaTime);
                positions[i]+=speeds[i]*direction*Time.fixedDeltaTime;
                if(positions[i]>2050)positions[i]=-20;if(positions[i]<-20)positions[i]=2050;
                cars[i].GetComponent<Rigidbody>().MovePosition(new Vector3(laneX[i],.2f,positions[i]));
            }
        }
    }
}
