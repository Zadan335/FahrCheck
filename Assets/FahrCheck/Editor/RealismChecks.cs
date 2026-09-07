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
