using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace FahrCheck.Editor {
 public static class RealismChecks {
  public static void Run(){
   int count=0;Action<bool,string> check=(pass,label)=>{count++;if(!pass)throw new Exception("Realism check failed: "+label);};
   var car=UnityEngine.Object.FindObjectOfType<CarController>();
   check(car.transform.Find("Imported coupe")!=null,"imported player body");
   check(car.wheelVisuals.All(w=>w.GetComponentsInChildren<Renderer>().Any(r=>r.enabled && r.name.StartsWith("Wheel"))),"four imported wheel meshes attached to physics pivots");
   check(car.transform.Find("Audi R8 2017")!=null,"Audi player model");
   for(int i=0;i<4;i++){
    var wheel=car.wheelVisuals[i].Find("Wheel Audi "+i);check(wheel!=null,"Audi wheel group "+i);
    var tyre=wheel.GetComponentsInChildren<Renderer>().First(r=>r.sharedMaterial.name=="R8 tyre");check(Mathf.Abs(tyre.bounds.size.y*.5f-car.wheels[i].radius)<.02f,"Audi visual tyre matches collider radius "+i);
   }
   var traffic=UnityEngine.Object.FindObjectsOfType<TrafficWheelMotion>();
   check(traffic.Length==39,"all city and Autobahn cars have grounded rolling wheels");
   foreach(var vehicle in traffic){
    check(vehicle.wheels.Length==4,"four traffic wheels: "+vehicle.name);
    foreach(var wheel in vehicle.wheels){
     var renderers=wheel.GetComponentsInChildren<Renderer>();
     var tyre=renderers.FirstOrDefault(r=>r.sharedMaterial!=null && r.sharedMaterial.name=="R8 tyre") ?? renderers[0];
     check(Mathf.Abs(tyre.bounds.min.y-vehicle.surfaceHeight)<.015f,"tyre touches surface: "+vehicle.name);
    }
   }
   var trafficAudis=traffic.Where(t=>t.GetComponentsInChildren<Transform>().Any(x=>x.name.StartsWith("Traffic Audi R8"))).ToArray();
   check(trafficAudis.Length==5,"five Audi traffic cars mixed into fleet");
   var sameDirection=UnityEngine.Object.FindObjectOfType<SameDirectionTraffic>();check(sameDirection!=null&&sameDirection.cars.Length==3,"three cars travel in player lane with braking control");
   var highway=UnityEngine.Object.FindObjectOfType<HighwayTraffic>();check(highway!=null&&highway.cars.Length==25,"dense six-lane Autobahn traffic");
   var mountainTerrain=GameObject.FindObjectsOfType<MeshRenderer>().Where(t=>t.name.StartsWith("Continuous realistic mountain mesh ")).ToArray();
   check(mountainTerrain.Length==2,"continuous mountain meshes on both sides of Autobahn");
   check(mountainTerrain.All(t=>t.GetComponent<MeshFilter>().sharedMesh.vertexCount>30000&&t.sharedMaterial.shader.name=="FahrCheck/Mountain Terrain"),"mountains use detailed runtime-safe meshes and layered project shader");
   check(mountainTerrain.All(t=>t.GetComponent<MeshFilter>().sharedMesh.colors.Any(c=>c.r>.5f)&&t.GetComponent<MeshFilter>().sharedMesh.colors.Any(c=>c.g>.5f)&&t.GetComponent<MeshFilter>().sharedMesh.colors.Any(c=>c.b>.5f)),"mountain vertices blend meadow, rock and snow");
   check(GameObject.FindObjectsOfType<Transform>().Count(t=>t.name.StartsWith("Forested foothills "))==2,"forested mountain foothills");
   check(!GameObject.FindObjectsOfType<Transform>().Any(t=>t.name=="Mountain ridge"),"old geometric mountain wedges removed");
   check(!UnityEngine.Object.FindObjectsOfType<Terrain>().Any(t=>t.transform.IsChildOf(highway.transform)),"Autobahn avoids build-dependent Terrain renderer");
   check(trafficAudis.SelectMany(t=>t.GetComponentsInChildren<Renderer>()).SelectMany(r=>r.sharedMaterials).Where(m=>m.name.StartsWith("Traffic Audi paint ")).Select(m=>m.name).Distinct().Count()==5,"five distinct Audi traffic colours");
   var stations=GameObject.FindObjectsOfType<Transform>().Where(t=>t.name.StartsWith("Corner fuel station ")).ToArray();
   check(stations.Length==2,"two corner fuel stations");
   check(stations.All(s=>s.Find("Service shop").position.x>=34 && s.Find("Pump canopy").position.x>=18),"fuel station architecture stays outside junction sight triangle");
   check(stations.All(s=>s.GetComponentsInChildren<Transform>().Count(t=>t.name=="Fuel pump")==2 && s.GetComponentsInChildren<Transform>().Count(t=>t.name=="Fuel district building")==2),"detailed pumps and surrounding fuel-station architecture");
   check(GameObject.FindObjectsOfType<Transform>().Count(t=>t.name.StartsWith("Side street architecture "))==24,"side-road and final-road building rows");
   var walkers=UnityEngine.Object.FindObjectsOfType<AmbientPedestrianWalker>();check(walkers.Length==8,"eight animated pavement pedestrians");
   check(walkers.All(w=>Mathf.Abs(w.start.x)>6 || Mathf.Abs(w.start.z-290)>6),"ambient pedestrians remain outside vehicle lanes");
   var human=UnityEngine.Object.FindObjectOfType<SkinnedPedestrianMotion>();
   check(human.animator.avatar.isValid && human.animator.avatar.isHuman,"valid humanoid avatar");
   check(human.animator.runtimeAnimatorController.animationClips.Any(c=>c.name.EndsWith("|Idle_Loop")) && human.animator.runtimeAnimatorController.animationClips.Any(c=>c.name.EndsWith("|Walk_Loop") && c.isLooping),"idle and walking clips bound");
   check(!human.animator.applyRootMotion,"encounter owns pedestrian position");
   foreach(string n in new[]{"Asphalt","Pavement","Real brick facade"}){
    var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/FahrCheck/Generated/Materials/"+n+".mat");check(m.mainTexture!=null && m.GetTexture("_BumpMap")!=null,n+" photographed maps");
   }
   check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FahrCheck/Generated/Realism/EuropeanWindow.prefab")!=null,"reusable facade module");
   foreach(var r in UnityEngine.Object.FindObjectsOfType<Renderer>())if(r.enabled)foreach(var m in r.sharedMaterials)check(m!=null && m.shader!=null && m.shader.name!="Hidden/InternalErrorShader",r.name+" valid material");
   Debug.Log("FAHRCHECK REALISM: "+count+" asset and material checks passed");
  }
 }
}
