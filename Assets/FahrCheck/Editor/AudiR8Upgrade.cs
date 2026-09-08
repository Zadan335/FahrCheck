using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace FahrCheck.Editor {
 public static partial class PrototypeBuilder {
  const string AudiPath="Assets/FahrCheck/Generated/Realism";
  static Material AudiMaterial(string name){
   string n=name.ToLowerInvariant();Color color=new Color(.055f,.06f,.065f);float metal=0,gloss=.22f;
   if(n=="body_color" || n=="frontcolor"){color=new Color(.38f,.44f,.5f);metal=.65f;gloss=.8f;}
   else if(n.Contains("chrome") || n.Contains("alum") || n.Contains("metal") || n.Contains("rims")){color=new Color(.55f,.57f,.59f);metal=.85f;gloss=.65f;}
   else if(n.Contains("tail_light") || n=="logo_red"){color=new Color(.48f,.008f,.012f);gloss=.7f;}
   else if(n=="light"){color=new Color(.85f,.9f,1f);gloss=.65f;}
   else if(n=="black_gloss"){color=new Color(.025f,.028f,.031f);gloss=.7f;}
   else if(n=="tyre"){color=new Color(.024f,.027f,.03f);gloss=.12f;}
   else if(n.StartsWith("interior")){color=new Color(.075f,.07f,.065f);gloss=.12f;}
   var m=Mat("R8 "+name,color);m.shader=Shader.Find("Standard");m.SetFloat("_Metallic",metal);m.SetFloat("_Glossiness",gloss);
   if(n.StartsWith("glass")){
    m.color=new Color(.55f,.65f,.7f,n=="glass_window"?.08f:.04f);m.SetFloat("_Mode",3);m.SetInt("_SrcBlend",(int)BlendMode.One);m.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);m.SetInt("_ZWrite",0);m.EnableKeyword("_ALPHAPREMULTIPLY_ON");m.renderQueue=3000;m.SetFloat("_Glossiness",.94f);
   }
   if(n=="light"){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",new Color(.5f,.58f,.7f));}
   if(n=="grille" || n=="trim" || n=="aluminum_2"){
    string texture=n=="aluminum_2"?"Aluminum_2":n;m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/ArtbyziaR8/r8.textures/"+texture+".jpg");m.color=Color.white;
   }
   EditorUtility.SetDirty(m);return m;
  }
  static void AudiR8Pass(CourseSession session){
   var car=session.car;
   foreach(var r in car.GetComponentsInChildren<Renderer>())r.enabled=false;
   var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/ArtbyziaR8/Audi-r8-FBX.fbx");
   if(source==null)throw new Exception("Audi source FBX is missing");
   var instance=Object.Instantiate(source);instance.name="Audi R8 2017";
   var renderers=instance.GetComponentsInChildren<MeshRenderer>();
   var tyres=renderers.Where(r=>r.sharedMaterial.name=="tyre").ToArray();
   if(tyres.Length!=4)throw new Exception("Audi requires exactly four identifiable tyres");
   float middleX=tyres.Average(t=>t.bounds.center.x),middleZ=tyres.Average(t=>t.bounds.center.z);
   float halfBase=(tyres.Max(t=>t.bounds.center.z)-tyres.Min(t=>t.bounds.center.z))/2;
   float scale=1.3f/halfBase;
   var originalCenters=tyres.Select(t=>t.bounds.center).ToArray();
   var wheelParts=new List<Transform>[4];for(int i=0;i<4;i++)wheelParts[i]=new List<Transform>();
   foreach(var r in renderers){
    int nearest=Enumerable.Range(0,4).OrderBy(i=>(r.bounds.center-originalCenters[i]).sqrMagnitude).First();
    Vector3 d=r.bounds.center-originalCenters[nearest];var size=r.bounds.size;
    // Tyres, rims, hubs and brake assemblies are separate meshes close to each wheel centre.
    if(Mathf.Abs(d.x)<1.6f && Mathf.Abs(d.y)<1.7f && Mathf.Abs(d.z)<1.7f && size.x<3.2f && size.y<6.4f && size.z<6.4f)wheelParts[nearest].Add(r.transform);
    r.sharedMaterials=r.sharedMaterials.Select(m=>AudiMaterial(m.name)).ToArray();r.enabled=true;
   }
   instance.transform.SetParent(car.transform,false);instance.transform.localScale=Vector3.one*scale;instance.transform.localPosition=new Vector3(-middleX*scale,.25f-originalCenters[0].y*scale,-middleZ*scale);
   for(int j=0;j<4;j++){
    int index=(originalCenters[j].z>middleZ?0:2)+(originalCenters[j].x>middleX?1:0);
    var pivot=new GameObject("Wheel Audi "+index).transform;pivot.SetParent(car.wheelVisuals[index],false);
    Vector3 center=instance.transform.TransformPoint(source.transform.InverseTransformPoint(originalCenters[j]));
    foreach(var part in wheelParts[j]){part.SetParent(pivot,true);part.position+=pivot.position-center;part.name="Wheel Audi part "+part.name;}
    // Match visible rubber radius to the unchanged WheelCollider radius.
    pivot.localScale=Vector3.one*(car.wheels[index].radius/(tyres[j].bounds.size.y*.5f));
   }
   var lamps=renderers.Where(r=>r!=null && r.sharedMaterial.name.Contains("tail_light_red_1")).Cast<Renderer>().ToArray();
   car.brakeLamps=lamps;foreach(var r in lamps)r.enabled=true;
   var tail=Mat("R8 rear lamp running",new Color(.35f,.006f,.009f));tail.SetFloat("_Glossiness",.75f);car.lampOff=tail;
   // Keep dedicated indicator meshes, resized and fitted beside the new lamp clusters.
   foreach(var pair in new[]{car.leftIndicators,car.rightIndicators})for(int i=0;i<pair.Length;i++){
    var r=pair[i];int side=pair==car.leftIndicators?-1:1;r.enabled=true;r.transform.localPosition=new Vector3(side*.75f,i==0?.50f:.61f,i==0?2.09f:-1.86f);r.transform.localScale=new Vector3(.11f,.045f,.022f);
   }
   // The supplied steering assembly is on the right. Mirror just that assembly for a left-hand driving position.
   var steering=new GameObject("Left-hand steering assembly").transform;steering.SetParent(car.transform,false);
   foreach(var r in renderers.Where(r=>r.name=="Mesh34" || r.name=="Mesh35"))r.transform.SetParent(steering,true);
   steering.localScale=new Vector3(-1,1,1);
   var camera=Object.FindObjectOfType<DrivingCamera>();camera.cockpitOffset=new Vector3(-.35f,.86f,.36f);
   foreach(var light in car.GetComponentsInChildren<Light>())light.transform.localPosition=new Vector3(Mathf.Sign(light.transform.localPosition.x)*.65f,.52f,2.08f);
   // A reusable assembled visual prefab contains the body and independently grouped wheels.
   var kit=new GameObject("Audi R8 visual kit");var bodyCopy=Object.Instantiate(instance,kit.transform);bodyCopy.name="Body";Object.Instantiate(steering,kit.transform).name="Left-hand steering assembly";
   for(int i=0;i<4;i++){var copy=Object.Instantiate(car.wheelVisuals[i].Find("Wheel Audi "+i),kit.transform);copy.name="Wheel "+i;copy.localPosition=car.wheelVisuals[i].localPosition;}
   PrefabUtility.SaveAsPrefabAsset(kit,AudiPath+"/AudiR8Visual.prefab");Object.DestroyImmediate(kit);
   UpgradeTrafficToAudis();
   Debug.Log("AUDI INTEGRATION: 4 wheel groups, "+renderers.Length+" source mesh parts, scale="+scale);
  }
  static void UpgradeTrafficToAudis(){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AudiPath+"/AudiR8Visual.prefab");
   var colours=new[]{new Color(.55f,.025f,.018f),new Color(.025f,.12f,.32f),new Color(.82f,.84f,.86f),new Color(.055f,.06f,.068f),new Color(.12f,.28f,.19f)};
   string[] names={"Oncoming car 0","Oncoming car 2","Parked car 1","Parked car 4","Priority car"};
   for(int index=0;index<names.Length;index++){
    var motion=Object.FindObjectsOfType<TrafficWheelMotion>().FirstOrDefault(t=>t.name==names[index]);
    if(motion==null)throw new Exception("Traffic actor missing for Audi upgrade: "+names[index]);
    foreach(var renderer in motion.GetComponentsInChildren<Renderer>())renderer.enabled=false;
    var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
    PrefabUtility.UnpackPrefabInstance(visual,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
    visual.name="Traffic Audi R8 "+(index+1);visual.transform.SetParent(motion.transform,false);
    var paint=Mat("Traffic Audi paint "+(index+1),colours[index]);paint.shader=Shader.Find("Standard");paint.SetFloat("_Metallic",.68f);paint.SetFloat("_Glossiness",.82f);
    foreach(var renderer in visual.GetComponentsInChildren<Renderer>()){
     var materials=renderer.sharedMaterials;
     for(int m=0;m<materials.Length;m++)if(materials[m].name=="R8 body_color" || materials[m].name=="R8 FrontColor")materials[m]=paint;
     renderer.sharedMaterials=materials;renderer.enabled=true;
    }
    var tyres=visual.GetComponentsInChildren<Renderer>().Where(r=>r.sharedMaterial!=null && r.sharedMaterial.name=="R8 tyre").ToArray();
    if(tyres.Length!=4)throw new Exception("Traffic Audi requires four tyres: "+names[index]);
    visual.transform.position+=Vector3.up*(motion.surfaceHeight-tyres.Min(r=>r.bounds.min.y));
    motion.wheels=Enumerable.Range(0,4).Select(i=>visual.transform.Find("Wheel "+i)).ToArray();
    motion.radii=new float[4];
    for(int wheelIndex=0;wheelIndex<4;wheelIndex++){
     var tyre=motion.wheels[wheelIndex].GetComponentsInChildren<Renderer>().First(r=>r.sharedMaterial.name=="R8 tyre");
     motion.wheels[wheelIndex].position+=Vector3.up*(motion.surfaceHeight-tyre.bounds.min.y);
     motion.radii[wheelIndex]=tyre.bounds.extents.y;
    }
   }
  }
 }
}
