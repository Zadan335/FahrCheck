using System;
using UnityEngine;
namespace FahrCheck.Editor
{
    public static class AcademyChecks
    {
        public static void Run()
        {
            int n=0;Action<bool,string> check=(ok,label)=>{n++;if(!ok)throw new Exception("Academy check failed: "+label);};
            check(MissionCatalog.All.Length==4,"four missions");
            check(MissionCatalog.All[0].Finish==110 && MissionCatalog.All[1].Signals && MissionCatalog.All[2].Stop && MissionCatalog.All[3].Turn,"distinct mission objectives");
            check(RouteRules.TurnArea(-1,298),"wide natural left-turn arc is recognised");
            check(RouteRules.Destination(-56,293,-1),"left destination zone");
            check(RouteRules.Destination(-57,293,-1),"remaining inside destination does not need another line crossing");
            check(!RouteRules.Destination(-56,293,1),"wrong branch cannot win");
            check(!RouteRules.Destination(-56,275,-1),"outside destination cannot win");
            check(FineCatalog.Speed(10).Euros==30 && FineCatalog.Speed(10.1f).Euros==50,"speed band boundary");
            check(FineCatalog.Speed(20).Points==0 && FineCatalog.Speed(21).Points==1,"speed points threshold");
            check(FineCatalog.Speed(31).BanMonths==1,"urban driving-ban band");
            check(FineCatalog.Red(1).Euros==90 && FineCatalog.Red(1.01f).Euros==200,"red duration boundary");
            check(FineCatalog.Indicator.Euros==10 && FineCatalog.Pedestrian.Euros==80,"sourced standard rates");
            check(FineCatalog.StopLine.Euros==10,"stop-line crossing not automatically full red fine");
            check(SignCatalog.All.Length>=20,"sign reference library");
            var human=UnityEngine.Object.FindObjectOfType<PedestrianAnimator>();
            check(human!=null && human.leftShin!=null && human.rightForearm!=null,"articulated human rig");
            check(human.GetComponent<Collider>()!=null,"pedestrian collision geometry");
            foreach(var actor in UnityEngine.Object.FindObjectsOfType<CollisionParticipant>()) check(actor.GetComponent<Collider>()!=null && actor.GetComponent<Rigidbody>()!=null,"actor has physics geometry");
            check(UnityEngine.Object.FindObjectsOfType<MissionGate>(true).Length==2,"configurable destination gates");
            Debug.Log("FAHRCHECK ACADEMY: "+n+" additional checks passed");
        }
    }
}
