using System.Collections.Generic;
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
        public GameObject highwayWorld;
        public HighwayTraffic highwayTraffic;
        static int selectedTurn = 1;
        static int selectedMission = 0;
        public int MissionIndex => selectedMission;
        public MissionDefinition Mission => MissionCatalog.All[selectedMission];
        public bool PassedStop { get; private set; }
        public string Penalty { get; private set; } = "";
        readonly List<string> violations = new List<string>();
        readonly HashSet<string> violationKeys = new HashSet<string>();
        public int ViolationCount => violations.Count;
        public string LastViolationTitle { get; private set; } = "";
        public string ViolationNotification { get; private set; } = "";
        public bool ShowViolationNotification => IsDriving && Time.unscaledTime < notificationUntil;
        public void SelectMission(int index) { if(State == SessionState.Welcome) { selectedMission=Mathf.Clamp(index,0,MissionCatalog.All.Length-1); ConfigureMission(); } }
        public void Menu() { restartImmediately=false; Time.timeScale=1; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        public void NextMission() { selectedMission=(selectedMission+1)%MissionCatalog.All.Length; Menu(); }
        public string CheckpointStatus => Mission.Highway?"NO POSTED LIMIT  •  130 advisory  •  keep right  •  safe distance":(Mission.Signals ? "Signal " + (PassedSignal ? "OK  " : "--  ") : "") + (Mission.Priority ? "Priority " + (PassedPriority ? "OK  " : "--  ") : "") + (Mission.Crossing ? "Crossing " + (PassedCrossing ? "OK  " : "--  ") : "") + (Mission.Stop ? "STOP " + (PassedStop ? "OK  " : "--  ") : "") + (Mission.Turn ? "Turn " + (TurnedRight ? "OK" : "--") : "");
        void Start() { if(State == SessionState.Welcome) ConfigureMission(); }
        void ConfigureMission()
        {
            if(highwayWorld!=null)highwayWorld.SetActive(Mission.Highway);
            car.transform.SetPositionAndRotation(new Vector3(Mission.Highway?312:3,.45f,Mission.Start),Quaternion.identity);
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
        public int CurrentLimit => Mission.Highway?0:RuleLogic.SpeedLimitAt(car.FrontPosition.z);
        public float RemainingMetres => State == SessionState.Complete ? 0 : Mission.Turn ? (TurnedRight ? 0 : Mathf.Max(0,288-car.transform.position.z)) + Mathf.Max(0,53-car.transform.position.x*TurnDirection) : Mathf.Max(0,Mission.Finish-car.transform.position.z);
        public bool PassedSignal { get; private set; }

        float startedAt;
        float previousZ;
        float previousElapsed;
        float notificationUntil;
        float previousX,wrongLaneSeconds,closeFollowingSeconds;
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
            Penalty=""; PassedStop=false;violations.Clear();violationKeys.Clear();LastViolationTitle="";ViolationNotification="";
            ResultTitle = ResultDetail = ResultSource = "";
            PassedSignal = PassedPriority = PassedCrossing = TurnedRight = false;
            rightHeld = 0;
            previousFront = car.FrontPosition;
            startedAt = Time.time;
            previousZ = car.FrontPosition.z;
            previousX=car.transform.position.x;wrongLaneSeconds=closeFollowingSeconds=0;
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

            if(Mission.Highway){EvaluateHighway(front,elapsed);return;}

            // A mission boundary is labelled separately from a traffic offence.
            if (!EncounterRules.InMission(car.transform.position.x, car.transform.position.z) || car.transform.position.y < -2f || Vector3.Dot(car.transform.up, Vector3.up) < 0.35f)
            {
                Finish(false, "Course left", "Follow Lindenallee, then take your selected final turn to the finish.\nThis is a course constraint, not a traffic-rule verdict.", "MISSION BOUNDARY");
                return;
            }

            if (RuleLogic.IsSpeeding(car.SpeedKmh, CurrentLimit))
            {
                RecordViolation("speeding", "Speed limit exceeded", "Your speed: " + car.SpeedKmh.ToString("F1") +
                    " km/h\nPosted limit: " + CurrentLimit + " km/h\nSlow down before passing a lower-limit sign.",
                    "StVO: speed limits / sign 274",FineCatalog.Speed(car.SpeedKmh-CurrentLimit).Display);
            }

            // Swept boundary test prevents skipping the line between physics steps.
            if (Mission.Signals && RuleLogic.ViolatesSignal(previousZ, front.z, previousElapsed, elapsed))
            {
                RecordViolation("red-signal", "Stop signal crossed", "The front of your car crossed the stop line while the signal required a stop.\nWait behind the line until green.", "StVO section 37 / stop-line marking",FineCatalog.StopLine.Display);
            }
            if (RuleLogic.CrossedForward(previousZ, front.z, RuleLogic.StopLineZ)) PassedSignal = true;
            if (encounters != null)
            {
                float priorityTime = Mathf.Lerp(previousElapsed, elapsed, RuleLogic.CrossingFraction(previousZ, front.z, EncounterRules.PriorityEntryZ));
                if (Mission.Priority && EncounterRules.ViolatesYield(previousZ, front.z, EncounterRules.PriorityEntryZ, EncounterRules.MustYieldRight(encounters.PriorityXAt(priorityTime))))
                { RecordViolation("right-of-way", "Right of way not given", "You entered the unsigned junction while the car from your right needed the conflict area. Wait before the junction until it clears.", "StVO section 8 / right before left",FineCatalog.Priority.Display); }
                float crossingTime = Mathf.Lerp(previousElapsed, elapsed, RuleLogic.CrossingFraction(previousZ, front.z, EncounterRules.CrossingEntryZ));
                if (Mission.Crossing && EncounterRules.ViolatesYield(previousZ, front.z, EncounterRules.CrossingEntryZ, EncounterRules.PedestrianHasPriority(encounters.PedestrianXAt(crossingTime), encounters.PedestrianIntent)))
                { RecordViolation("pedestrian", "Pedestrian priority not given", "A pedestrian visibly intended to use the zebra crossing or was still crossing. Allow them to reach the opposite pavement before entering.", "StVO section 26 / pedestrian crossing",FineCatalog.Pedestrian.Display); }
            }
            if (RuleLogic.CrossedForward(previousZ, front.z, EncounterRules.PriorityEntryZ)) PassedPriority = true;
            if (RuleLogic.CrossedForward(previousZ, front.z, EncounterRules.CrossingEntryZ)) PassedCrossing = true;
            rightHeld = car.Indicator == TurnDirection ? rightHeld + Time.fixedDeltaTime : 0;
            float heading = Mathf.DeltaAngle(0, car.transform.eulerAngles.y) * TurnDirection;
            if (Mission.Turn && !TurnedRight && RouteRules.TurnArea(car.transform.position.x,car.transform.position.z) && heading >= 20 && heading < 150)
            {
                if (EncounterRules.MissingTurnIndicator(heading, car.Indicator * TurnDirection, rightHeld))
                { RecordViolation("indicator", TurnName + " turn not signalled", "Use " + (TurnDirection == 1 ? "E" : "Q") + " to indicate " + TurnName + " before turning and keep it on through the turn. This course requires one second of advance indication.", "StVO section 9 / turning indication (course timing model)",FineCatalog.Indicator.Display); }
                var traffic = FindObjectOfType<AmbientTraffic>();
                if (traffic != null) foreach (float z in traffic.positions)
                    if (EncounterRules.LeftTurnConflict(TurnDirection, z))
                    { RecordViolation("left-priority", "Oncoming traffic not given priority", "Wait before turning left across the oncoming lane until approaching traffic has cleared.", "StVO section 9 / left-turn priority",FineCatalog.LeftPriority.Display); break; }
                if (heading >= 50) TurnedRight = true;
            }
            if(Mission.Stop)
            {
                if(front.z>=260 && front.z<=265 && car.SpeedKmh<.3f) PassedStop=true;
                if(RuleLogic.CrossedForward(previousZ,front.z,265) && !PassedStop)
                { RecordViolation("stop-sign","STOP sign not obeyed","Come to a complete stop behind the line before continuing.","StVO sign 206",FineCatalog.Stop.Display); PassedStop=true; }
            }
            bool checkpoints=(!Mission.Signals || PassedSignal) && (!Mission.Priority || PassedPriority) && (!Mission.Crossing || PassedCrossing) && (!Mission.Stop || PassedStop);
            bool destination=Mission.Turn ? TurnedRight && RouteRules.Destination(car.transform.position.x,car.transform.position.z,TurnDirection) : car.transform.position.z>=Mission.Finish && Mathf.Abs(car.transform.position.x)<=5.5f;
            if(checkpoints && destination)
            {
                bool clean=violations.Count==0;
                Finish(clean,clean?"Route complete":"Mission failed: "+violations.Count+" violation"+(violations.Count==1?"":"s"),clean?Mission.Name + " completed.\nTime: " + elapsed.ToString("F1") + " seconds.":"Route completed, but the assessment failed.\n\n"+string.Join("\n",violations.ToArray()),clean?"MISSION COMPLETE":"TRAFFIC VIOLATION SUMMARY");
                if (clean && !RuntimeVerification.Active) { PlayerPrefs.SetInt("FahrCheck.Academy.Completed."+MissionIndex,1); PlayerPrefs.Save(); } return;
            }
            previousFront = front;
            previousZ = front.z;
            previousElapsed = elapsed;
        }

        void EvaluateHighway(Vector3 front,float elapsed)
        {
            if(car.transform.position.x<301.5f||car.transform.position.x>314.5f||car.transform.position.z<-10||car.transform.position.z>2020||car.transform.position.y<-2)
            {Finish(false,"Autobahn left","Remain on the three-lane carriageway.","MISSION BOUNDARY");return;}
            int requiredIndicator=car.transform.position.x<previousX-.35f?-1:car.transform.position.x>previousX+.35f?1:0;
            if(requiredIndicator!=0&&car.Indicator!=requiredIndicator)RecordViolation("highway-indicator","Lane change not signalled","Signal before moving between Autobahn lanes.","StVO sections 5 and 7",FineCatalog.HighwayIndicator.Display);
            bool justified=highwayTraffic!=null&&highwayTraffic.OvertakingJustified(car.transform.position.x,car.transform.position.z);
            wrongLaneSeconds=car.transform.position.x<310&&!justified?wrongLaneSeconds+Time.fixedDeltaTime:0;
            if(wrongLaneSeconds>8)RecordViolation("keep-right","Failure to keep right","Return to the right lane after overtaking.","StVO section 2(2), BKat 4.2",FineCatalog.KeepRight.Display);
            float gap=highwayTraffic==null?999:highwayTraffic.ClosestAhead(car.transform.position.x,car.transform.position.z);
            float threshold=Mathf.Max(12,car.SpeedKmh*.25f);
            closeFollowingSeconds=car.SpeedKmh>80&&gap<threshold?closeFollowingSeconds+Time.fixedDeltaTime:0;
            if(closeFollowingSeconds>1.5f)RecordViolation("highway-distance","Unsafe following distance","Increase the gap to the vehicle ahead.","StVO section 4; BKat 12",FineCatalog.HighwayDistance.Display);
            if(car.SignedSpeed<-1)RecordViolation("highway-reverse","Reversing on Autobahn","Reversing on the through carriageway is prohibited.","StVO section 18(7); BKat 83.3",new FineAssessment(200,2,"BKat 83.3",1).Display);
            if(car.transform.position.z>=Mission.Finish){bool clean=violations.Count==0;Finish(clean,clean?"Autobahn complete":"Mission failed: "+violations.Count+" violation"+(violations.Count==1?"":"s"),clean?"2 km Autobahn drive completed.\n130 km/h was advisory, not a posted limit.":"Route completed, but the assessment failed.\n\n"+string.Join("\n",violations.ToArray()),clean?"MISSION COMPLETE":"AUTOBAHN VIOLATION SUMMARY");if(clean&&!RuntimeVerification.Active){PlayerPrefs.SetInt("FahrCheck.Academy.Completed."+MissionIndex,1);PlayerPrefs.Save();}return;}
            previousX=car.transform.position.x;previousFront=front;previousZ=front.z;previousElapsed=elapsed;
        }

        void RecordViolation(string key,string title,string detail,string source,string penalty)
        {
            if(!violationKeys.Add(key))return;
            LastViolationTitle=title;
            violations.Add((violations.Count+1)+". "+title+" — "+penalty);
            Penalty=string.Join("\n",violations.ToArray());
            ViolationNotification="FOUL: "+title+"\nContinue to the finish. This offence will appear in your result.";
            notificationUntil=Time.unscaledTime+4f;
            Debug.Log("FAHRCHECK VIOLATION RECORDED: "+title+" | "+source+" | "+detail);
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
