using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FahrCheck.Editor
{
    public static class PrototypeChecks
    {
        [MenuItem("FahrCheck/2. Run rule and scene checks")]
        public static void Run()
        {
            var failures = new List<string>();
            int count = 0;
            Action<bool, string> check = (condition, name) => { count++; if (!condition) failures.Add(name); };

            check(RuleLogic.SpeedLimitAt(64.99f) == 50, "50 before lower-limit sign");
            check(RuleLogic.SpeedLimitAt(65f) == 30, "30 applies at lower-limit sign");
            check(RuleLogic.SpeedLimitAt(214.99f) == 30, "30 remains before next sign");
            check(RuleLogic.SpeedLimitAt(215f) == 50, "50 applies at new sign");
            check(!RuleLogic.IsSpeeding(30f, 30), "Driving exactly at limit is legal");
            check(RuleLogic.IsSpeeding(30.1f, 30), "Small genuine excess is detected");
            check(!RuleLogic.IsSpeeding(0f, 30), "Stationary car is legal");
            check(RuleLogic.AspectAt(0) == SignalAspect.Red, "Signal starts red");
            check(RuleLogic.AspectAt(9) == SignalAspect.RedYellow, "Red-yellow phase");
            check(RuleLogic.AspectAt(10) == SignalAspect.Green, "Green phase");
            check(RuleLogic.AspectAt(22) == SignalAspect.Yellow, "Yellow phase");
            check(RuleLogic.AspectAt(25) == SignalAspect.Red, "Cycle repeats");
            check(RuleLogic.RequiresStop(SignalAspect.RedYellow), "Red-yellow still requires stop");
            check(!RuleLogic.ViolatesSignal(120, 126.9f, 1, 2), "Stopping before line does not fail");
            check(RuleLogic.ViolatesSignal(126, 128, 1, 2), "Crossing during red fails");
            check(RuleLogic.ViolatesSignal(126, 128, 9.1f, 9.9f), "Crossing red-yellow fails");
            check(!RuleLogic.ViolatesSignal(126, 128, 11, 12), "Crossing green is legal");
            check(!RuleLogic.ViolatesSignal(126, 128, 23, 24), "Yellow decision is outside first prototype scope");
            check(!RuleLogic.ViolatesSignal(128, 130, 24.9f, 25.1f), "Red after entering cannot retroactively fail");
            check(!RuleLogic.ViolatesSignal(128, 126, 1, 2), "Reverse crossing does not trigger forward detector");
            check(RuleLogic.ViolatesSignal(110, 145, 1, 2), "Fast crossing cannot skip detector");
            check(RuleLogic.ViolatesSignal(126, 128, 24.8f, 25.4f), "Uses red at interpolated crossing time");
            check(!RuleLogic.ViolatesSignal(126, 128, 24.6f, 25.2f), "Uses yellow before red transition");

            check(EncounterRules.MustYieldRight(10), "Approaching right-hand car requires yield");
            check(!EncounterRules.MustYieldRight(-9), "Cleared right-hand car does not require yield");
            check(!EncounterRules.MustYieldRight(26), "Distant car does not reserve junction");
            check(EncounterRules.ViolatesYield(180,190,184,true), "Swept priority entry detects conflict");
            check(!EncounterRules.ViolatesYield(180,183.9f,184,true), "Waiting before priority entry is legal");
            check(!EncounterRules.ViolatesYield(185,188,184,true), "Later conflict cannot retroactively fail entry");
            check(!EncounterRules.ViolatesYield(185,180,184,true), "Reverse is not forward priority entry");
            check(!EncounterRules.ViolatesYield(180,190,184,false), "Clear junction permits entry");
            check(EncounterRules.PedestrianHasPriority(6.5f,true), "Visible pedestrian intent counts before stepping out");
            check(EncounterRules.PedestrianHasPriority(0,true), "Pedestrian on crossing has priority");
            check(!EncounterRules.PedestrianHasPriority(-7,true), "Cleared pedestrian releases crossing");
            check(!EncounterRules.PedestrianHasPriority(6.5f,false), "No intent does not reserve crossing");
            check(EncounterRules.MissingTurnIndicator(25,0,0), "Unsignalled turn fails");
            check(EncounterRules.MissingTurnIndicator(25,-1,5), "Wrong indicator fails");
            check(EncounterRules.MissingTurnIndicator(25,1,.5f), "Late indication fails course timing model");
            check(!EncounterRules.MissingTurnIndicator(25,1,1), "Advance right indication passes");
            check(!EncounterRules.MissingTurnIndicator(10,0,0), "Small steering corrections are not turns");
            check(EncounterRules.InMission(40,287), "Final right-turn street is in mission");
            check(!EncounterRules.InMission(20,190), "Wrong route is mission boundary");
            var encounters = UnityEngine.Object.FindObjectOfType<CourseEncounters>();
            check(encounters != null && encounters.priorityCar != null && encounters.pedestrian != null, "Traffic and pedestrian configured");
            check(!EncounterRules.MissingIndicator(-30,-1,-1,2), "Advance left indication permits left turn");
            check(EncounterRules.MissingIndicator(-30,-1,1,2), "Right indicator does not permit left turn");
            check(EncounterRules.InMission(-40,293), "Left exit is inside mission");
            check(EncounterRules.LeftTurnConflict(-1,305), "Left turn yields to approaching oncoming traffic");
            check(!EncounterRules.LeftTurnConflict(1,305), "Right turn does not cross oncoming lane");
            check(!EncounterRules.LeftTurnConflict(-1,270), "Cleared oncoming car releases left turn");
            var weather = UnityEngine.Object.FindObjectOfType<WeatherController>();
            check(weather != null && weather.rain != null && weather.sun != null && weather.asphalt != null, "Weather references configured");
            var traffic = UnityEngine.Object.FindObjectOfType<AmbientTraffic>();
            check(traffic != null && traffic.cars.Length == 4 && traffic.positions.Length == 4, "Four moving traffic participants");
            var bodyMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/FahrCheck/Generated/Materials/CarBody.asset");
            float volume = 0;
            if (bodyMesh != null) { var vertices=bodyMesh.vertices; var faces=bodyMesh.triangles; for(int i=0;i<faces.Length;i+=3) volume += Vector3.Dot(vertices[faces[i]],Vector3.Cross(vertices[faces[i+1]],vertices[faces[i+2]]))/6f; }
            check(volume > 0, "Car body has outward-facing closed geometry");
            var car = UnityEngine.Object.FindObjectOfType<CarController>();
            var session = UnityEngine.Object.FindObjectOfType<CourseSession>();
            var camera = UnityEngine.Object.FindObjectOfType<DrivingCamera>();
            var hud = UnityEngine.Object.FindObjectOfType<DrivingHud>();
            check(car != null, "Generated scene contains a car");
            check(session != null && session.car == car && session.encounters == encounters, "Session references the car");
            if (car != null)
            {
                check(car.wheels != null && car.wheels.Length == 4, "Four wheel colliders assigned");
                check(car.wheelVisuals != null && car.wheelVisuals.Length == 4, "Four wheel visuals assigned");
                check(car.session == session, "Car references the session");
                check(car.GetComponent<Rigidbody>() != null, "Car has Rigidbody");
            }
            check(camera != null && camera.target == (car != null ? car.transform : null), "Camera follows car");
            check(hud != null && hud.car == car && hud.session == session, "HUD references configured");
            check(EditorBuildSettings.scenes.Length > 0 && EditorBuildSettings.scenes[0].path == PrototypeBuilder.ScenePath
                && EditorBuildSettings.scenes[0].enabled, "Prototype registered in build settings");
            var signals = UnityEngine.Object.FindObjectsOfType<TrafficSignal>();
            check(signals.Length == 4, "Four opposing signal heads created");
            foreach (var signal in signals)
                check(signal.session == session && signal.redLamp != null && signal.yellowLamp != null && signal.greenLamp != null,
                    "Signal references configured");

            if (failures.Count > 0)
                throw new InvalidOperationException("FahrCheck checks failed:\n- " + string.Join("\n- ", failures));
            Debug.Log("FahrCheck: all " + count + " rule and scene checks passed. Driving feel and rendering still require a Play Mode check.");
        }
    }
}
