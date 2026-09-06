using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace FahrCheck.Editor
{
    public static partial class PrototypeBuilder
    {
        static void SculptedBody(Transform root, Material paint)
        {
            var points = new[] { new Vector2(-2.1f,.42f),new Vector2(2.1f,.42f),new Vector2(2.1f,.7f),new Vector2(1.75f,.92f),new Vector2(.65f,1.02f),new Vector2(-1.3f,1.02f),new Vector2(-2.1f,.85f) };
            var vertices = new List<Vector3>(); var triangles=new List<int>();
            for(int side=0;side<2;side++) foreach(var p in points) vertices.Add(new Vector3(side==0?-.86f:.86f,p.y,p.x));
            int n=points.Length;
            for(int i=1;i<n-1;i++) { triangles.AddRange(new[]{0,i+1,i,n,n+i,n+i+1}); }
            for(int i=0;i<n;i++) { int j=(i+1)%n; triangles.AddRange(new[]{i,j,j+n,i,j+n,i+n}); }
            // Profile is traversed clockwise when seen from the outside: reverse the generated faces.
            for(int i=0;i<triangles.Count;i+=3) { int swap=triangles[i+1]; triangles[i+1]=triangles[i+2]; triangles[i+2]=swap; }
            var path="Assets/FahrCheck/Generated/Materials/CarBody.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null) { mesh=new Mesh(); AssetDatabase.CreateAsset(mesh,path); } else mesh.Clear();
            mesh.name="Original sculpted car body"; mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
            var body=new GameObject("Sculpted body"); body.transform.SetParent(root,false); body.AddComponent<MeshFilter>().sharedMesh=mesh; body.AddComponent<MeshRenderer>().sharedMaterial=paint;
            paint.SetFloat("_Metallic",.45f); paint.SetFloat("_Glossiness",.7f);
            var glass=Glass();
            var windshield=Box("Windscreen",root,new Vector3(0,1.31f,.71f),new Vector3(1.43f,.66f,.035f),glass);
            windshield.transform.localRotation=Quaternion.Euler(24,0,0);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Side glazing",root,new Vector3(side*.765f,1.3f,-.12f),new Vector3(.025f,.52f,1.5f),glass);
                Box("Door belt trim",root,new Vector3(side*.874f,.97f,-.12f),new Vector3(.025f,.045f,2.4f),Mat("Chrome trim",new Color(.48f,.52f,.55f)));
                Box("Door handle",root,new Vector3(side*.89f,.88f,-.2f),new Vector3(.06f,.05f,.22f),Mat("Chrome trim",Color.gray));
                Box("Wing mirror",root,new Vector3(side*1.0f,1.17f,.58f),new Vector3(.26f,.14f,.3f),paint);
                var light=new GameObject("Dipped headlight").AddComponent<Light>(); light.transform.SetParent(root,false); light.transform.localPosition=new Vector3(side*.56f,.79f,2.16f); light.transform.localRotation=Quaternion.Euler(7,0,0); light.type=LightType.Spot; light.range=45; light.spotAngle=58; light.intensity=2; light.color=new Color(1,.94f,.8f); light.shadows=LightShadows.None;
            }
            Box("Front grille",root,new Vector3(0,.57f,2.13f),new Vector3(.65f,.15f,.03f),Mat("Car trim",new Color(.065f,.09f,.11f)));
            Box("Rear plate",root,new Vector3(0,.67f,-2.13f),new Vector3(.52f,.12f,.025f),Mat("Plate white",new Color(.9f,.91f,.87f)));
        }
        static Material Glass()
        {
            var m=Mat("Transparent glass",new Color(.3f,.48f,.55f,.18f));
            m.SetFloat("_Mode",3); m.SetInt("_SrcBlend",(int)BlendMode.One); m.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha); m.SetInt("_ZWrite",0); m.EnableKeyword("_ALPHAPREMULTIPLY_ON"); m.renderQueue=3000; m.SetFloat("_Glossiness",.85f); return m;
        }
        static Transform TrafficModel(string name, Vector3 position, Color color, bool parked)
        {
            var root=new GameObject(name).transform; root.SetParent(world,false); root.position=new Vector3(position.x,.2f,position.z);
            var paint=Mat(name+" paint",color); SculptedBody(root,paint);
            Box("Roof",root,new Vector3(0,1.6f,-.18f),new Vector3(1.5f,.09f,1.75f),paint);
            for(int i=0;i<4;i++)
            {
                var tyre=Shape("Tyre",PrimitiveType.Cylinder,root,new Vector3(i%2==0?-.87f:.87f,.14f,i<2?1.3f:-1.3f),new Vector3(.66f,.13f,.66f),Mat("Traffic rubber",new Color(.04f,.045f,.05f)));
                tyre.transform.localRotation=Quaternion.Euler(0,0,90);
            }
            foreach(var lamp in root.GetComponentsInChildren<Light>()) lamp.enabled=!parked;
            return root;
        }
        static void UpgradeVisuals(CourseSession session)
        {
            var asphalt=Materials["Asphalt"];
            string path="Assets/FahrCheck/Generated/Materials/AsphaltGrain.asset";
            var grain=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(grain==null) { grain=new Texture2D(128,128,TextureFormat.RGB24,true); grain.name="Original asphalt grain"; AssetDatabase.CreateAsset(grain,path); }
            var rng=new System.Random(42); var colors=new Color[128*128];
            for(int i=0;i<colors.Length;i++) { float value=.8f+(float)rng.NextDouble()*.2f; colors[i]=new Color(value,value,value); }
            grain.SetPixels(colors); grain.Apply(); grain.wrapMode=TextureWrapMode.Repeat; EditorUtility.SetDirty(grain);
            asphalt.mainTexture=grain; asphalt.mainTextureScale=new Vector2(5,90); asphalt.SetFloat("_Glossiness",.22f);
            // Add curb joints, bollards and layered tree foliage without purchased assets.
            for(int side=-1;side<=1;side+=2)
            for(int z=5;z<365;z+=12)
            {
                if(Mathf.Abs(z-136)<18 || Mathf.Abs(z-190)<14 || Mathf.Abs(z-290)<14) continue;
                Box("Curb joint",world,new Vector3(side*6.1f,.14f,z),new Vector3(.18f,.08f,.05f),Mat("Curb joint",new Color(.4f,.43f,.44f)));

            }
            var trees = world.GetComponentsInChildren<Transform>();
            foreach(var tree in trees) if(tree.name == "Linden tree")
            {
                Shape("Layered canopy",PrimitiveType.Sphere,tree,new Vector3(1,4.4f,.5f),new Vector3(2.7f,3.1f,2.8f),Mat("Leaf highlights",new Color(.34f,.48f,.24f)));
                Shape("Layered canopy",PrimitiveType.Sphere,tree,new Vector3(-.8f,4.8f,-.5f),new Vector3(2.6f,2.8f,2.7f),Mat("Leaf shade",new Color(.24f,.38f,.2f)));
            }
            var traffic=new GameObject("Neighbourhood traffic").AddComponent<AmbientTraffic>(); traffic.session=session;
            traffic.positions=new[]{340f,310f,110f,65f}; traffic.cars=new Transform[4];
            Color[] paints={new Color(.65f,.12f,.1f),new Color(.75f,.78f,.8f),new Color(.1f,.18f,.24f),new Color(.68f,.55f,.28f)};
            for(int i=0;i<4;i++) { traffic.cars[i]=TrafficModel("Oncoming car "+i,new Vector3(-3,.65f,traffic.positions[i]),paints[i],false); traffic.cars[i].rotation=Quaternion.Euler(0,180,0); }
            for(int i=0;i<6;i++) { var parked=TrafficModel("Parked car "+i,new Vector3(i%2==0?12:-12,.65f,25+i*40),paints[i%4],true); parked.rotation=Quaternion.Euler(0,i%2==0?0:180,0); }
            // Replace the priority actor's placeholder with the same detailed car model.
            var old=session.encounters.priorityCar; var actor=TrafficModel("Priority car",old.position,new Color(.83f,.31f,.15f),false); actor.rotation=old.rotation; session.encounters.priorityCar=actor; Object.DestroyImmediate(old.gameObject);
            var weather=new GameObject("Dynamic weather").AddComponent<WeatherController>(); weather.session=session; weather.sun=RenderSettings.sun; weather.asphalt=asphalt; weather.headlights=session.car.GetComponentsInChildren<Light>();
            var rain=new GameObject("Local rain").AddComponent<ParticleSystem>(); weather.rain=rain;
            var main=rain.main; main.startLifetime=1.4f; main.startSpeed=0; main.startSize=.035f; main.maxParticles=1600; main.simulationSpace=ParticleSystemSimulationSpace.World; main.startColor=new Color(.7f,.8f,.9f,.45f);
            var emission=rain.emission; emission.rateOverTime=0; var shape=rain.shape; shape.shapeType=ParticleSystemShapeType.Box; shape.scale=new Vector3(30,.1f,36);
            var velocity=rain.velocityOverLifetime; velocity.enabled=true; velocity.space=ParticleSystemSimulationSpace.World; velocity.y=-14; velocity.x=-1;
            var renderer=rain.GetComponent<ParticleSystemRenderer>(); renderer.renderMode=ParticleSystemRenderMode.Stretch; renderer.lengthScale=5; renderer.velocityScale=.06f;
            var rainMaterial=Mat("Rain drops",new Color(.65f,.75f,.85f,.35f)); rainMaterial.shader=Shader.Find("Particles/Standard Unlit"); rainMaterial.SetFloat("_Mode",2); rainMaterial.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha); rainMaterial.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha); rainMaterial.SetInt("_ZWrite",0); rainMaterial.EnableKeyword("_ALPHABLEND_ON"); rainMaterial.renderQueue=3000; renderer.sharedMaterial=rainMaterial;
            QualitySettings.pixelLightCount=4; QualitySettings.shadowResolution=ShadowResolution.High; QualitySettings.shadowCascades=2;
        }
    }
}
