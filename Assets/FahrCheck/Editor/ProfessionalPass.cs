using UnityEngine;
using UnityEditor;
namespace FahrCheck.Editor
{
    public static partial class PrototypeBuilder
    {
        static Transform Joint(string name,Transform parent,Vector3 local)
        { var t=new GameObject(name).transform; t.SetParent(parent,false); t.localPosition=local; return t; }
        static Transform Limb(string name,Transform parent,Vector3 local,float length,float width,Material material)
        { var pivot=Joint(name,parent,local); Shape(name+" mesh",PrimitiveType.Capsule,pivot,new Vector3(0,-length/2,0),new Vector3(width,length/2,width),material); return pivot; }
        static void BuildHuman(CourseSession session)
        {
            Object.DestroyImmediate(session.encounters.pedestrian.gameObject);
            var human=Joint("Animated pedestrian",world,new Vector3(6.5f,0,245)); human.rotation=Quaternion.Euler(0,270,0);
            var animator=human.gameObject.AddComponent<PedestrianAnimator>();
            var skin=Mat("Human skin",new Color(.63f,.42f,.3f)); var jacket=Mat("Human jacket",new Color(.14f,.32f,.43f)); var pants=Mat("Human trousers",new Color(.11f,.13f,.17f)); var shoes=Mat("Human shoes",new Color(.055f,.045f,.04f));
            animator.hips=Joint("Hips",human,new Vector3(0,.95f,0));
            Shape("Pelvis",PrimitiveType.Sphere,animator.hips,Vector3.zero,new Vector3(.34f,.25f,.23f),pants);
            Shape("Jacket",PrimitiveType.Capsule,animator.hips,new Vector3(0,.22f,0),new Vector3(.4f,.28f,.26f),jacket);
            animator.head=Joint("Head",animator.hips,new Vector3(0,.65f,0));
            Shape("Face",PrimitiveType.Sphere,animator.head,Vector3.zero,new Vector3(.23f,.29f,.23f),skin);
            Shape("Hair",PrimitiveType.Sphere,animator.head,new Vector3(0,.09f,-.015f),new Vector3(.24f,.16f,.235f),Mat("Human hair",new Color(.12f,.07f,.045f)));
            Shape("Nose",PrimitiveType.Sphere,animator.head,new Vector3(0,0,.12f),new Vector3(.05f,.065f,.055f),skin);
            for(int side=-1;side<=1;side+=2) Shape("Eye",PrimitiveType.Sphere,animator.head,new Vector3(side*.052f,.03f,.105f),Vector3.one*.018f,shoes);
            animator.leftThigh=Limb("Left thigh",animator.hips,new Vector3(-.1f,0,0),.43f,.15f,pants);
            animator.rightThigh=Limb("Right thigh",animator.hips,new Vector3(.1f,0,0),.43f,.15f,pants);
            animator.leftShin=Limb("Left shin",animator.leftThigh,new Vector3(0,-.43f,0),.42f,.115f,pants);
            animator.rightShin=Limb("Right shin",animator.rightThigh,new Vector3(0,-.43f,0),.42f,.115f,pants);
            foreach(var shin in new[]{animator.leftShin,animator.rightShin}) Box("Shoe",shin,new Vector3(0,-.42f,.06f),new Vector3(.14f,.1f,.27f),shoes);
            animator.leftArm=Limb("Left arm",animator.hips,new Vector3(-.23f,.4f,0),.28f,.105f,jacket);
            animator.rightArm=Limb("Right arm",animator.hips,new Vector3(.23f,.4f,0),.28f,.105f,jacket);
            animator.leftForearm=Limb("Left forearm",animator.leftArm,new Vector3(0,-.28f,0),.25f,.085f,jacket);
            animator.rightForearm=Limb("Right forearm",animator.rightArm,new Vector3(0,-.28f,0),.25f,.085f,jacket);
            foreach(var arm in new[]{animator.leftForearm,animator.rightForearm}) Shape("Hand",PrimitiveType.Sphere,arm,new Vector3(0,-.27f,0),new Vector3(.085f,.12f,.085f),skin);
            var capsule=human.gameObject.AddComponent<CapsuleCollider>(); capsule.center=new Vector3(0,.88f,0); capsule.height=1.76f; capsule.radius=.25f;
            human.gameObject.AddComponent<CollisionParticipant>().pedestrian=true;
            var body=human.gameObject.AddComponent<Rigidbody>(); body.isKinematic=true; body.interpolation=RigidbodyInterpolation.Interpolate;
            session.encounters.pedestrian=human;
        }
        static void ProfessionalPass(CourseSession session)
        {
            foreach(var gate in world.GetComponentsInChildren<Transform>())
            {
                if(gate.name=="Finish gate") gate.gameObject.AddComponent<MissionGate>().direction=1;
                if(gate.name=="Left route finish gate") gate.gameObject.AddComponent<MissionGate>().direction=-1;
            }
            foreach(var t in world.GetComponentsInChildren<Transform>())
                if(t.name.StartsWith("Oncoming car ") || t.name.StartsWith("Same-direction car ") || t.name.StartsWith("Highway car ") || t.name.StartsWith("Parked car ") || t.name=="Priority car")
                {
                    var collider=t.gameObject.AddComponent<BoxCollider>(); collider.center=new Vector3(0,.8f,0); collider.size=new Vector3(1.8f,1.2f,4.2f);
                    t.gameObject.AddComponent<CollisionParticipant>();
                    var body=t.gameObject.AddComponent<Rigidbody>(); body.isKinematic=true; body.interpolation=RigidbodyInterpolation.Interpolate;
                }
            BuildHuman(session);
            BuildSignBoards();
            AddOpenJunctionDetails();
            AddCornerFuelStations();
            ExpandSideStreetArchitecture();
            var camera=Object.FindObjectOfType<Camera>();
            var grade=Mat("Driving colour grade",Color.white);grade.shader=Shader.Find("Hidden/FahrCheck/DriveGrade");
            camera.gameObject.AddComponent<DrivePresentation>().grade=grade;
            for(int z=20;z<360;z+=46)
            {
                if(Mathf.Abs(z-136)<20 || Mathf.Abs(z-190)<20 || Mathf.Abs(z-290)<20)continue;
                var wood=Mat("Street furniture wood",new Color(.34f,.22f,.13f));var iron=Mat("Street furniture iron",new Color(.1f,.13f,.14f));
                Box("Bench seat",world,new Vector3(8.2f,.48f,z),new Vector3(.7f,.12f,1.8f),wood,true);
                Box("Bench back",world,new Vector3(8.5f,.85f,z),new Vector3(.1f,.7f,1.8f),wood,true);
                for(int side=-1;side<=1;side+=2)Box("Bench leg",world,new Vector3(8.2f,.23f,z+side*.65f),new Vector3(.5f,.46f,.07f),iron);
                Shape("Street bin",PrimitiveType.Cylinder,world,new Vector3(-8,.5f,z+4),new Vector3(.5f,.5f,.5f),Mat("Street bins",new Color(.14f,.24f,.19f)),true);
            }
            // Pitched roofs, window surrounds and shop fronts make a coherent town rather than bare cubes.
            var objects=world.GetComponentsInChildren<Transform>();
            foreach(var t in objects) if(t.name.StartsWith("Townhouse"))
            {
                var building=t.Find("Building"); if(building==null) continue;
                float h=building.localScale.y, w=building.localScale.x;
                var roof=Mat("Slate roof",new Color(.18f,.2f,.22f));
                for(int side=-1;side<=1;side+=2)
                { var panel=Box("Pitched roof",t,new Vector3(side*w*.25f,h+1,0),new Vector3(w*.56f,.17f,15),roof); panel.transform.localRotation=Quaternion.Euler(0,0,side*-22); }
                Box("Chimney",t,new Vector3(w*.2f,h+2,-3),new Vector3(.7f,1.8f,.7f),Mat("Brick chimney",new Color(.42f,.28f,.23f)));
                foreach(var window in t.GetComponentsInChildren<Transform>()) if(window.name=="Window")
                    Box("Window sill",t,window.localPosition+Vector3.down*.67f,new Vector3(.22f,.11f,1.5f),Mat("Stone detailing",new Color(.77f,.75f,.69f)));
            }
            // Remove floating instructional text; guidance lives in the HUD.
            foreach(var text in world.GetComponentsInChildren<TextMesh>()) if(text.text=="30 ahead" || text.text=="ZEBRA CROSSING" || text.text.Contains("LEFT     RIGHT")) Object.DestroyImmediate(text.gameObject);
            RenderSettings.ambientIntensity=.8f; RenderSettings.sun.transform.rotation=Quaternion.Euler(34,-35,0);
        }
        static void AddOpenJunctionDetails()
        {
            var stone=Mat("Junction planter stone",new Color(.42f,.44f,.42f));
            var hedge=Mat("Low junction hedge",new Color(.12f,.29f,.13f));
            var metal=Mat("Junction guard rail",new Color(.34f,.38f,.39f));
            // Low objects fill the long side street while preserving the driver's sight triangle.
            foreach(int side in new[]{-1}) foreach(float x in new[]{18f,27f,36f})
            {
                float px=x*side;
                Box("Low junction planter",world,new Vector3(px,.28f,199),new Vector3(5,.55f,1.1f),stone,true);
                Box("Low junction hedge",world,new Vector3(px,.72f,199),new Vector3(4.5f,.85f,.8f),hedge);
            }
            foreach(int side in new[]{-1,1})
            {
                Box("Open junction rail",world,new Vector3(side*31,.42f,181),new Vector3(18,.08f,.08f),metal);
                for(float x=22;x<=40;x+=6) Box("Open junction rail post",world,new Vector3(side*x,.4f,181),new Vector3(.08f,.8f,.08f),metal);
            }
        }
        static void AddCornerFuelStations()
        {
            var concrete=Mat("Fuel station forecourt",new Color(.43f,.45f,.44f));
            var white=Mat("Fuel station white",new Color(.86f,.87f,.84f));
            var blue=Mat("Fuel station blue",new Color(.035f,.18f,.34f));
            var red=Mat("Fuel station red",new Color(.72f,.035f,.025f));
            var dark=Mat("Fuel station charcoal",new Color(.055f,.065f,.07f));
            var warm=Mat("Fuel station interior light",new Color(.92f,.72f,.38f),true);
            var brick=Mat("Fuel district brick",new Color(.48f,.29f,.22f));
            var glass=Mat("Fuel station glass",new Color(.12f,.25f,.31f));glass.SetFloat("_Glossiness",.78f);
            foreach(float junctionZ in new[]{136f,190f})
            {
                var station=new GameObject("Corner fuel station "+junctionZ).transform;station.SetParent(world,false);
                // Everything starts beyond x=18 and sits north of the side road, keeping its approach visible.
                Box("Forecourt",station,new Vector3(29,.035f,junctionZ+15),new Vector3(27,.07f,16),concrete,true);
                Box("Service shop",station,new Vector3(38,1.65f,junctionZ+18),new Vector3(8,3.3f,8),white,true);
                Box("Shop parapet",station,new Vector3(38,3.42f,junctionZ+18),new Vector3(8.5f,.28f,8.5f),dark);
                Box("Shop fascia",station,new Vector3(33.96f,2.75f,junctionZ+18),new Vector3(.09f,.55f,8.1f),blue);
                for(int pane=-1;pane<=1;pane++)Box("Shop window",station,new Vector3(33.90f,1.35f,junctionZ+18+pane*2.2f),new Vector3(.08f,1.65f,1.65f),glass);
                Box("Shop entrance",station,new Vector3(33.84f,1.15f,junctionZ+21),new Vector3(.12f,2.3f,1.25f),glass,true);
                Label("FAHRCHECK ENERGIE",station,new Vector3(33.82f,2.78f,junctionZ+18),.065f,Color.white);
                Box("Pump canopy",station,new Vector3(25,3.7f,junctionZ+13),new Vector3(13,.35f,7),white);
                Box("Canopy stripe",station,new Vector3(25,3.78f,junctionZ+9.48f),new Vector3(13,.38f,.12f),red);
                Box("Canopy blue fascia",station,new Vector3(18.48f,3.72f,junctionZ+13),new Vector3(.14f,.62f,7.1f),blue);
                Box("Canopy warm lighting",station,new Vector3(25,3.49f,junctionZ+13),new Vector3(10,.03f,4.5f),warm);
                foreach(int side in new[]{-1,1})
                {
                    Box("Canopy column",station,new Vector3(25+side*5.3f,1.85f,junctionZ+13),new Vector3(.22f,3.7f,.22f),white);
                    float pumpX=25+side*2.4f;
                    Box("Pump island",station,new Vector3(pumpX,.09f,junctionZ+13),new Vector3(1.55f,.18f,1.4f),white,true);
                    Box("Fuel pump",station,new Vector3(pumpX,.78f,junctionZ+13),new Vector3(.88f,1.48f,.7f),blue,true);
                    Box("Pump illuminated header",station,new Vector3(pumpX,1.4f,junctionZ+13),new Vector3(.92f,.22f,.73f),red);
                    Box("Pump display",station,new Vector3(pumpX,1.05f,junctionZ+12.64f),new Vector3(.54f,.32f,.03f),glass);
                    Box("Pump keypad",station,new Vector3(pumpX,.72f,junctionZ+12.63f),new Vector3(.4f,.16f,.035f),dark);
                    var hose=Shape("Fuel hose",PrimitiveType.Cylinder,station,new Vector3(pumpX+side*.55f,.75f,junctionZ+13),new Vector3(.045f,.62f,.045f),dark);hose.transform.localRotation=Quaternion.Euler(0,0,8*side);
                    Box("Fuel nozzle",station,new Vector3(pumpX+side*.62f,.48f,junctionZ+12.7f),new Vector3(.1f,.34f,.08f),dark);
                    foreach(int guard in new[]{-1,1})Shape("Pump bollard",PrimitiveType.Cylinder,station,new Vector3(pumpX+guard*.7f,.38f,junctionZ+13),new Vector3(.12f,.38f,.12f),red,true);
                }
                Box("Fuel price pylon",station,new Vector3(17.5f,2.1f,junctionZ+20),new Vector3(.65f,4.2f,.65f),blue);
                Box("Fuel price panel",station,new Vector3(17.5f,3.15f,junctionZ+20),new Vector3(1.8f,1.7f,.18f),white);
                Label("E5\nE10\nDIESEL",station,new Vector3(17.5f,3.15f,junctionZ+19.88f),.055f,new Color(.72f,.035f,.025f));
                // A recessed urban block completes the corner without enclosing the road approach.
                foreach(int buildingIndex in new[]{0,1})
                {
                    float bx=51+buildingIndex*13, height=buildingIndex==0?8.5f:11f;
                    Box("Fuel district building",station,new Vector3(bx,height*.5f,junctionZ+27),new Vector3(11,height,12),buildingIndex==0?brick:white,true);
                    Box("Fuel district roof",station,new Vector3(bx,height+.18f,junctionZ+27),new Vector3(11.5f,.36f,12.5f),dark);
                    for(float floor=1.7f;floor<height-1;floor+=2.4f)for(int bay=-1;bay<=1;bay++)
                        Box("Fuel district window",station,new Vector3(bx+bay*3,floor,junctionZ+20.94f),new Vector3(1.7f,1.25f,.08f),glass);
                    Box("Fuel district entrance",station,new Vector3(bx,1.2f,junctionZ+20.88f),new Vector3(1.5f,2.4f,.12f),dark,true);
                }
            }
        }
        static void ExpandSideStreetArchitecture()
        {
            var pavement=Mat("Pavement",new Color(.64f,.65f,.62f));
            var glass=Mat("Side street glazing",new Color(.10f,.19f,.23f));glass.SetFloat("_Glossiness",.72f);
            var roof=Mat("Side street roof",new Color(.12f,.15f,.17f));
            var facades=new[]{new Color(.56f,.32f,.24f),new Color(.76f,.72f,.61f),new Color(.48f,.53f,.5f),new Color(.67f,.48f,.34f)};
            // Pavements follow the long arms visible in the screenshots.
            foreach(float z in new[]{127.5f,144.5f,181.5f,198.5f})foreach(int side in new[]{-1,1})Box("Side-road pavement",world,new Vector3(side*40.5f,.04f,z),new Vector3(69,.16f,4),pavement,true);
            foreach(float z in new[]{281.5f,298.5f})foreach(int side in new[]{-1,1})Box("Final-road pavement",world,new Vector3(side*39,.04f,z),new Vector3(66,.16f,4),pavement,true);
            int buildingNumber=0;
            foreach(var p in new[]{
                new Vector3(-30,0,119),new Vector3(-47,0,119),new Vector3(-64,0,119),
                new Vector3(-28,0,153),new Vector3(-46,0,153),new Vector3(-63,0,153),
                new Vector3(-28,0,173),new Vector3(-47,0,173),new Vector3(-64,0,173),
                new Vector3(-30,0,207),new Vector3(-49,0,207),new Vector3(-66,0,207),
                new Vector3(22,0,273),new Vector3(40,0,273),new Vector3(58,0,273),
                new Vector3(22,0,307),new Vector3(40,0,307),new Vector3(58,0,307),
                new Vector3(-22,0,273),new Vector3(-40,0,273),new Vector3(-58,0,273),
                new Vector3(-22,0,307),new Vector3(-40,0,307),new Vector3(-58,0,307)})
            {
                float height=6.8f+(buildingNumber%3)*1.7f, width=14+(buildingNumber%2)*2;
                var root=new GameObject("Side street architecture "+buildingNumber).transform;root.SetParent(world,false);root.position=p;
                Box("Building",root,new Vector3(0,height*.5f,0),new Vector3(width,height,12),Mat("Side facade "+(buildingNumber%4),facades[buildingNumber%4]),true);
                Box("Roof",root,new Vector3(0,height+.18f,0),new Vector3(width+.5f,.36f,12.5f),roof);
                int face=p.z==119||p.z==173||p.z==273?1:-1;
                for(float floor=1.7f;floor<height-1;floor+=2.35f)for(int bay=-2;bay<=2;bay++)
                    Box("Window",root,new Vector3(bay*width/6,floor,face*6.03f),new Vector3(1.35f,1.2f,.08f),glass);
                Box("Entrance",root,new Vector3(0,1.15f,face*6.08f),new Vector3(1.45f,2.3f,.12f),roof,true);
                buildingNumber++;
            }
            var trunk=Mat("Bark",new Color(.34f,.28f,.21f));var leaves=Mat("Leaves",new Color(.27f,.43f,.3f));
            foreach(var p in new[]{new Vector3(-18,0,118),new Vector3(-18,0,154),new Vector3(-18,0,172),new Vector3(-18,0,208),new Vector3(14,0,272),new Vector3(32,0,308),new Vector3(-32,0,272),new Vector3(-14,0,308)})
            {
                var tree=new GameObject("Linden tree").transform;tree.SetParent(world,false);tree.position=p;
                Shape("Trunk",PrimitiveType.Cylinder,tree,new Vector3(0,1.8f,0),new Vector3(.4f,1.8f,.4f),trunk);
                Shape("Crown",PrimitiveType.Sphere,tree,new Vector3(0,4.4f,0),new Vector3(3.8f,4.8f,3.5f),leaves);
            }
        }
        static void BuildSignBoards()
        {
            var white=Mat("Sign white",new Color(.96f,.96f,.94f)); var red=Mat("Sign red",new Color(.75f,.035f,.035f)); var black=Mat("Sign black",new Color(.035f,.04f,.045f));
            var cancel=RoadSign("282",new Vector3(7,2.5f,215),white,black);
            for(int i=-2;i<=2;i++) { var stripe=Box("Cancellation stripe",cancel,new Vector3(i*.14f,0,-.06f),new Vector3(.06f,.8f,.012f),black); stripe.transform.localRotation=Quaternion.Euler(0,0,-35); }
            // Replace the redundant 50 sign at exactly this position: general urban 50 resumes at 282.
            foreach(var t in world.GetComponentsInChildren<Transform>()) if(t!=null && t.name.StartsWith("Posted limit") && Mathf.Abs(t.position.z-215)<.1f) Object.DestroyImmediate(t.gameObject);
            var stop=new GameObject("STOP lesson").transform; stop.SetParent(world,false); stop.gameObject.AddComponent<StopLessonMarker>();
            var board=RoadSign("206",new Vector3(7,2.5f,265),red,white,8); board.SetParent(stop,true); Label("STOP",board,new Vector3(0,0,-.065f),.035f,Color.white);
            Box("Stop lesson line",stop,new Vector3(3,.025f,265),new Vector3(5.5f,.015f,.35f),white);
            var crossing=Joint("Crossing sign",world,new Vector3(7,2.5f,242));
            Box("Pole",crossing,new Vector3(0,-1.25f,.04f),new Vector3(.07f,2.5f,.07f),Mat("Sign metal",Color.gray));
            Box("Blue crossing plate",crossing,Vector3.zero,new Vector3(.9f,.9f,.06f),Mat("Sign blue",new Color(.035f,.18f,.55f)));
            var triangle=RoadSign("Crossing triangle",Vector3.zero,white,white,3); Object.DestroyImmediate(triangle.Find("Pole").gameObject); triangle.SetParent(crossing,false);triangle.localPosition=new Vector3(0,-.04f,-.04f);triangle.localScale=Vector3.one*.85f;triangle.localRotation=Quaternion.Euler(0,0,-90);
            Shape("Pictogram head",PrimitiveType.Sphere,crossing,new Vector3(0,.16f,-.06f),Vector3.one*.1f,black);
            var torso=Box("Pictogram torso",crossing,new Vector3(-.015f,.015f,-.075f),new Vector3(.055f,.2f,.02f),black);torso.transform.localRotation=Quaternion.Euler(0,0,-20);
            for(int side=-1;side<=1;side+=2){var leg=Box("Pictogram leg",crossing,new Vector3(side*.065f,-.13f,-.075f),new Vector3(.04f,.19f,.02f),black);leg.transform.localRotation=Quaternion.Euler(0,0,side*30);}
            for(int i=-2;i<=2;i++)Box("Pictogram zebra",crossing,new Vector3(i*.1f,-.255f,-.075f),new Vector3(.065f,.025f,.02f),black);
        }
        static Transform RoadSign(string code,Vector3 position,Material face,Material border,int sides=48)
        {
            var root=Joint("Sign "+code,world,position);
            Box("Pole",root,new Vector3(0,-1.25f,.05f),new Vector3(.065f,2.5f,.065f),Mat("Sign metal",Color.gray));
            var verts=new Vector3[sides+1]; var triangles=new int[sides*3];
            for(int i=0;i<sides;i++) { float angle=2*Mathf.PI*(i+.5f)/sides; verts[i+1]=new Vector3(Mathf.Cos(angle)*.5f,Mathf.Sin(angle)*.5f,0); triangles[i*3]=0; triangles[i*3+1]=(i+1)%sides+1; triangles[i*3+2]=i+1; }
            string path="Assets/FahrCheck/Generated/Materials/SignPolygon"+sides+".asset"; var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path); if(mesh==null){ mesh=new Mesh(); AssetDatabase.CreateAsset(mesh,path); }else mesh.Clear(); mesh.vertices=verts;mesh.triangles=triangles;mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);
            var plate=Joint("Face",root,Vector3.zero);plate.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;plate.gameObject.AddComponent<MeshRenderer>().sharedMaterial=border;
            var inset=Joint("Sign inset",root,new Vector3(0,0,-.007f));inset.localScale=Vector3.one*.93f;inset.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;inset.gameObject.AddComponent<MeshRenderer>().sharedMaterial=face;
            return root;
        }
    }
}
