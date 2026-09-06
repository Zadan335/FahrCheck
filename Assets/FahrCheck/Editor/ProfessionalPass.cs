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
                if(t.name.StartsWith("Oncoming car ") || t.name.StartsWith("Parked car ") || t.name=="Priority car")
                {
                    var collider=t.gameObject.AddComponent<BoxCollider>(); collider.center=new Vector3(0,.8f,0); collider.size=new Vector3(1.8f,1.2f,4.2f);
                    t.gameObject.AddComponent<CollisionParticipant>();
                    var body=t.gameObject.AddComponent<Rigidbody>(); body.isKinematic=true; body.interpolation=RigidbodyInterpolation.Interpolate;
                }
            BuildHuman(session);
            BuildSignBoards();
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
