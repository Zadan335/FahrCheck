using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FahrCheck
{
    public sealed class RuntimeVerification : MonoBehaviour
    {
        public static bool Active { get; private set; }
        public static bool SuppressLaneTraffic { get; private set; }
        static int errors;
        int checks;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-fahrcheckSmoke") < 0 || FindObjectOfType<RuntimeVerification>() != null) return;
            Active = true; Application.runInBackground = true;
            var go = new GameObject("Automated runtime verification");
            DontDestroyOnLoad(go);
            go.AddComponent<RuntimeVerification>();
        }
        void Awake() { Application.logMessageReceived += Log; StartCoroutine(Watchdog()); }
        IEnumerator Watchdog() { yield return new WaitForSecondsRealtime(600); Debug.LogError("Runtime verification timed out"); Exit(1); }
        void OnDestroy() { Application.logMessageReceived -= Log; }
        static void Log(string message, string stack, LogType type) { if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)  { errors++; Exit(1); } }
        void Check(bool value, string name) { checks++; if (!value) { Debug.LogError("SMOKE FAIL: " + name); Exit(1); throw new Exception(name); } Debug.Log("SMOKE PASS: " + name); }
        static void Exit(int code)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(code);
#else
            Application.Quit(code);
#endif
        }
        IEnumerator Start()
        {
            yield return null;
            var session = FindObjectOfType<CourseSession>();
            session.SelectMission(3);
            var car = session.car;
            Check(session.State == SessionState.Welcome && Time.timeScale == 0, "Welcome pauses simulation");
            var trafficWheel = FindObjectOfType<AmbientTraffic>().cars[0].GetComponent<TrafficWheelMotion>();
            var initialWheelRotation = trafficWheel.wheels[0].localRotation;
            session.Begin();
            car.AutomatedInput = new CarController.Controls { brake = true };
            yield return new WaitForSeconds(1);
            Check(trafficWheel.TravelledDistance > 1 && Quaternion.Angle(initialWheelRotation, trafficWheel.wheels[0].localRotation) > 1, "Traffic wheels roll with actual travel");
            foreach(var vehicle in FindObjectsOfType<TrafficWheelMotion>())
                if(vehicle.name.StartsWith("Parked car")) Check(vehicle.TravelledDistance < .001f, "Parked wheels remain stationary: " + vehicle.name);
            Check(car.wheels[0].isGrounded && car.wheels[3].isGrounded, "Car settles on wheel colliders");
            car.AutomatedInput = new CarController.Controls { throttle = 1 };
            yield return new WaitForSeconds(3);
            Check(car.SpeedKmh > 6 && car.SignedSpeed > 0, "Wheel physics accelerates forward and speed display source responds");
            car.AutomatedInput = new CarController.Controls { brake = true };
            yield return new WaitForSeconds(2);
            Check(car.SpeedKmh < 1, "Service brake stops car");
            car.AutomatedInput = new CarController.Controls { reverse = true };
            yield return new WaitForSeconds(2);
            Check(car.SignedSpeed < -0.5f, "Reverse drives backwards");
            car.AutomatedInput = new CarController.Controls { handbrake = true };
            yield return new WaitForSeconds(1);
            Check(car.SpeedKmh < 1, "Space brake stops reverse");
            car.AutomatedInput = new CarController.Controls { throttle = 1, steer = .6f };
            float heading = car.transform.eulerAngles.y;
            yield return new WaitForSeconds(2);
            Check(Mathf.Abs(Mathf.DeltaAngle(heading, car.transform.eulerAngles.y)) > 2, "Steering changes vehicle heading");
            car.AutomatedInput = new CarController.Controls { brake = true };
            car.SetIndicator(1);
            Check(car.Indicator == 1, "Indicator control");
            var camera = FindObjectOfType<DrivingCamera>();
            camera.Toggle();
            Check(camera.cockpit && Vector3.Distance(camera.transform.position, car.transform.position) < 3, "Cockpit switch");
            camera.Toggle();
            Check(!camera.cockpit, "Chase switch");
            session.TogglePause();
            float elapsed = session.ElapsedSeconds;
            Vector3 position = car.transform.position;
            yield return new WaitForSecondsRealtime(.2f);
            Check(session.State == SessionState.Paused && session.ElapsedSeconds == elapsed && car.transform.position == position, "Pause freezes car and signal clock");
            session.TogglePause();
            Check(session.IsDriving, "Resume");
            session.Restart();
            yield return null; yield return new WaitForFixedUpdate();
            session = FindObjectOfType<CourseSession>(); car = session.car;
            Check(session.IsDriving && car.FrontPosition.z < 25, "Retry reloads and starts course");
            var laneTraffic=FindObjectOfType<SameDirectionTraffic>();float laneCarStart=laneTraffic.positions[0];
            car.Body.position=new Vector3(3,.4f,laneCarStart+8);car.Body.velocity=Vector3.zero;
            yield return new WaitForSecondsRealtime(1f);
            Check(laneTraffic.positions[0]-laneCarStart<2.5f && car.Body.position.z-laneTraffic.positions[0]>4.5f,"Same-lane traffic brakes for blocked player car");
            SuppressLaneTraffic=true;
            session.Restart();yield return null;yield return new WaitForFixedUpdate();session=FindObjectOfType<CourseSession>();car=session.car;
            laneTraffic=FindObjectOfType<SameDirectionTraffic>();laneTraffic.enabled=false;foreach(var laneCar in laneTraffic.cars)laneCar.gameObject.SetActive(false);
            car.Body.position = new Vector3(8, .4f, 30);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.State == SessionState.Failed && session.ResultSource == "MISSION BOUNDARY", "Boundary is distinct from traffic violation");
            session.Restart(); yield return null; yield return new WaitForFixedUpdate();
            session = FindObjectOfType<CourseSession>(); car = session.car;
            car.Body.position = new Vector3(3, .4f, 67); car.Body.velocity = Vector3.forward * 10;
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.LastViolationTitle == "Speed limit exceeded" && session.ShowViolationNotification, "Speeding records an on-screen foul while driving continues");
            session.Restart(); yield return null; yield return new WaitForFixedUpdate();
            session = FindObjectOfType<CourseSession>(); car = session.car;
            car.Body.position = new Vector3(3, .4f, 128);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.LastViolationTitle == "Stop signal crossed", "Red entry records a foul without ending the route");
            session.Restart(); yield return null; yield return new WaitForFixedUpdate();
            session = FindObjectOfType<CourseSession>(); car = session.car;
            car.AutomatedInput = new CarController.Controls { brake = true };
            yield return new WaitForSeconds(10.2f);
            car.Body.position = new Vector3(3,.4f,128);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.PassedSignal, "Green entry is legal in runtime");
            yield return new WaitForSeconds(15.1f);
            Check(session.IsDriving && RuleLogic.RequiresStop(RuleLogic.AspectAt(session.ElapsedSeconds)), "Light turning red after legal entry does not fail");
            car.Body.position = new Vector3(3,.4f,156);
            yield return new WaitForSeconds(1.5f);
            car.Body.position = new Vector3(3,.4f,185);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.LastViolationTitle == "Right of way not given", "Conflicting right-hand traffic records a foul");
            session.Restart(); yield return null; yield return new WaitForFixedUpdate();
            session = FindObjectOfType<CourseSession>(); car = session.car;
            car.AutomatedInput = new CarController.Controls { brake = true };
            yield return new WaitForSeconds(10.2f);
            car.Body.position = new Vector3(3,.4f,156);
            yield return new WaitForSeconds(10);
            car.Body.position = new Vector3(3,.4f,211);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.PassedPriority, "Waiting for right-hand traffic permits entry");
            car.Body.position = new Vector3(3,.4f,244);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.LastViolationTitle == "Pedestrian priority not given", "Visible pedestrian intent violation is recorded");
            session.Restart(); yield return null; yield return new WaitForFixedUpdate();
            session = FindObjectOfType<CourseSession>(); car = session.car;
            car.AutomatedInput = new CarController.Controls { brake = true };
            yield return new WaitForSeconds(10.2f);
            car.Body.position = new Vector3(3,.4f,156);
            yield return new WaitForSeconds(10);
            car.Body.position = new Vector3(3,.4f,211);
            yield return new WaitForSeconds(13.5f);
            car.Body.position = new Vector3(3,.4f,250);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.PassedCrossing, "Waiting for pedestrian permits crossing");
            car.Body.position = new Vector3(3,.4f,287); car.Body.rotation = Quaternion.Euler(0,30,0);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.LastViolationTitle == "RIGHT turn not signalled", "Unsignalled actual heading change records a foul");
            car.Body.rotation=Quaternion.Euler(0,90,0);yield return new WaitForSecondsRealtime(.15f);
            car.Body.position = new Vector3(56,.4f,287);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.State==SessionState.Failed && session.ResultTitle.Contains("Mission failed") && session.ResultDetail.Contains("RIGHT turn not signalled"),"Completed route fails with accumulated foul list");
            session.Restart(); yield return null; yield return new WaitForFixedUpdate();
            session = FindObjectOfType<CourseSession>(); car = session.car;
            car.AutomatedInput = new CarController.Controls { brake = true };
            yield return new WaitForSeconds(10.2f);
            car.Body.position = new Vector3(3,.4f,156);
            yield return new WaitForSeconds(10);
            car.Body.position = new Vector3(3,.4f,211);
            yield return new WaitForSeconds(13.5f);
            car.Body.position = new Vector3(3,.4f,250); car.SetIndicator(1);
            yield return new WaitForSeconds(1.2f);
            car.Body.position = new Vector3(3,.4f,287); car.Body.rotation = Quaternion.Euler(0,90,0);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.TurnedRight, "Advance indication permits right turn");
            car.Body.position = new Vector3(56,.4f,287);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.State == SessionState.Complete && session.ResultTitle == "Route complete", "All checkpoints and final gate complete mission");
            session.Restart(); yield return null; yield return new WaitForFixedUpdate();
            Check(FindObjectOfType<CourseSession>().IsDriving, "Retry after success");
            var weather = FindObjectOfType<WeatherController>();
            weather.automatic = false; weather.SetWeather(WeatherKind.Rain);
            yield return new WaitForSeconds(5.2f);
            RuntimeCapture.Save("rain-player");
            Check(weather.Weather == WeatherKind.Rain && weather.rain.emission.rateOverTime.constant > 0, "Rain emits visible particles");
            weather.SetWeather(WeatherKind.Fog); yield return new WaitForSeconds(1.6f);
            Check(RenderSettings.fogEndDistance <= 90, "Fog reduces visibility");
            RuntimeCapture.Save("fog-player");
            weather.SetWeather(WeatherKind.Clear); yield return new WaitForSeconds(1.6f);
            Check(RenderSettings.fogEndDistance >= 400, "Clear weather restores visibility");
            RuntimeCapture.Save("clear-player");
            Check(FindObjectOfType<AmbientTraffic>().cars.Length >= 4, "Multiple moving cars configured");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield return null; yield return null;
            session = FindObjectOfType<CourseSession>();
            Check(session.State == SessionState.Welcome, "Fresh scene returns to mission selection");
            session.SelectRoute(-1); session.Begin(); car = session.car;
            Check(session.TurnDirection == -1, "Left-turn mission selectable");
            car.AutomatedInput = new CarController.Controls { brake = true };
            yield return new WaitForSeconds(10.2f);
            car.Body.position = new Vector3(3,.4f,156);
            yield return new WaitForSeconds(10);
            car.Body.position = new Vector3(3,.4f,211);
            yield return new WaitForSeconds(13.5f);
            car.Body.position = new Vector3(3,.4f,250); car.SetIndicator(-1);
            yield return new WaitForSeconds(1.2f);
            var traffic = FindObjectOfType<AmbientTraffic>();
            while (Array.Exists(traffic.positions, z => EncounterRules.LeftTurnConflict(-1,z))) yield return new WaitForSeconds(.2f);
            car.Body.position = new Vector3(3,.4f,293); car.Body.rotation = Quaternion.Euler(0,-90,0);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.IsDriving && session.TurnedRight, "Left turn succeeds after yielding and indicating");
            car.Body.position = new Vector3(-56,.4f,293);
            yield return new WaitForSecondsRealtime(.15f);
            Check(session.State == SessionState.Complete, "Left route completes at west gate");
            yield return AcademyRuntimeVerification.Run(Check);
            Check(errors == 0, "No runtime errors");
            Debug.Log("FAHRCHECK RUNTIME PASSED: " + checks + " checks");
            Exit(0);
        }
    }
}
