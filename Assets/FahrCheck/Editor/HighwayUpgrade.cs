using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace FahrCheck.Editor
{
 public static partial class PrototypeBuilder
 {
  static void HighwayPass(CourseSession session)
  {
   var highway=new GameObject("AUTOBAHN / 2 km generated level").transform;highway.SetParent(world,false);session.highwayWorld=highway.gameObject;
   var asphalt=Materials["Asphalt"];var white=Materials["Markings"];
   var grass=Mat("Autobahn landscape",new Color(.25f,.38f,.22f));var concrete=Mat("Autobahn barrier",new Color(.48f,.5f,.49f));
   Box("Highway terrain",highway,new Vector3(300,-.35f,1000),new Vector3(180,.6f,2150),grass,true);
   Box("Northbound carriageway",highway,new Vector3(308,-.02f,1000),new Vector3(13,.1f,2050),asphalt,true);
   Box("Southbound carriageway",highway,new Vector3(292,-.02f,1000),new Vector3(13,.1f,2050),asphalt,true);
   Box("Median barrier",highway,new Vector3(300,.48f,1000),new Vector3(.55f,.96f,2050),concrete,true);
   foreach(float x in new[]{302f,314f,286f,298f})Box("Autobahn edge line",highway,new Vector3(x,.04f,1000),new Vector3(.14f,.015f,2050),white);
   foreach(float x in new[]{306f,310f,290f,294f})for(float z=5;z<2045;z+=12)Box("Autobahn lane dash",highway,new Vector3(x,.045f,z),new Vector3(.14f,.015f,6),white);
   for(float z=20;z<2020;z+=50)foreach(float x in new[]{283f,317f}){Box("Reflector post",highway,new Vector3(x,.55f,z),new Vector3(.12f,1.1f,.12f),white);Box("Post reflector",highway,new Vector3(x,.82f,z),new Vector3(.15f,.16f,.08f),Mat("Reflector",Color.white,true));}
   CreateAutobahnLandscape(highway);
   Box("Autobahn sign gantry",highway,new Vector3(308,5.3f,35),new Vector3(15,.25f,.25f),concrete);
   foreach(float x in new[]{301f,315f})Box("Gantry support",highway,new Vector3(x,2.65f,35),new Vector3(.24f,5.3f,.24f),concrete);
   var sign=Box("Autobahn start sign",highway,new Vector3(308,4.3f,34.8f),new Vector3(3.2f,1.55f,.12f),Mat("Autobahn blue",new Color(.02f,.16f,.5f)));
   Label("AUTOBAHN\nA 8   2 km\n130 ADVISORY",sign.transform,new Vector3(0,0,-.08f),.055f,Color.white);
   var trafficRoot=new GameObject("Dense Autobahn traffic");trafficRoot.transform.SetParent(highway,false);var traffic=trafficRoot.AddComponent<HighwayTraffic>();traffic.session=session;session.highwayTraffic=traffic;
   var cars=new List<Transform>();var positions=new List<float>();var lanes=new List<float>();var speeds=new List<float>();var directions=new List<int>();
   Color[] colours={new Color(.1f,.18f,.3f),new Color(.65f,.08f,.05f),new Color(.72f,.74f,.75f),new Color(.08f,.09f,.1f),new Color(.3f,.42f,.18f)};
   int index=0;
   foreach(float lane in new[]{304f,308f,312f})for(int j=0;j<(lane==312?6:5);j++){
    float z=90+j*330+(lane-304)*11;var car=TrafficModel("Highway car "+index,new Vector3(lane,.2f,z),colours[index%colours.Length],false);car.SetParent(trafficRoot.transform,true);cars.Add(car);positions.Add(z);lanes.Add(lane);speeds.Add(lane==304?34:lane==308?28:22);directions.Add(1);index++;
   }
   foreach(float lane in new[]{288f,292f,296f})for(int j=0;j<3;j++){
    float z=180+j*650+(lane-288)*19;var car=TrafficModel("Highway car "+index,new Vector3(lane,.2f,z),colours[index%colours.Length],false);car.rotation=Quaternion.Euler(0,180,0);car.SetParent(trafficRoot.transform,true);cars.Add(car);positions.Add(z);lanes.Add(lane);speeds.Add(24+(lane-288));directions.Add(-1);index++;
   }
   traffic.cars=cars.ToArray();traffic.positions=positions.ToArray();traffic.laneX=lanes.ToArray();traffic.cruiseSpeeds=speeds.ToArray();traffic.directions=directions.ToArray();
  }
  static void CreateAutobahnLandscape(Transform highway)
  {
   EnsureFolder("Assets/FahrCheck/Generated/Realism");
   var meadow=TerrainTexture("Alpine meadow",new Color(.18f,.34f,.12f),.105f);
   var rock=TerrainTexture("Weathered mountain rock",new Color(.43f,.39f,.34f),.19f);
   var snow=TerrainTexture("Distant mountain snow",new Color(.84f,.86f,.87f),.08f);
   var material=Mat("Layered Alpine mountain",Color.white);var shader=Shader.Find("FahrCheck/Mountain Terrain");
   if(shader==null)throw new System.InvalidOperationException("FahrCheck mountain shader was not imported.");
   material.shader=shader;material.SetTexture("_GrassTex",meadow);material.SetTexture("_RockTex",rock);material.SetTexture("_SnowTex",snow);EditorUtility.SetDirty(material);
   CreateTerrainSide(highway,-1,135,material,13.7f);
   CreateTerrainSide(highway,1,317,material,41.3f);
  }
  static void CreateTerrainSide(Transform parent,int side,float startX,Material material,float seed)
  {
   const int across=129,along=257;const float width=148,length=2100;
   string suffix=side<0?"Left":"Right";
   var vertices=new Vector3[across*along];var uv=new Vector2[vertices.Length];var colours=new Color[vertices.Length];var triangles=new int[(across-1)*(along-1)*6];int ti=0;
   for(int z=0;z<along;z++)for(int x=0;x<across;x++){
    float u=x/(across-1f),v=z/(along-1f),metres=MountainHeight(u,v,side,seed,length),du=1f/(across-1),dv=1f/(along-1);
    float dx=(MountainHeight(Mathf.Clamp01(u+du),v,side,seed,length)-MountainHeight(Mathf.Clamp01(u-du),v,side,seed,length))/(2*du*width);
    float dz=(MountainHeight(u,Mathf.Clamp01(v+dv),side,seed,length)-MountainHeight(u,Mathf.Clamp01(v-dv),side,seed,length))/(2*dv*length);
    float slope=Mathf.Atan(Mathf.Sqrt(dx*dx+dz*dz))*Mathf.Rad2Deg;
    float snow=Mathf.Clamp01(Mathf.InverseLerp(38,57,metres))*Mathf.Clamp01(1-slope/72f);
    float rock=Mathf.Clamp01(Mathf.InverseLerp(10,32,slope))*.92f+Mathf.Clamp01(Mathf.InverseLerp(25,54,metres))*.42f;
    rock=Mathf.Clamp01(rock)*(1-snow*.65f);float meadow=Mathf.Max(.06f,1-rock-snow),total=meadow+rock+snow;int index=z*across+x;
    vertices[index]=new Vector3(u*width,metres,v*length);uv[index]=new Vector2(u*width/13f,v*length/13f);colours[index]=new Color(meadow/total,rock/total,snow/total,1);
   }
   for(int z=0;z<along-1;z++)for(int x=0;x<across-1;x++){int a=z*across+x,b=a+1,c=a+across,d=c+1;triangles[ti++]=a;triangles[ti++]=c;triangles[ti++]=b;triangles[ti++]=b;triangles[ti++]=c;triangles[ti++]=d;}
   var generated=new Mesh{name="Runtime-safe layered Autobahn mountains "+suffix,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};generated.vertices=vertices;generated.uv=uv;generated.colors=colours;generated.triangles=triangles;generated.RecalculateNormals();generated.RecalculateTangents();generated.RecalculateBounds();
   string path="Assets/FahrCheck/Generated/Realism/AutobahnMountainMesh"+suffix+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
   if(mesh==null){AssetDatabase.CreateAsset(generated,path);mesh=generated;}else{EditorUtility.CopySerialized(generated,mesh);Object.DestroyImmediate(generated);EditorUtility.SetDirty(mesh);}
   var go=new GameObject("Continuous realistic mountain mesh "+suffix);go.transform.SetParent(parent,false);go.transform.localPosition=new Vector3(startX,-.2f,-30);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;renderer.receiveShadows=true;
   CreateFoothillForest(go.transform,side,seed,suffix,width,length);
  }
  static float MountainHeight(float u,float v,int side,float seed,float length)
  {
   float outward=side<0?1-u:u,rise=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.13f,.72f,outward));
   float broad=Mathf.PerlinNoise(seed+v*3.8f,seed*.27f+u*.75f);
   float ridge=Mathf.Pow(1-Mathf.Abs(Mathf.PerlinNoise(seed*.43f+v*9.4f,seed+u*2.1f)*2-1),1.7f),profile=0,routeMetres=v*length;
   for(int peak=0;peak<13;peak++){float centre=105+peak*163+Mathf.PerlinNoise(seed,peak*.61f)*65,spread=72+Mathf.PerlinNoise(peak*.37f,seed)*76,distance=(routeMetres-centre)/spread;profile=Mathf.Max(profile,(.52f+Mathf.PerlinNoise(seed+peak*.73f,2.4f)*.48f)*Mathf.Exp(-distance*distance*.5f));}
   float erosion=Mathf.PerlinNoise(seed*1.8f+u*7.3f,v*23f+seed*.12f),ravines=Mathf.Pow(1-Mathf.Abs(Mathf.PerlinNoise(seed+u*13f,v*31f+seed*.18f)*2-1),2.2f);
   return rise*(4+46*profile+12*broad+10*ridge*profile+5*erosion+9*ravines*profile);
  }
  static Texture2D TerrainTexture(string name,Color colour,float frequency)
  {
   string texturePath="Assets/FahrCheck/Generated/Realism/"+name+" Texture.asset";
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
   if(texture==null){texture=new Texture2D(96,96,TextureFormat.RGB24,true){name=name+" procedural texture"};AssetDatabase.CreateAsset(texture,texturePath);}
   var pixels=new Color[texture.width*texture.height];float offset=name.Length*1.73f;
   for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++){
    float n=Mathf.PerlinNoise(offset+x*frequency,offset*.37f+y*frequency);
    float grain=Mathf.PerlinNoise(offset*2+x*frequency*3.7f,y*frequency*3.7f);
    float value=.70f+n*.36f+grain*.10f;pixels[y*texture.width+x]=new Color(Mathf.Clamp01(colour.r*value),Mathf.Clamp01(colour.g*value),Mathf.Clamp01(colour.b*value),1);
   }
   texture.wrapMode=TextureWrapMode.Repeat;texture.filterMode=FilterMode.Trilinear;texture.anisoLevel=4;texture.SetPixels(pixels);texture.Apply(true,false);EditorUtility.SetDirty(texture);
   return texture;
  }
  static void CreateFoothillForest(Transform parent,int side,float seed,string suffix,float width,float length)
  {
   var vertices=new List<Vector3>();var triangles=new List<int>();var random=new System.Random((int)(seed*100));const int sides=7;
   for(int tree=0;tree<420;tree++){
    float outward=.28f+(float)random.NextDouble()*.66f;
    float x=side<0?(1-outward)*width:outward*width;
    float z=18+(float)random.NextDouble()*(length-36);float y=MountainHeight(x/width,z/length,side,seed,length);
    float treeHeight=2.8f+(float)random.NextDouble()*4.8f;float radius=treeHeight*(.17f+(float)random.NextDouble()*.07f);
    for(int tier=0;tier<2;tier++){
     float baseY=y+treeHeight*(.08f+tier*.27f),topY=y+treeHeight*(.72f+tier*.28f),tierRadius=radius*(1-tier*.42f),lean=((tree%5)-2)*.025f;
     int root=vertices.Count;for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;vertices.Add(new Vector3(x+Mathf.Cos(a)*tierRadius,baseY,z+Mathf.Sin(a)*tierRadius));}vertices.Add(new Vector3(x+lean*treeHeight,topY,z));
     for(int i=0;i<sides;i++){int next=root+(i+1)%sides,tip=root+sides;triangles.Add(root+i);triangles.Add(next);triangles.Add(tip);triangles.Add(tip);triangles.Add(next);triangles.Add(root+i);}
    }
   }
   var generated=new Mesh{name="Combined Autobahn foothill forest "+suffix};generated.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;generated.SetVertices(vertices);generated.SetTriangles(triangles,0);generated.RecalculateNormals();generated.RecalculateBounds();
   string path="Assets/FahrCheck/Generated/Realism/AutobahnForest"+suffix+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
   if(mesh==null){AssetDatabase.CreateAsset(generated,path);mesh=generated;}else{EditorUtility.CopySerialized(generated,mesh);Object.DestroyImmediate(generated);EditorUtility.SetDirty(mesh);}
   var forest=new GameObject("Forested foothills "+suffix);forest.transform.SetParent(parent,false);forest.AddComponent<MeshFilter>().sharedMesh=mesh;forest.AddComponent<MeshRenderer>().sharedMaterial=Mat("Alpine forest",new Color(.08f,.21f,.075f));
  }
 }
}
