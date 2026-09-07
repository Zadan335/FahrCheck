using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object=UnityEngine.Object;
namespace FahrCheck.Editor {
 public static partial class PrototypeBuilder {
  const string RealPath="Assets/FahrCheck/Generated/Realism";
  static void RealismPass(CourseSession session){
   if(!AssetDatabase.IsValidFolder(RealPath))AssetDatabase.CreateFolder("Assets/FahrCheck/Generated","Realism");
   SurfaceMaterials();UpgradeCars(session);UpgradePedestrian(session);FacadeKit();LeafyTrees();
   QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=100;
   RenderSettings.sun.color=new Color(1,.96f,.91f);RenderSettings.sun.intensity=1.05f;
  }
  static Material Surface(string name,string slug){
   var m=Mat(name,Color.white);string p="Assets/ThirdParty/PolyHaven/"+slug;
   m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(p+"_Diffuse.jpg");
   m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(p+"_nor_gl.jpg"));m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.6f);m.SetFloat("_Glossiness",.16f);m.mainTextureScale=Vector2.one;EditorUtility.SetDirty(m);return m;
  }
  static void MetricUv(GameObject go,float tile){
   var f=go.GetComponent<MeshFilter>();if(f==null)return;var src=f.sharedMesh;
   string id=go.transform.localScale.ToString("F3").Replace(" ","").Replace("(","").Replace(")","")+"-"+tile;
   string path=RealPath+"/MetricCube-"+id+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
   if(mesh==null){mesh=Object.Instantiate(src);var vertices=mesh.vertices;var normals=mesh.normals;var uv=new Vector2[vertices.Length];
    for(int i=0;i<uv.Length;i++){var v=Vector3.Scale(vertices[i],go.transform.localScale);var n=normals[i];uv[i]=(Mathf.Abs(n.y)>.5f?new Vector2(v.x,v.z):Mathf.Abs(n.x)>.5f?new Vector2(v.z,v.y):new Vector2(v.x,v.y))/tile;}
    mesh.uv=uv;mesh.RecalculateTangents();AssetDatabase.CreateAsset(mesh,path);
   }f.sharedMesh=mesh;
  }
  static void SurfaceMaterials(){
   var asphalt=Surface("Asphalt","asphalt_01");var paving=Surface("Pavement","brick_pavement");var brick=Surface("Real brick facade","red_brick_03");
   foreach(var r in world.GetComponentsInChildren<MeshRenderer>()){
    if(r.sharedMaterial==asphalt || r.sharedMaterial==paving)MetricUv(r.gameObject,r.sharedMaterial==asphalt?3:2);
    if(r.name=="Building") {r.sharedMaterial=brick;MetricUv(r.gameObject,3);}
   }
  }
  static void UpgradeCars(CourseSession session){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/FreeCoupeCar/Car.prefab");
   if(prefab==null)throw new Exception("CC0 coupe prefab is missing");
   foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/ThirdParty/FreeCoupeCar"})){
    var material=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));material.shader=Shader.Find("Standard");material.renderQueue=-1;material.shaderKeywords=new string[0];material.SetFloat("_Glossiness",.45f);material.SetFloat("_Metallic",.3f);EditorUtility.SetDirty(material);
   }
   var actors=Object.FindObjectsOfType<CollisionParticipant>().Where(a=>!a.pedestrian).Select(a=>a.transform).ToList();actors.Add(session.car.transform);
   foreach(var root in actors){
    bool player=root==session.car.transform;
    foreach(var r in root.GetComponentsInChildren<Renderer>()){
     if(player && (session.car.brakeLamps.Contains(r)||session.car.leftIndicators.Contains(r)||session.car.rightIndicators.Contains(r)||r.name=="Dashboard"))continue;
     r.enabled=false;
    }
    var model=(GameObject)PrefabUtility.InstantiatePrefab(prefab);if(player)PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);model.name="Imported coupe";model.transform.SetParent(root,false);model.transform.localRotation=Quaternion.Euler(0,180,0);model.transform.localScale=new Vector3(15.5f,19,19);model.transform.localPosition=new Vector3(0,.81f,0);
    foreach(var c in model.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
    foreach(var b in model.GetComponentsInChildren<Rigidbody>())Object.DestroyImmediate(b);
    if(player){
     session.car.lampOff.color=new Color(.045f,.025f,.018f);
     foreach(var lamp in session.car.brakeLamps.Concat(session.car.leftIndicators).Concat(session.car.rightIndicators))lamp.transform.localScale*=.6f;
     string[] names={"WheelFrontL","WheelFrontR","WheelRearL","WheelReartR"};
     for(int i=0;i<4;i++){
      var wheel=model.GetComponentsInChildren<Transform>().First(t=>t.name==names[i]);var renderer=wheel.GetComponent<Renderer>();Vector3 center=renderer.bounds.center;
      var pivot=new GameObject("Imported wheel alignment").transform;pivot.SetParent(session.car.wheelVisuals[i],false);
      wheel.SetParent(pivot,true);wheel.position+=pivot.position-center;
      // Existing wheel pivots retain WheelCollider pose updates and steering.
     }
    }
   }
  }
  static void UpgradePedestrian(CourseSession session){
   var root=session.encounters.pedestrian;
   foreach(var t in root.Cast<Transform>().ToArray())Object.DestroyImmediate(t.gameObject);
   Object.DestroyImmediate(root.GetComponent<PedestrianAnimator>());
   var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Quaternius/Superhero_Male_FullBody.fbx");
   var model=Object.Instantiate(source,root);model.name="Clothed civilian";model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.Euler(0,180,0);model.transform.localScale=new Vector3(.86f,.97f,.97f);
   var skin=Mat("Civilian skin",Color.white);skin.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Quaternius/T_Superhero_Male_Ligh.png");skin.SetFloat("_Glossiness",.22f);
   var cloth=Mat("Civilian navy knit",new Color(.13f,.19f,.23f));cloth.SetFloat("_Glossiness",.08f);
   var trousers=Mat("Civilian charcoal trousers",new Color(.16f,.16f,.15f));trousers.SetFloat("_Glossiness",.08f);
   var leather=Mat("Civilian leather shoes",new Color(.075f,.045f,.028f));leather.SetFloat("_Glossiness",.28f);
   var hair=Mat("Civilian hair",new Color(.15f,.095f,.055f));
   foreach(var r in model.GetComponentsInChildren<SkinnedMeshRenderer>()){
    if(r.name=="SuperHero_Male"){
     var mesh=Object.Instantiate(r.sharedMesh);mesh.name="CivilianClothing";var weights=mesh.boneWeights;var triangles=mesh.triangles;var groups=new List<int>[4];for(int i=0;i<4;i++)groups[i]=new List<int>();
     for(int i=0;i<triangles.Length;i+=3){
      string bone=r.bones[weights[triangles[i]].boneIndex0].name.ToLowerInvariant();
      int group=bone.Contains("head")||bone.Contains("neck")||bone.Contains("hand")||bone.Contains("index")||bone.Contains("thumb")||bone.Contains("middle")||bone.Contains("ring")||bone.Contains("pinky")?0:bone.Contains("foot")||bone.Contains("ball")?3:bone.Contains("thigh")||bone.Contains("calf")||bone.Contains("pelvis")?2:1;
      groups[group].AddRange(new[]{triangles[i],triangles[i+1],triangles[i+2]});
     }
     mesh.subMeshCount=4;for(int i=0;i<4;i++)mesh.SetTriangles(groups[i],i);
     string path=RealPath+"/CivilianClothing.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(saved==null){AssetDatabase.CreateAsset(mesh,path);saved=mesh;}else {EditorUtility.CopySerialized(mesh,saved);Object.DestroyImmediate(mesh);}r.sharedMesh=saved;r.sharedMaterials=new[]{skin,cloth,trousers,leather};
    }else if(r.name=="Eyes") {var eyes=Mat("Civilian eyes",Color.white);eyes.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Quaternius/T_Eye_Brown.png");r.sharedMaterial=eyes;}
    else r.sharedMaterial=hair;
   }
   var animator=model.GetComponent<Animator>();if(animator==null)animator=model.AddComponent<Animator>();
   if(animator.avatar==null)animator.avatar=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(source)).OfType<Avatar>().First();
   animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
   string animationPath="Assets/ThirdParty/Quaternius/UAL1_Standard.fbx";
   var importer=(ModelImporter)AssetImporter.GetAtPath(animationPath);
   var importedClips=importer.clipAnimations.Length>0?importer.clipAnimations:importer.defaultClipAnimations;
   bool updateLoops=false;foreach(var clip in importedClips)if(clip.name.EndsWith("_Loop") && !clip.loopTime){clip.loopTime=true;updateLoops=true;}
   if(updateLoops){importer.clipAnimations=importedClips;importer.SaveAndReimport();}
   var clips=AssetDatabase.LoadAllAssetsAtPath(animationPath).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
   var idle=clips.First(c=>c.name.EndsWith("|Idle_Loop"));var walk=clips.First(c=>c.name.EndsWith("|Walk_Loop"));
   foreach(var clip in new[]{idle,walk}){var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);}
   string cp=RealPath+"/Civilian.controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(cp);
   if(controller==null){controller=AnimatorController.CreateAnimatorControllerAtPath(cp);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);var state=controller.layers[0].stateMachine.AddState("Locomotion");var tree=new BlendTree{name="Idle to walking",blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(tree,controller);tree.AddChild(idle,0);tree.AddChild(walk,1.15f);state.motion=tree;controller.layers[0].stateMachine.defaultState=state;}
   animator.runtimeAnimatorController=controller;
   var hairSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Quaternius/Hair_SimpleParted.fbx");
   var hairModel=Object.Instantiate(hairSource,model.transform);hairModel.name="Parted hairstyle";hairModel.transform.localPosition=Vector3.zero;hairModel.transform.localRotation=Quaternion.identity;
   var skeleton=model.GetComponentsInChildren<Transform>().Where(t=>!t.IsChildOf(hairModel.transform)).GroupBy(t=>t.name).ToDictionary(g=>g.Key,g=>g.First());
   foreach(var renderer in hairModel.GetComponentsInChildren<SkinnedMeshRenderer>()){
    renderer.bones=renderer.bones.Select(b=>skeleton[b.name]).ToArray();renderer.rootBone=skeleton[renderer.rootBone.name];renderer.sharedMaterial=hair;
   }
   var unusedAnimator=hairModel.GetComponent<Animator>();if(unusedAnimator!=null)Object.DestroyImmediate(unusedAnimator);
   var motion=root.gameObject.AddComponent<SkinnedPedestrianMotion>();motion.animator=animator;
   // Preview a valid idle pose in saved scene renders; Play Mode uses the controller.
   idle.SampleAnimation(model,0);
   PrefabUtility.SaveAsPrefabAsset(root.gameObject,RealPath+"/Civilian.prefab");
  }
  static void FacadeKit(){
   var stone=Mat("Limestone frames",new Color(.65f,.63f,.57f));var metal=Mat("Zinc gutters",new Color(.28f,.3f,.31f));var glass=Mat("Recessed window glass",new Color(.09f,.13f,.16f));glass.SetFloat("_Glossiness",.8f);glass.SetFloat("_Metallic",.25f);
   var kit=new GameObject("European window module").transform;
   Box("Recess",kit,Vector3.zero,new Vector3(.12f,1.6f,1.5f),Mat("Window recess",new Color(.12f,.12f,.11f)));
   Box("Glass",kit,new Vector3(-.08f,0,0),new Vector3(.035f,1.35f,1.25f),glass);
   foreach(int side in new[]{-1,1}){Box("Jamb",kit,new Vector3(-.13f,0,side*.72f),new Vector3(.16f,1.6f,.1f),stone);Box("Lintel",kit,new Vector3(-.13f,side*.75f,0),new Vector3(.18f,.1f,1.5f),stone);}
   Box("Mullion",kit,new Vector3(-.14f,0,0),new Vector3(.08f,1.4f,.05f),stone);Box("Transom",kit,new Vector3(-.14f,.22f,0),new Vector3(.08f,.05f,1.4f),stone);Box("Projecting sill",kit,new Vector3(-.24f,-.82f,0),new Vector3(.4f,.12f,1.65f),stone);
   var windowPrefab=PrefabUtility.SaveAsPrefabAsset(kit.gameObject,RealPath+"/EuropeanWindow.prefab");Object.DestroyImmediate(kit.gameObject);
   foreach(var t in world.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Townhouse")).ToArray()){
    var wall=t.Find("Building");float width=wall.localScale.x,height=wall.localScale.y;int facing=t.position.x>0?1:-1;
    foreach(var w in t.GetComponentsInChildren<Transform>().Where(w=>w.name=="Window").ToArray()){
     var module=(GameObject)PrefabUtility.InstantiatePrefab(windowPrefab);module.transform.SetParent(t,false);module.transform.localPosition=w.localPosition;module.transform.localRotation=Quaternion.Euler(0,facing>0?0:180,0);Object.DestroyImmediate(w.gameObject);
    }
    for(int side=-1;side<=1;side+=2){
     Box("Rain gutter",t,new Vector3(side*(width/2+.15f),height+.1f,0),new Vector3(.15f,.14f,14.6f),metal);
     Box("Downpipe",t,new Vector3(-facing*(width/2+.18f),height/2,side*6.65f),new Vector3(.1f,height,.1f),metal);
     // Close pitched-roof gables with graduated brick courses.
     for(int i=0;i<12;i++){float y=i*.12f;var block=Box("Gable brick",t,new Vector3(0,height+y,side*6.95f),new Vector3(Mathf.Max(.2f,width*(1-i/12f)),.13f,.12f),Materials["Real brick facade"]);MetricUv(block,3);}
    }
    var entry=t.Find("Entrance");if(entry!=null){entry.GetComponent<Renderer>().sharedMaterial=Mat("Oak entrance",new Color(.18f,.11f,.06f));Box("Door surround",t,entry.localPosition+Vector3.up*1.32f,new Vector3(.2f,.14f,1.85f),stone);Box("Door handle",t,entry.localPosition+new Vector3(-facing*.1f,0,.45f),new Vector3(.06f,.28f,.04f),metal);}
   }
  }
 }
}
