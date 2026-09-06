using System;
using System.Collections;
using UnityEngine;
namespace FahrCheck
{
    public static class AcademyRuntimeVerification
    {
        static CourseSession Session => UnityEngine.Object.FindObjectOfType<CourseSession>();
        static IEnumerator Choose(int mission,int direction=1)
        {
            Session.Menu();yield return null;yield return null;
            Session.SelectMission(mission);Session.SelectRoute(direction);Session.Begin();Session.car.AutomatedInput=new CarController.Controls{brake=true};
            yield return new WaitForFixedUpdate();
        }
        public static IEnumerator Run(Action<bool,string> check)
        {
            yield return Choose(0);Session.car.Body.position=new Vector3(3,.4f,111);yield return new WaitForSecondsRealtime(.2f);
            check(Session.State==SessionState.Complete,"Lesson 1 completes without irrelevant junction checkpoints");
            yield return Choose(1);yield return new WaitForSeconds(10.2f);Session.car.Body.position=new Vector3(3,.4f,156);yield return new WaitForSeconds(10);
            Session.car.Body.position=new Vector3(3,.4f,211);yield return new WaitForSecondsRealtime(.2f);
            check(Session.State==SessionState.Complete,"Lesson 2 signal and priority completion");
            yield return Choose(2);yield return new WaitForSeconds(14);Session.car.Body.position=new Vector3(3,.4f,264);yield return new WaitForSecondsRealtime(.2f);
            check(Session.State==SessionState.Failed && Session.ResultTitle=="STOP sign not obeyed" && Session.Penalty.Contains("EUR 10"),"Rolling STOP is assessed with sourced rate");
            yield return Choose(2);yield return new WaitForSeconds(14);Session.car.Body.position=new Vector3(3,.4f,261);yield return new WaitForSecondsRealtime(.2f);
            check(Session.PassedStop,"Complete stop recorded behind line");Session.car.Body.position=new Vector3(3,.4f,276);yield return new WaitForSecondsRealtime(.2f);
            check(Session.State==SessionState.Complete,"Lesson 3 crossing and STOP completion");
            yield return Choose(0);var car=Session.car;
            var obstacle=new GameObject("Collision test vehicle");var collider=obstacle.AddComponent<BoxCollider>();collider.size=new Vector3(2,1.5f,1);
            obstacle.transform.position=new Vector3(3,1,24);obstacle.AddComponent<CollisionParticipant>();var body=obstacle.AddComponent<Rigidbody>();body.isKinematic=true;
            car.AutomatedInput=new CarController.Controls{throttle=1};car.Body.velocity=Vector3.forward*3;
            yield return new WaitForSecondsRealtime(2);
            check(Session.State==SessionState.Failed && Session.ResultTitle=="Collision","Physical vehicle contact ends attempt");
            check(string.IsNullOrEmpty(Session.Penalty),"Collision alone does not invent liability or fine");
            yield return Choose(3,-1);yield return new WaitForSeconds(10.2f);car=Session.car;car.Body.position=new Vector3(3,.4f,156);yield return new WaitForSeconds(10);
            car.Body.position=new Vector3(3,.4f,211);yield return new WaitForSeconds(14);car.Body.position=new Vector3(3,.4f,280);car.SetIndicator(-1);yield return new WaitForSeconds(1.2f);
            // Isolate the physical arc from NPC timing; those conflicts have separate integration coverage.
            var traffic=UnityEngine.Object.FindObjectOfType<AmbientTraffic>();traffic.enabled=false;
            for(int i=0;i<traffic.cars.Length;i++){traffic.positions[i]=360;traffic.cars[i].gameObject.SetActive(false);}
            car.Body.position=new Vector3(3,.3f,286);car.Body.rotation=Quaternion.identity;car.Body.velocity=Vector3.zero;
            float deadline=Time.realtimeSinceStartup+65;
            while(Session.IsDriving && Time.realtimeSinceStartup<deadline)
            {
                Vector3 p=car.transform.position;Vector3 target;
                if(p.x> -4)
                {
                    float theta=Mathf.Clamp(Mathf.Atan2(p.z-286,p.x+3),0,Mathf.PI/2);
                    theta=Mathf.Min(Mathf.PI/2,theta+.5f);
                    target=new Vector3(-3+6*Mathf.Cos(theta),p.y,286+6*Mathf.Sin(theta));
                    if(theta>=Mathf.PI/2 && p.x<0)target=new Vector3(-8,p.y,292);
                }
                else target=new Vector3(p.x-5,p.y,292);
                Vector3 local=car.transform.InverseTransformPoint(target);
                float steer=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*Mathf.Rad2Deg/22,-1,1);
                car.AutomatedInput=new CarController.Controls{throttle=car.SpeedKmh<9?.4f:0,brake=car.SpeedKmh>11,steer=steer};
                yield return new WaitForFixedUpdate();
            }
            check(Session.State==SessionState.Complete,"Actual wheel-driven curved left turn reaches destination");
        }
    }
}
