using UnityEngine;
using UnityEngine.SceneManagement;

namespace FahrCheck
{
    public enum SessionState { Welcome, Driving, Paused, Failed, Complete }

    [DefaultExecutionOrder(100)]
    public sealed class CourseSession : MonoBehaviour
    {
        public CarController car;
        public CourseEncounters encounters;
        static int selectedTurn = 1;
        static int selectedMission = 0;
        public int MissionIndex => selectedMission;
        public MissionDefinition Mission => MissionCatalog.All[selectedMission];
        public bool PassedStop { get; private set; }
        public string Penalty { get; private set; } = "";
        public void SelectMission(int index) { if(State == SessionState.Welcome) { selectedMission=Mathf.Clamp(index,0,MissionCatalog.All.Length-1); ConfigureMission(); } }
        public void Menu() { restartImmediately=false; Time.timeScale=1; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        public void NextMission() { selectedMission=(selectedMission+1)%MissionCatalog.All.Length; Menu(); }
        public string CheckpointStatus => (Mission.Signals ? "Signal " + (PassedSignal ? "OK  " : "--  ") : "") + (Mission.Priority ? "Priority " + (PassedPriority ? "OK  " : "--  ") : "") + (Mission.Crossing ? "Crossing " + (PassedCrossing ? "OK  " : "--  ") : "") + (Mission.Stop ? "STOP " + (PassedStop ? "OK  " : "--  ") : "") + (Mission.Turn ? "Turn " + (TurnedRight ? "OK" : "--") : "");
        void Start() { if(State == SessionState.Welcome) ConfigureMission(); }
        void ConfigureMission()
        {
            car.transform.SetPositionAndRotation(new Vector3(3,.45f,Mission.Start),Quaternion.identity);
            if(car.Body!=null) { car.Body.position=car.transform.position; car.Body.rotation=Quaternion.identity; car.Body.velocity=Vector3.zero; car.Body.angularVelocity=Vector3.zero; }
            foreach(var gate in FindObjectsOfType<MissionGate>(true)) gate.Configure(Mission,TurnDirection);
            foreach(var stop in FindObjectsOfType<StopLessonMarker>(true)) stop.gameObject.SetActive(Mission.Stop);
        }
        public int TurnDirection => selectedTurn;
        public string TurnName => selectedTurn == 1 ? "RIGHT" : "LEFT";
        public void SelectRoute(int direction) { if (State == SessionState.Welcome) { selectedTurn = direction < 0 ? -1 : 1; ConfigureMission(); } }
        public bool PassedPriority { get; private set; }
        public bool PassedCrossing { get; private set; }
        public bool TurnedRight { get; private set; }
        float rightHeld;
        Vector3 previousFront;
        public SessionState State { get; private set; }
        public string ResultTitle { get; private set; }
        public string ResultDetail { get; private set; }
        public string ResultSource { get; private set; }
        public bool IsDriving => State == SessionState.Driving;
        public float ElapsedSeconds => Mathf.Max(0f, Time.time - startedAt);
        public int CurrentLimit => RuleLogic.SpeedLimitAt(car.FrontPosition.z);
        public float RemainingMetres => State == SessionState.Complete ? 0 : Mission.Turn ? (TurnedRight ? 0 : Mathf.Max(0,288-car.transform.position.z)) + Mathf.Max(0,53-car.transform.position.x*TurnDirection) : Mathf.Max(0,Mission.Finish-car.transform.position.z);
        public bool PassedSignal { get; private set; }

        float startedAt;
        float previousZ;
        float previousElapsed;
        static bool restartImmediately;

        void Awake()
        {
            State = SessionState.Welcome;
            Time.timeScale = 0f;
            startedAt = Time.time;
            if (restartImmediately)
            {
                restartImmediately = false;
                Begin();
            }
        }

        public void Begin()
        {
            ConfigureMission();
            Penalty=""; PassedStop=false;
            ResultTitle = ResultDetail = ResultSource = "";
            PassedSignal = PassedPriority = PassedCrossing = TurnedRight = false;
            rightHeld = 0;
            previousFront = car.FrontPosition;
            startedAt = Time.time;
            previousZ = car.FrontPosition.z;
            previousElapsed = 0f;
            State = SessionState.Driving;
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (RuntimeVerification.Active) return;
            if (Input.GetKeyDown(KeyCode.Return) && State == SessionState.Welcome) Begin();
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
        }

        public void TogglePause()
        {
            if (State == SessionState.Driving) { State = SessionState.Paused; Time.timeScale = 0f; }
            else if (State == SessionState.Paused) { State = SessionState.Driving; Time.timeScale = 1f; }
        }

        void FixedUpdate()
        {
            if (!IsDriving || car == null) return;
            Vector3 front = car.FrontPosition;
            float elapsed = ElapsedSeconds;

            // A mission boundary is labelled separately from a traffic offence.
            if (!EncounterRules.InMission(car.transform.position.x, car.transform.position.z) || car.transform.position.y < -2f || Vector3.Dot(car.transform.up, Vector3.up) < 0.35f)
            {
                Finish(false, "Course left", "Follow Lindenallee, then take your selected final turn to the finish.\nThis is a course constraint, not a traffic-rule verdict.", "MISSION BOUNDARY");
                return;
            }

            if (RuleLogic.IsSpeeding(car.SpeedKmh, CurrentLimit))
            {
                Finish(false, "Speed limit exceeded", "Your speed: " + car.SpeedKmh.ToString("F1") +
                    " km/h\nPosted limit: " + CurrentLimit + " km/h\nSlow down before passing a lower-limit sign.",
                    "StVO: speed limits / sign 274");
                return;
            }

            // Swept boundary test prevents skipping the line between physics steps.
            if (Mission.Signals && RuleLogic.ViolatesSignal(previousZ, front.z, previousElapsed, elapsed))
            {
                Finish(false, "Stop signal crossed", "The front of your car crossed the stop line while the signal required a stop.\nWait behind the line until green.", "StVO section 37 / stop-line marking");
                return;
            }
            if (RuleLogic.CrossedForward(previousZ, front.z, RuleLogic.StopLineZ)) PassedSignal = true;
            if (encounters != null)
            {
                float priorityTime = Mathf.Lerp(previousElapsed, elapsed, RuleLogic.CrossingFraction(previousZ, front.z, EncounterRules.PriorityEntryZ));
                if (Mission.Priority && EncounterRules.ViolatesYield(previousZ, front.z, EncounterRules.PriorityEntryZ, EncounterRules.MustYieldRight(encounters.PriorityXAt(priorityTime))))
                { Finish(false, "Right of way not given", "You entered the unsigned junction while the car from your right needed the conflict area. Wait before the junction until it clears.", "StVO section 8 / right before left"); return; }
                float crossingTime = Mathf.Lerp(previousElapsed, elapsed, RuleLogic.CrossingFraction(previousZ, front.z, EncounterRules.CrossingEntryZ));
                if (Mission.Crossing && EncounterRules.ViolatesYield(previousZ, front.z, EncounterRules.CrossingEntryZ, EncounterRules.PedestrianHasPriority(encounters.PedestrianXAt(crossingTime), encounters.PedestrianIntent)))
                { Finish(false, "Pedestrian priority not given", "A pedestrian visibly intended to use the zebra crossing or was still crossing. Allow them to reach the opposite pavement before entering.", "StVO section 26 / pedestrian crossing"); return; }
            }
            if (RuleLogic.CrossedForward(previousZ, front.z, EncounterRules.PriorityEntryZ)) PassedPriority = true;
            if (RuleLogic.CrossedForward(previousZ, front.z, EncounterRules.CrossingEntryZ)) PassedCrossing = true;
            rightHeld = car.Indicator == TurnDirection ? rightHeld + Time.fixedDeltaTime : 0;
            float heading = Mathf.DeltaAngle(0, car.transform.eulerAngles.y) * TurnDirection;
            if (Mission.Turn && !TurnedRight && RouteRules.TurnArea(car.transform.position.x,car.transform.position.z) && heading >= 20 && heading < 150)
            {
                if (EncounterRules.MissingTurnIndicator(heading, car.Indicator * TurnDirection, rightHeld))
                { Finish(false, TurnName + " turn not signalled", "Use " + (TurnDirection == 1 ? "E" : "Q") + " to indicate " + TurnName + " before turning and keep it on through the turn. This course requires one second of advance indication.", "StVO section 9 / turning indication (course timing model)"); return; }
                var traffic = FindObjectOfType<AmbientTraffic>();
                if (traffic != null) foreach (float z in traffic.positions)
                    if (EncounterRules.LeftTurnConflict(TurnDirection, z))
                    { Finish(false, "Oncoming traffic not given priority", "Wait before turning left across the oncoming lane until approaching traffic has cleared.", "StVO section 9 / left-turn priority"); return; }
                if (heading >= 50) TurnedRight = true;
            }
            if(Mission.Stop)
            {
                if(front.z>=260 && front.z<=265 && car.SpeedKmh<.3f) PassedStop=true;
                if(RuleLogic.CrossedForward(previousZ,front.z,265) && !PassedStop)
                { Finish(false,"STOP sign not obeyed","Come to a complete stop behind the line before continuing.","StVO sign 206"); return; }
            }
            bool checkpoints=(!Mission.Signals || PassedSignal) && (!Mission.Priority || PassedPriority) && (!Mission.Crossing || PassedCrossing) && (!Mission.Stop || PassedStop);
            bool destination=Mission.Turn ? TurnedRight && RouteRules.Destination(car.transform.position.x,car.transform.position.z,TurnDirection) : car.transform.position.z>=Mission.Finish && Mathf.Abs(car.transform.position.x)<=5.5f;
            if(checkpoints && destination)
            {
                Finish(true,"Route complete",Mission.Name + " completed.\nTime: " + elapsed.ToString("F1") + " seconds.","MISSION COMPLETE");
                if (!RuntimeVerification.Active) { PlayerPrefs.SetInt("FahrCheck.Academy.Completed."+MissionIndex,1); PlayerPrefs.Save(); } return;
            }
            previousFront = front;
            previousZ = front.z;
            previousElapsed = elapsed;
        }

        void Finish(bool success, string title, string detail, string source)
        {
            if(!success)
            {
                if(title=="Speed limit exceeded") Penalty=FineCatalog.Speed(car.SpeedKmh-CurrentLimit).Display;
                else if(title.EndsWith("turn not signalled")) Penalty=FineCatalog.Indicator.Display;
                else if(title=="Right of way not given") Penalty=FineCatalog.Priority.Display;
                else if(title=="Pedestrian priority not given") Penalty=FineCatalog.Pedestrian.Display;
                else if(title=="Oncoming traffic not given priority") Penalty=FineCatalog.LeftPriority.Display;
                else if(title=="Stop signal crossed") Penalty=FineCatalog.StopLine.Display;
                else if(title=="STOP sign not obeyed") Penalty=FineCatalog.Stop.Display;
            }
            ResultTitle = title;
            ResultDetail = detail;
            ResultSource = source;
            State = success ? SessionState.Complete : SessionState.Failed;
            Time.timeScale = 0f;
        }

        public void CollisionFailure(string other)
        { if(IsDriving) Finish(false,"Collision", "Contact with " + other + ". The attempt ends for safety. Collision contact alone does not establish legal fault or a fixed fine.","COLLISION / LIABILITY NOT ASSESSED"); }
        public void Restart()
        {
            Time.timeScale = 1f;
            restartImmediately = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void OnDestroy() { Time.timeScale = 1f; }
    }
}
