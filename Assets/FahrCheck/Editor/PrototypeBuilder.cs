using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FahrCheck.Editor
{
    public static partial class PrototypeBuilder
    {
        public const string ScenePath = "Assets/FahrCheck/Scenes/Lindenallee.unity";
        const string MaterialPath = "Assets/FahrCheck/Generated/Materials";
        static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        static Transform world;

        [MenuItem("FahrCheck/1. Create or rebuild prototype")]
        public static void CreatePrototype()
        {
            SetupWindow.Status = "Create Course requested.";
            if (SetupWindow.BlockReason.Length > 0) { SetupWindow.Status = SetupWindow.BlockReason; if (!Application.isBatchMode) SetupWindow.Open(); throw new System.InvalidOperationException(SetupWindow.BlockReason); }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) { SetupWindow.Status = "Cancelled: current scene was not saved."; return; }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                if (Application.isBatchMode && (!File.Exists(GeneratedHashPath) || File.ReadAllText(GeneratedHashPath) != SceneHash())) throw new System.InvalidOperationException("Scene exists and has edits or no generator fingerprint. Open Setup to confirm rebuilding.");
                if (!Application.isBatchMode && !EditorUtility.DisplayDialog("Rebuild prototype?", "This replaces Lindenallee. A backup copy will be saved under Preserved; existing material assets are reused.", "Back up and rebuild", "Cancel")) { SetupWindow.Status = "Rebuild cancelled."; return; }
                EnsureFolder("Assets/FahrCheck/Preserved");
                if (!AssetDatabase.CopyAsset(ScenePath, "Assets/FahrCheck/Preserved/Lindenallee-" + System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity")) throw new IOException("Backup failed; rebuild cancelled.");
            }
            EnsureFolder("Assets/FahrCheck/Scenes");
            EnsureFolder(MaterialPath);
            try
            {
            Progress("Creating roads and neighbourhood", 0.15f);
            Materials.Clear();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            world = new GameObject("LINDENALLEE / generated course").transform;
            ConfigureLighting();
            CreateRoad();
            CreateTown();

            Progress("Creating vehicle, signals and interface", 0.55f);
            var session = new GameObject("Course Session").AddComponent<CourseSession>();
            var car = CreateCar();
            session.car = car;
            car.session = session;
            CreateEncounters(session);
            CreateSignal(session, new Vector3(6.8f, 0f, RuleLogic.StopLineZ + 1.5f));
            CreateSignal(session, new Vector3(-6.8f, 0f, RuleLogic.StopLineZ + 1.5f));

            CreateSignal(session, new Vector3(-6.8f,0,144)).transform.rotation = Quaternion.Euler(0,180,0);
            CreateSignal(session, new Vector3(6.8f,0,144)).transform.rotation = Quaternion.Euler(0,180,0);
            Box("Oncoming stop line",world,new Vector3(-3,.021f,145),new Vector3(5.5f,.012f,.4f),Materials["Markings"]);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 550f;
            camera.fieldOfView = 65f;
            camera.backgroundColor = new Color(0.65f, 0.79f, 0.86f);
            camera.clearFlags = CameraClearFlags.Skybox;
            cameraObject.AddComponent<AudioListener>();
            var drivingCamera = cameraObject.AddComponent<DrivingCamera>();
            drivingCamera.target = car.transform;
            drivingCamera.session = session;
            cameraObject.transform.position = car.transform.position + new Vector3(0, 3.5f, -7.5f);
            cameraObject.transform.LookAt(car.transform.position + Vector3.forward * 10 + Vector3.up);
            var hud = new GameObject("Driving HUD").AddComponent<DrivingHud>();
            hud.session = session;
            hud.car = car;

            UpgradeVisuals(session);
            ProfessionalPass(session);
            PlayerSettings.companyName = "FahrCheck";
            PlayerSettings.productName = "FahrCheck";
            PlayerSettings.defaultScreenWidth = 1440;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            QualitySettings.vSyncCount = 1;
            QualitySettings.antiAliasing = 4;
            QualitySettings.shadowDistance = 100f;
            Time.fixedDeltaTime = 0.02f;
            EditorSceneManager.MarkSceneDirty(scene);
            Progress("Saving scene and Build Settings", 0.9f);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Unity could not save " + ScenePath);
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            File.WriteAllText(GeneratedHashPath, SceneHash());
            if (!Application.isBatchMode) Selection.activeGameObject = car.gameObject;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.LookAt(new Vector3(0, 0, 135), Quaternion.Euler(55, -25, 0), 155);
            SetupWindow.Status = "Success: Lindenallee saved and registered in Build Settings. Use Open and Play, then Enter.";
            }
            catch (System.Exception e) { SetupWindow.Status = "Generation failed: " + e; throw; }
            finally { EditorUtility.ClearProgressBar(); }
        }

        static string GeneratedHashPath => Path.Combine(Application.dataPath, "../Library/FahrCheck.generated.sha256");
        static string SceneHash() { using (var hash = System.Security.Cryptography.SHA256.Create()) return System.BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath, "../" + ScenePath)))); }
        static void Progress(string message, float amount) { SetupWindow.Status = message; if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("FahrCheck", message, amount); }
        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(parent, path.Substring(path.LastIndexOf('/') + 1)))) throw new IOException("Could not create asset folder " + path);
        }

        static Material Mat(string name, Color color, bool glow = false)
        {
            if (Materials.TryGetValue(name, out Material known)) return known;
            string path = MaterialPath + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            material.SetFloat("_Glossiness", 0.2f);
            if (glow)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.7f);
            }
            EditorUtility.SetDirty(material);
            Materials[name] = material;
            return material;
        }

        static GameObject Shape(string name, PrimitiveType type, Transform parent, Vector3 position,
            Vector3 scale, Material material, bool collider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material mat, bool collider = false)
        {
            return Shape(name, PrimitiveType.Cube, parent, position, scale, mat, collider);
        }

        static void Label(string text, Transform parent, Vector3 position, float size, Color color)
        {
            var go = new GameObject(text);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontSize = 72;
            mesh.characterSize = size;
            mesh.color = color;
            mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            go.GetComponent<MeshRenderer>().sharedMaterial = mesh.font.material;
        }

        static void ConfigureLighting()
        {
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.94f, 0.82f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42, -30, 0);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.63f, 0.76f, 0.86f);
            RenderSettings.ambientEquatorColor = new Color(0.60f, 0.65f, 0.67f);
            RenderSettings.ambientGroundColor = new Color(0.29f, 0.33f, 0.3f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.68f, 0.79f, 0.83f);
            RenderSettings.fogStartDistance = 160f;
            RenderSettings.fogEndDistance = 480f;
            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                string path = MaterialPath + "/Sky.mat";
                var sky = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (sky == null) { sky = new Material(skyShader); AssetDatabase.CreateAsset(sky, path); }
                sky.SetFloat("_AtmosphereThickness", 0.85f);
                RenderSettings.skybox = sky;
                RenderSettings.sun = sun;
            }
        }

        static void CreateRoad()
        {
            Material grass = Mat("Grass", new Color(0.40f, 0.51f, 0.37f));
            Material asphalt = Mat("Asphalt", new Color(0.22f, 0.25f, 0.28f));
            Material pavement = Mat("Pavement", new Color(0.64f, 0.65f, 0.62f));
            Material white = Mat("Markings", new Color(0.95f, 0.94f, 0.88f));
            Box("Ground", world, new Vector3(0, -0.3f, 175), new Vector3(240, 0.5f, 470), grass, true);
            Box("Lindenallee road", world, new Vector3(0, -0.03f, 180), new Vector3(12, 0.08f, 390), asphalt, true);
            Box("Cross street", world, new Vector3(0, -0.04f, 136), new Vector3(170, 0.08f, 14), asphalt, true);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Pavement south", world, new Vector3(side * 8.2f, 0.04f, 59), new Vector3(4.4f, 0.16f, 140), pavement, true);
                foreach (var segment in new[] { new Vector2(143,184), new Vector2(196,284), new Vector2(296,379) })
                    Box("Pavement north", world, new Vector3(side * 8.2f, .04f, (segment.x + segment.y) / 2), new Vector3(4.4f, .16f, segment.y - segment.x), pavement, true);
                for (int z = 0; z < 365; z += 8)
                    if ((z < 125 || z > 145) && Mathf.Abs(z-190)>10 && Mathf.Abs(z-290)>10)
                        Box("Road edge", world, new Vector3(side * 5.7f, 0.018f, z), new Vector3(0.12f, 0.008f, 8), white);
            }
            for (int z = 0; z < 370; z += 9)
                if ((z < 123 || z > 146) && Mathf.Abs(z-190)>10 && Mathf.Abs(z-290)>10)
                    Box("Centre dash", world, new Vector3(0, 0.019f, z), new Vector3(0.15f, 0.008f, 4), white);
            // The line's approach edge matches the detector coordinate.
            Box("Stop line", world, new Vector3(3, 0.021f, RuleLogic.StopLineZ + 0.2f), new Vector3(5.5f, 0.012f, 0.4f), white);
            SpeedSign(50, 10);
            SpeedSign(30, RuleLogic.LimitStartZ);
            SpeedSign(50, RuleLogic.LimitEndZ);
            Label("30 ahead", world, new Vector3(8.1f, 2.8f, 42), 0.11f, Color.white);

            Box("Unsigned cross street", world, new Vector3(0,-.03f,190), new Vector3(80,.08f,12), asphalt,true);
            Box("Final right turn street", world, new Vector3(0,-.03f,290), new Vector3(144,.08f,12), asphalt,true);
            for (int x = -5; x <= 5; x += 2) Box("Zebra crossing", world, new Vector3(x,.03f,245), new Vector3(1,.015f,4),white);
            Label("ZEBRA CROSSING",world,new Vector3(8,3,242),.1f,Color.white);
            Label("<- LEFT     RIGHT ->",world,new Vector3(2,3,280),.12f,Color.white);
            var finishRoot = new GameObject("Finish gate").transform;
            finishRoot.SetParent(world,false); finishRoot.position = new Vector3(EncounterRules.FinishX,0,290); finishRoot.rotation = Quaternion.Euler(0,90,0);
            Material mint = Mat("Finish mint", new Color(0.35f, 0.90f, 0.69f), true);
            Box("Finish left", finishRoot, new Vector3(-5, 3.8f, 0f), new Vector3(0.3f, 7.6f, 0.3f), mint);
            Box("Finish right", finishRoot, new Vector3(5, 3.8f, 0f), new Vector3(0.3f, 7.6f, 0.3f), mint);
            Box("Finish beam", finishRoot, new Vector3(0, 7.4f, 0f), new Vector3(10, 0.65f, 0.4f), mint);
            Label("FINISH", finishRoot, new Vector3(0, 7.4f, 0f - 0.22f), 0.18f, new Color(0.03f, 0.13f, 0.10f));
            for (int i = 0; i < 12; i++)
                Box("Finish tile", finishRoot, new Vector3(-5.5f + i, 0.026f, 0f), new Vector3(0.9f, 0.014f, 1f), i % 2 == 0 ? white : asphalt);
            var leftFinish = Object.Instantiate(finishRoot.gameObject,world).transform;
            leftFinish.name = "Left route finish gate"; leftFinish.position = new Vector3(-EncounterRules.FinishX,0,290); leftFinish.rotation = Quaternion.Euler(0,-90,0);
        }

        static void SpeedSign(int limit, float z)
        {
            var root = new GameObject("Posted limit " + limit).transform;
            root.SetParent(world, false);
            root.localPosition = new Vector3(7.3f, 0, z);
            Material pole = Mat("Metal", new Color(0.37f, 0.42f, 0.45f));
            Box("Pole", root, new Vector3(0, 1.65f, 0), new Vector3(0.07f, 3.3f, 0.07f), pole);
            var red = Shape("Red rim", PrimitiveType.Cylinder, root, new Vector3(0, 3.1f, 0), new Vector3(1.15f, 0.045f, 1.15f), Mat("Sign red", new Color(0.80f, 0.035f, 0.05f)));
            red.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var face = Shape("White face", PrimitiveType.Cylinder, root, new Vector3(0, 3.1f, -0.052f), new Vector3(0.90f, 0.012f, 0.90f), Mat("Sign white", Color.white));
            face.transform.localRotation = Quaternion.Euler(90, 0, 0);
            Label(limit.ToString(), root, new Vector3(0, 3.1f, -0.08f), 0.115f, Color.black);
        }

        static void CreateTown()
        {
            var rng = new System.Random(1701);
            Material roof = Mat("Roof", new Color(0.29f, 0.34f, 0.37f));
            Material glass = Mat("Windows", new Color(0.19f, 0.34f, 0.40f));
            Material trunk = Mat("Bark", new Color(0.34f, 0.28f, 0.21f));
            Material leaves = Mat("Leaves", new Color(0.27f, 0.43f, 0.30f));
            Color[] palette = { new Color(0.83f, 0.75f, 0.62f), new Color(0.74f, 0.77f, 0.71f), new Color(0.81f, 0.65f, 0.53f), new Color(0.86f, 0.85f, 0.78f) };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int z = 12; z < 368; z += 23)
                {
                    if (Mathf.Abs(z - 136) < 22 || Mathf.Abs(z - 190) < 18 || Mathf.Abs(z - 290) < 18) continue;
                    float height = 6f + rng.Next(0, 4) * 2.4f;
                    float width = 11f + rng.Next(0, 5);
                    var building = new GameObject("Townhouse " + side + " / " + z).transform;
                    building.SetParent(world, false);
                    building.localPosition = new Vector3(side * 21f, 0, z);
                    Material wall = Mat("Facade " + rng.Next(0, palette.Length), palette[(z / 23) % palette.Length]);
                    Box("Building", building, new Vector3(0, height / 2, 0), new Vector3(width, height, 14), wall, true);
                    Box("Roof cornice", building, new Vector3(0, height, 0), new Vector3(width + 0.5f, 0.35f, 14.5f), roof);
                    for (float floor = 2f; floor < height - 0.5f; floor += 2.5f)
                        for (int window = -1; window <= 1; window++)
                            Box("Window", building, new Vector3(-side * (width / 2 + 0.025f), floor, window * 3.7f), new Vector3(0.06f, 1.25f, 1.3f), glass);
                    Box("Entrance", building, new Vector3(-side * (width / 2 + 0.05f), 1.25f, 0), new Vector3(0.08f, 2.5f, 1.6f), roof);
                    var tree = new GameObject("Linden tree").transform;
                    tree.SetParent(world, false);
                    tree.localPosition = new Vector3(side * 10.8f, 0, z + 9);
                    Shape("Trunk", PrimitiveType.Cylinder, tree, new Vector3(0, 1.8f, 0), new Vector3(0.4f, 1.8f, 0.4f), trunk);
                    Shape("Crown", PrimitiveType.Sphere, tree, new Vector3(0, 4.4f, 0), new Vector3(3.8f, 4.8f, 3.5f), leaves);
                    Box("Streetlight", world, new Vector3(side * 7f, 2.9f, z - 7), new Vector3(0.08f, 5.8f, 0.08f), roof);
                    Box("Light head", world, new Vector3(side * 6.7f, 5.8f, z - 7), new Vector3(0.9f, 0.13f, 0.4f), roof);
                }
            }
            var streetSign = new GameObject("Street name").transform;
            streetSign.SetParent(world, false);
            streetSign.localPosition = new Vector3(7.6f, 3.8f, 122);
            Box("Sign", streetSign, Vector3.zero, new Vector3(3.7f, 0.65f, 0.08f), Mat("Street blue", new Color(0.05f, 0.21f, 0.43f)));
            Label("Lindenallee", streetSign, new Vector3(0, 0, -0.05f), 0.13f, Color.white);
        }

        static void CreateEncounters(CourseSession session)
        {
            var encounters = new GameObject("Course encounters").AddComponent<CourseEncounters>();
            encounters.session = session; session.encounters = encounters;
            var npc = new GameObject("Priority traffic from right").transform;
            npc.SetParent(world,false); npc.position = new Vector3(26,.65f,187); npc.rotation = Quaternion.Euler(0,-90,0);
            Box("Vehicle body",npc,Vector3.zero,new Vector3(1.8f,.7f,4.2f),Mat("NPC coral",new Color(.8f,.3f,.18f)));
            Box("Cabin",npc,new Vector3(0,.55f,-.2f),new Vector3(1.6f,.6f,2),Mat("NPC windows",new Color(.2f,.3f,.35f)));
            for(int i=0;i<4;i++) Shape("NPC wheel",PrimitiveType.Sphere,npc,new Vector3(i%2==0?-.9f:.9f,-.35f,i<2?1.3f:-1.3f),new Vector3(.3f,.6f,.6f),Mat("NPC tyres",Color.black));
            encounters.priorityCar = npc;
            var person = new GameObject("Pedestrian intending to cross").transform;
            person.SetParent(world,false); person.position = new Vector3(6.5f,.95f,245);
            Shape("Coat",PrimitiveType.Capsule,person,Vector3.zero,new Vector3(.5f,.65f,.45f),Mat("Pedestrian coat",new Color(.9f,.65f,.1f)));
            Shape("Head",PrimitiveType.Sphere,person,new Vector3(0,.8f,0),Vector3.one*.35f,Mat("Pedestrian skin",new Color(.6f,.4f,.3f)));
            encounters.pedestrian = person;
        }

        static CarController CreateCar()
        {
            var root = new GameObject("Player Car");
            root.transform.position = new Vector3(3, 0.45f, RuleLogic.StartZ);
            root.AddComponent<Rigidbody>();
            var bodyCollider = root.AddComponent<BoxCollider>();
            bodyCollider.center = new Vector3(0, 0.65f, 0);
            bodyCollider.size = new Vector3(1.72f, 0.58f, 4.24f);
            var car = root.AddComponent<CarController>();
            Material blue = Mat("Car ocean", new Color(0.09f, 0.37f, 0.53f));
            Material trim = Mat("Car trim", new Color(0.065f, 0.09f, 0.11f));
            Material window = Mat("Car glass", new Color(0.26f, 0.40f, 0.46f));
            Material silver = Mat("Wheel alloy", new Color(0.68f, 0.71f, 0.73f));
            SculptedBody(root.transform, blue);
            Box("Roof", root.transform, new Vector3(0, 1.61f, -0.14f), new Vector3(1.6f, 0.1f, 1.86f), blue);
            Box("Rear glass", root.transform, new Vector3(0, 1.25f, -1.04f), new Vector3(1.5f, 0.66f, 0.05f), window);
            Box("Dashboard", root.transform, new Vector3(0, 1.10f, 0.55f), new Vector3(1.58f, 0.25f, 0.35f), trim);
            Box("Front bumper", root.transform, new Vector3(0, 0.5f, 2.08f), new Vector3(1.74f, 0.16f, 0.10f), trim);
            Box("Rear bumper", root.transform, new Vector3(0, 0.5f, -2.08f), new Vector3(1.74f, 0.16f, 0.10f), trim);
            car.lampOff = Mat("Lamp off", new Color(0.22f, 0.12f, 0.075f));
            car.indicatorOn = Mat("Indicator amber", new Color(1f, 0.55f, 0.04f), true);
            car.brakeOn = Mat("Brake red", new Color(0.95f, 0.05f, 0.03f), true);
            car.leftIndicators = new Renderer[2];
            car.rightIndicators = new Renderer[2];
            car.brakeLamps = new Renderer[2];
            for (int i = 0; i < 2; i++)
            {
                int side = i == 0 ? -1 : 1;
                Box("Front pillar", root.transform, new Vector3(side * 0.76f, 1.28f, 0.75f), new Vector3(0.09f, 0.63f, 0.11f), blue);
                Box("Rear pillar", root.transform, new Vector3(side * 0.76f, 1.28f, -1.03f), new Vector3(0.09f, 0.63f, 0.11f), blue);
                Box("Headlamp", root.transform, new Vector3(side * 0.56f, 0.77f, 2.11f), new Vector3(0.35f, 0.15f, 0.035f), Mat("Headlamp", new Color(1f, 0.94f, 0.7f), true));
                car.brakeLamps[i] = Box("Brake lamp", root.transform, new Vector3(side * 0.57f, 0.77f, -2.11f), new Vector3(0.34f, 0.14f, 0.04f), car.lampOff).GetComponent<Renderer>();
                Renderer[] lamps = i == 0 ? car.leftIndicators : car.rightIndicators;
                for (int end = 0; end < 2; end++)
                    lamps[end] = Box("Indicator", root.transform, new Vector3(side * 0.79f, 0.76f, end == 0 ? 2.12f : -2.12f), new Vector3(0.12f, 0.13f, 0.045f), car.lampOff).GetComponent<Renderer>();
            }
            car.wheels = new WheelCollider[4];
            car.wheelVisuals = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? -0.85f : 0.85f;
                float z = i < 2 ? 1.3f : -1.3f;
                var wheel = new GameObject("Wheel physics " + i);
                wheel.transform.SetParent(root.transform, false);
                wheel.transform.localPosition = new Vector3(x, 0.40f, z);
                var collider = wheel.AddComponent<WheelCollider>();
                collider.mass = 25f;
                collider.radius = 0.34f;
                collider.suspensionDistance = 0.3f;
                collider.forceAppPointDistance = 0.1f;
                var spring = collider.suspensionSpring;
                spring.spring = 35000f;
                spring.damper = 4500f;
                spring.targetPosition = 0.5f;
                collider.suspensionSpring = spring;
                var grip = collider.sidewaysFriction;
                grip.stiffness = 1.5f;
                collider.sidewaysFriction = grip;
                car.wheels[i] = collider;
                var visual = new GameObject("Wheel visual " + i).transform;
                visual.SetParent(root.transform, false);
                visual.localPosition = new Vector3(x, 0.25f, z);
                var tyre = Shape("Tyre", PrimitiveType.Cylinder, visual, Vector3.zero, new Vector3(0.68f, 0.13f, 0.68f), trim);
                tyre.transform.localRotation = Quaternion.Euler(0, 0, 90);
                var hub = Shape("Alloy", PrimitiveType.Cylinder, visual, new Vector3(x < 0 ? -0.137f : 0.137f, 0, 0), new Vector3(0.43f, 0.013f, 0.43f), silver);
                hub.transform.localRotation = Quaternion.Euler(0, 0, 90);
                car.wheelVisuals[i] = visual;
            }
            return car;
        }

        static TrafficSignal CreateSignal(CourseSession session, Vector3 position)
        {
            var root = new GameObject("Traffic signal").transform;
            root.SetParent(world, false);
            root.localPosition = position;
            Material dark = Mat("Signal housing", new Color(0.055f, 0.07f, 0.065f));
            Box("Pole", root, new Vector3(0, 2.1f, 0), new Vector3(0.13f, 4.2f, 0.13f), Mat("Metal", Color.gray));
            Box("Housing", root, new Vector3(0, 4.0f, 0), new Vector3(0.65f, 1.8f, 0.4f), dark);
            var signal = root.gameObject.AddComponent<TrafficSignal>();
            signal.session = session;
            signal.off = Mat("Signal off", new Color(0.10f, 0.14f, 0.11f));
            signal.redOn = Mat("Signal red", new Color(1f, 0.035f, 0.02f), true);
            signal.yellowOn = Mat("Signal yellow", new Color(1f, 0.65f, 0.02f), true);
            signal.greenOn = Mat("Signal green", new Color(0.05f, 0.95f, 0.28f), true);
            Renderer[] lamps = new Renderer[3];
            for (int i = 0; i < 3; i++)
            {
                var lamp = Shape("Lamp " + i, PrimitiveType.Cylinder, root, new Vector3(0, 4.55f - i * 0.55f, -0.23f), new Vector3(0.43f, 0.035f, 0.43f), signal.off);
                lamp.transform.localRotation = Quaternion.Euler(90, 0, 0);
                lamps[i] = lamp.GetComponent<Renderer>();
            }
            signal.redLamp = lamps[0]; signal.yellowLamp = lamps[1]; signal.greenLamp = lamps[2];
            return signal;
        }
    }
}
