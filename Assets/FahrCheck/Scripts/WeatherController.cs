using UnityEngine;
namespace FahrCheck
{
    public enum WeatherKind { Clear, Rain, Fog }
    public sealed class WeatherController : MonoBehaviour
    {
        public CourseSession session;
        public Light sun;
        public ParticleSystem rain;
        public Material asphalt;
        public Light[] headlights;
        public bool automatic = true;
        public WeatherKind Weather { get; private set; }
        public string Description => Weather == WeatherKind.Rain ? "RAIN / longer braking distances" : Weather == WeatherKind.Fog ? "FOG / reduce speed for visibility" : "CLEAR / dry road";
        Material roadInstance, skyInstance;
        float wetness;
        void Awake()
        {
            roadInstance = new Material(asphalt);
            if (RenderSettings.skybox != null) { skyInstance = new Material(RenderSettings.skybox); RenderSettings.skybox = skyInstance; }
            foreach (var r in FindObjectsOfType<MeshRenderer>()) if (r.sharedMaterial == asphalt) r.sharedMaterial = roadInstance;
        }
        public void Cycle() { automatic = false; SetWeather((WeatherKind)(((int)Weather + 1) % 3)); }
        public void SetWeather(WeatherKind value) { Weather = value; }
        void Update()
        {
            if (!RuntimeVerification.Active && Input.GetKeyDown(KeyCode.F)) Cycle();
            if (automatic) Weather = (WeatherKind)((int)(session.ElapsedSeconds / 60) % 3);
            wetness = Mathf.MoveTowards(wetness, Weather == WeatherKind.Rain ? 1 : 0, Time.deltaTime * .2f);
            roadInstance.SetFloat("_Glossiness", Mathf.Lerp(.22f,.78f,wetness));
            roadInstance.color = Color.Lerp(new Color(.25f,.27f,.29f),new Color(.13f,.16f,.19f),wetness);
            float darkness = Weather == WeatherKind.Clear ? 0 : Weather == WeatherKind.Rain ? .65f : .4f;
            sun.intensity = Mathf.Lerp(1.15f,.45f,darkness);
            sun.color = Color.Lerp(new Color(1,.94f,.82f),new Color(.72f,.8f,.92f),darkness);
            if (skyInstance != null)
            {
                skyInstance.SetColor("_SkyTint", Color.Lerp(new Color(.5f,.5f,.5f),new Color(.38f,.4f,.44f),darkness));
                skyInstance.SetFloat("_Exposure", Mathf.Lerp(1.3f,.7f,darkness));
                skyInstance.SetFloat("_AtmosphereThickness", Weather == WeatherKind.Clear ? .85f : 2f);
            }
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(.63f,.76f,.86f),new Color(.38f,.43f,.48f),darkness);
            RenderSettings.fogColor = Color.Lerp(new Color(.68f,.79f,.83f),new Color(.55f,.6f,.64f),darkness);
            if (Camera.main != null)
            {
                Camera.main.clearFlags = Weather == WeatherKind.Clear ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = RenderSettings.fogColor;
            }
            RenderSettings.fogStartDistance = Weather == WeatherKind.Fog ? 12 : Weather == WeatherKind.Rain ? 50 : 160;
            RenderSettings.fogEndDistance = Weather == WeatherKind.Fog ? 90 : Weather == WeatherKind.Rain ? 230 : 480;
            if (rain != null)
            {
                rain.transform.position = session.car.transform.position + Vector3.up * 12;
                var emission = rain.emission; emission.rateOverTime = Weather == WeatherKind.Rain ? 850 : 0;
            }
            foreach (var lamp in headlights) if (lamp != null) lamp.intensity = Weather == WeatherKind.Clear ? .25f : 3;
            foreach (var wheel in session.car.wheels) { var grip = wheel.sidewaysFriction; grip.stiffness = Mathf.Lerp(1.5f,1.15f,wetness); wheel.sidewaysFriction = grip; var forward = wheel.forwardFriction; forward.stiffness = Mathf.Lerp(1f,.75f,wetness); wheel.forwardFriction = forward; }
        }
        void OnDestroy() { if (roadInstance != null) Destroy(roadInstance); if (skyInstance != null) Destroy(skyInstance); }
    }
}
