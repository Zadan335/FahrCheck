using UnityEditor;
using UnityEngine;
namespace FahrCheck.Editor {
public static class AssetInspection {
 public static void Run(){
 foreach(var path in new[]{"Assets/ThirdParty/FreeCoupeCar/Car.prefab"}){
 var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);var go=Object.Instantiate(asset);Debug.Log("ASSET "+path);
 foreach(var r in go.GetComponentsInChildren<Renderer>())Debug.Log("MESH "+r.name+" bounds="+r.bounds+" materials="+string.Join(",",System.Array.ConvertAll(r.sharedMaterials,m=>m==null?"null":m.name)));
 Object.DestroyImmediate(go);
 }
 foreach(var guid in AssetDatabase.FindAssets("t:Model",new[]{"Assets/ThirdParty"})){
 var path=AssetDatabase.GUIDToAssetPath(guid); Debug.Log("MODEL "+path);
 var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path)); foreach(var r in model.GetComponentsInChildren<SkinnedMeshRenderer>()) Debug.Log("SKIN "+r.name+" "+r.bounds+" local="+r.sharedMesh.bounds+" bones="+string.Join(",",System.Array.ConvertAll(r.bones,b=>b.name))); Object.DestroyImmediate(model);
 foreach(var a in AssetDatabase.LoadAllAssetsAtPath(path))if(a is AnimationClip c)Debug.Log("CLIP "+c.name+" length="+c.length);
 }
 }
}}
