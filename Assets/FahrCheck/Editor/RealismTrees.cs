using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace FahrCheck.Editor {
 public static partial class PrototypeBuilder {
  static void LeafyTrees(){
   // Original reusable leaf geometry. One shared mesh replaces the spherical crowns.
   string path=RealPath+"/LindenFoliage.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
   if(mesh==null){
    var vertices=new List<Vector3>();var indices=new List<int>();var colours=new List<Color>();var rng=new System.Random(731);
    for(int i=0;i<1800;i++){
     float angle=(float)rng.NextDouble()*Mathf.PI*2;float radius=Mathf.Sqrt((float)rng.NextDouble())*1.9f;float y=3.1f+(float)rng.NextDouble()*3.1f;
     float taper=Mathf.Sqrt(Mathf.Max(.05f,1-Mathf.Pow((y-4.6f)/1.8f,2)));
     var center=new Vector3(Mathf.Cos(angle)*radius*taper,y,Mathf.Sin(angle)*radius*taper);
     var rot=Quaternion.Euler((float)rng.NextDouble()*150,(float)rng.NextDouble()*360,0);float length=.13f+(float)rng.NextDouble()*.17f;
     var u=rot*Vector3.right*length*.5f;var v=rot*Vector3.forward*length;
     int n=vertices.Count;vertices.AddRange(new[]{center-v,center-u,center+v,center+u,center+rot*Vector3.up*.025f});
     int[] tri={0,1,4,1,2,4,2,3,4,3,0,4};foreach(int t in tri)indices.Add(n+t);
     float shade=.7f+(float)rng.NextDouble()*.4f;for(int j=0;j<5;j++)colours.Add(new Color(.3f,.43f,.14f)*shade);
    }
    mesh=new Mesh{name="Original individual linden leaves"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.SetColors(colours);mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,path);
   }
   var leaf=Mat("Natural linden foliage",new Color(.22f,.34f,.12f));leaf.shader=Shader.Find("FahrCheck/LeafSurface");
   foreach(var tree in world.GetComponentsInChildren<Transform>())if(tree.name=="Linden tree"){
    foreach(var r in tree.GetComponentsInChildren<Renderer>())if(r.name!="Trunk")r.enabled=false;
    var crown=new GameObject("Individual leaf canopy");crown.transform.SetParent(tree,false);crown.AddComponent<MeshFilter>().sharedMesh=mesh;crown.AddComponent<MeshRenderer>().sharedMaterial=leaf;
    for(int i=0;i<9;i++){
     float angle=i*137.5f*Mathf.Deg2Rad;var start=new Vector3(0,2.4f+i*.18f,0);var end=new Vector3(Mathf.Cos(angle)*1.5f,4+i*.17f,Mathf.Sin(angle)*1.5f);var delta=end-start;
     var branch=Shape("Linden branch",PrimitiveType.Cylinder,tree,(start+end)/2,new Vector3(.08f,delta.magnitude/2,.08f),Materials["Bark"]);branch.transform.localRotation=Quaternion.FromToRotation(Vector3.up,delta);
    }
   }
  }
 }
}
