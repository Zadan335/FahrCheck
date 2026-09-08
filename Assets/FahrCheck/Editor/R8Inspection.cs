using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace FahrCheck.Editor {
 public static class R8Inspection {
  public static void Run(){
   var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/ArtbyziaR8/Audi-r8-FBX.fbx"));
   var sb=new StringBuilder();var all=new Bounds();bool first=true;
   foreach(var r in model.GetComponentsInChildren<Renderer>()){
    if(first){all=r.bounds;first=false;}else all.Encapsulate(r.bounds);
    sb.AppendLine(r.name+" center="+r.bounds.center.ToString("F4")+" size="+r.bounds.size.ToString("F4")+" materials="+string.Join(";",r.sharedMaterials.Select(m=>m.name)));
   }
   sb.Insert(0,"BOUNDS "+all+"\n");System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Logs/r8-meshes.txt"),sb.ToString());Object.DestroyImmediate(model);Debug.Log("R8 inspection complete: "+all);
  }
 }
}
