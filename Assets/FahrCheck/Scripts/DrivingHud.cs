using UnityEngine;
namespace FahrCheck
{
    public sealed class DrivingHud : MonoBehaviour
    {
        public CourseSession session; public CarController car;
        GUIStyle title, body, small, digits, action; Texture2D disk; bool guide; int page;
        static readonly Color Ink=new Color(.025f,.045f,.065f,.96f), Muted=new Color(.64f,.72f,.79f), Accent=new Color(.3f,.87f,.71f);
        GUIStyle Style(int size,Color color,bool bold=false) { var s=new GUIStyle(GUI.skin.label){fontSize=size,wordWrap=true,fontStyle=bold?FontStyle.Bold:FontStyle.Normal};s.normal.textColor=color;return s; }
        void Init()
        {
            title=Style(28,Color.white,true);body=Style(17,Color.white);small=Style(13,Muted);digits=Style(58,Color.white,true);action=new GUIStyle(GUI.skin.button){fontSize=16,fontStyle=FontStyle.Bold};
            disk=new Texture2D(96,96,TextureFormat.RGBA32,false);var pixels=new Color[96*96];for(int y=0;y<96;y++)for(int x=0;x<96;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(48,48));pixels[y*96+x]=new Color(1,1,1,Mathf.Clamp01(48-d));}disk.SetPixels(pixels);disk.Apply();
        }
        static void Fill(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void Circle(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,disk);GUI.color=old;}
        bool Button(Rect r,string text,bool active=false){var old=GUI.backgroundColor;GUI.backgroundColor=active?Accent:new Color(.21f,.28f,.34f);bool result=GUI.Button(r,text,action);GUI.backgroundColor=old;return result;}
        void OnGUI()
        {
            if(session==null||car==null)return;if(title==null)Init();var matrix=GUI.matrix;bool enabled=GUI.enabled;GUI.enabled=!RuntimeVerification.Active;
            float scale=Mathf.Max(.25f,Mathf.Min(Screen.width/1280f,Screen.height/760f));GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);float w=Screen.width/scale,h=Screen.height/scale;
            Fill(new Rect(20,20,w-40,66),Ink);Fill(new Rect(20,20,4,66),Accent);
            GUI.Label(new Rect(40,30,205,36),"FAHRCHECK",title);GUI.Label(new Rect(250,32,540,34),session.Mission.Name,body);
            var weather=FindObjectOfType<WeatherController>();GUI.Label(new Rect(w-350,31,310,42),(weather!=null?weather.Description:"")+"\nF weather   •   "+session.ElapsedSeconds.ToString("F0")+" s",small);
            if(session.IsDriving)
            {
                Fill(new Rect(w/2-345,99,690,65),Ink);GUI.Label(new Rect(w/2-330,107,660,30),session.encounters.Guidance,body);GUI.Label(new Rect(w/2-330,137,660,22),session.CheckpointStatus,small);
                if(session.ShowViolationNotification){Fill(new Rect(w/2-345,174,690,76),new Color(.48f,.055f,.035f,.96f));GUI.Label(new Rect(w/2-326,184,652,55),session.ViolationNotification,body);}
            }
            Fill(new Rect(20,h-182,280,150),Ink);GUI.Label(new Rect(39,h-177,142,75),Mathf.RoundToInt(car.SpeedKmh).ToString("00"),digits);GUI.Label(new Rect(44,h-102,125,22),"km/h   |   "+(car.SignedSpeed<-.2f?"REVERSE":"DRIVE"),small);
            Circle(new Rect(191,h-159,84,84),new Color(.8f,.06f,.07f));Circle(new Rect(198,h-152,70,70),Color.white);
            var limit=Style(30,Color.black,true);limit.alignment=TextAnchor.MiddleCenter;GUI.Label(new Rect(198,h-152,70,70),session.Mission.Highway?"∞":session.CurrentLimit.ToString(),limit);
            GUI.Label(new Rect(40,h-66,240,24),car.Indicator<0?"◀  LEFT INDICATOR":car.Indicator>0?"RIGHT INDICATOR  ▶":"INDICATORS OFF",small);
            GUI.Label(new Rect(330,h-87,w-650,54),"W / S  accelerate / brake     A / D  steer\nQ / E indicators   X reverse   C camera   R retry   Esc pause",small);
            MiniMap(new Rect(w-228,h-232,208,200));
            float total=session.Mission.Turn?325:session.Mission.Finish-session.Mission.Start;float progress=Mathf.Clamp01(1-session.RemainingMetres/total);
            Fill(new Rect(20,h-16,w-40,4),new Color(.15f,.21f,.26f));Fill(new Rect(20,h-16,(w-40)*progress,4),Accent);
            if(session.State!=SessionState.Driving) Overlay(w,h);
            GUI.matrix=matrix;GUI.enabled=enabled;
        }
        void MiniMap(Rect r)
        {
            Fill(r,Ink);GUI.Label(new Rect(r.x+12,r.y+8,185,23),"ROUTE / "+Mathf.CeilToInt(session.RemainingMetres)+" m",small);
            if(session.Mission.Highway){float progress=Mathf.Clamp01(car.transform.position.z/2000f);for(int i=0;i<3;i++)Fill(new Rect(r.x+62+i*28,r.y+35,3,145),new Color(.33f,.39f,.43f));Circle(new Rect(r.x+116,r.y+174-progress*138,9,9),Accent);return;}
            Vector2 origin=new Vector2(r.center.x,r.yMax-14);float step=.39f;
            Fill(new Rect(origin.x-4,origin.y-145,8,145),new Color(.33f,.39f,.43f));
            foreach(float z in new[]{136f,190f,290f})Fill(new Rect(origin.x-31,origin.y-z*step-3,62,6),new Color(.33f,.39f,.43f));
            Vector3 p=car.transform.position;Circle(new Rect(origin.x+p.x*.5f-4,origin.y-p.z*step-4,8,8),Accent);
            Vector2 goal=session.Mission.Turn?new Vector2(origin.x+session.TurnDirection*55*.5f,origin.y-290*step):new Vector2(origin.x,origin.y-session.Mission.Finish*step);
            Circle(new Rect(goal.x-4,goal.y-4,8,8),new Color(1,.72f,.3f));
        }
        void Overlay(float w,float h)
        {
            Fill(new Rect(0,0,w,h),new Color(.015f,.025f,.035f,.7f));float x=w/2-500,y=h/2-290;
            Fill(new Rect(x,y,1000,580),Ink);Fill(new Rect(x,y,1000,4),Accent);
            if(guide){Guide(x,y);return;}
            if(session.State==SessionState.Welcome)
            {
                if(GUI.Button(new Rect(x+28,y+554,944,22),"Audi R8 2017 by artbyzia · CC BY 4.0 · adapted for FahrCheck — source & licence",GUI.skin.label))Application.OpenURL("https://sketchfab.com/3d-models/audi-r8-2017-3702de6c13cb4829afba67aaf2e92963");
                GUI.Label(new Rect(x+28,y+22,700,44),"DRIVING ACADEMY",title);
                GUI.Label(new Rect(x+28,y+63,920,28),"Four focused missions. One clear rule at a time.",small);
                for(int i=0;i<MissionCatalog.All.Length;i++)
                {
                    string done=PlayerPrefs.GetInt("FahrCheck.Academy.Completed."+i,0)==1?"  ✓":"";
                    if(Button(new Rect(x+28,y+106+i*67,346,55),MissionCatalog.All[i].Name+done,session.MissionIndex==i))session.SelectMission(i);
                }
                GUI.Label(new Rect(x+410,y+110,548,50),session.Mission.Name,title);
                GUI.Label(new Rect(x+410,y+174,548,86),session.Mission.Brief,body);
                GUI.Label(new Rect(x+410,y+270,548,60),"Traffic violations are recorded while you continue driving. The final assessment lists all offences and fines. Collisions still end the attempt for safety.",small);
                if(session.Mission.Turn)
                {
                    if(Button(new Rect(x+410,y+344,263,42),"LEFT turn",session.TurnDirection<0))session.SelectRoute(-1);
                    if(Button(new Rect(x+690,y+344,263,42),"RIGHT turn",session.TurnDirection>0))session.SelectRoute(1);
                }
                if(Button(new Rect(x+410,y+408,543,58),"START MISSION  [Enter]",true))session.Begin();
                if(Button(new Rect(x+28,y+507,346,42),"SIGNS & RULEBOOK"))guide=true;
                GUI.Label(new Rect(x+410,y+500,548,50),"W/S drive & brake • A/D steer • Q/E indicate\nX reverse • C camera • F weather • Esc pause",small);
            }
            else if(session.State==SessionState.Paused)
            {
                GUI.Label(new Rect(x+35,y+35,900,50),"PAUSED",title);GUI.Label(new Rect(x+35,y+106,900,80),session.Mission.Brief,body);
                if(Button(new Rect(x+35,y+255,440,55),"RESUME  [Esc]",true))session.TogglePause();
                if(Button(new Rect(x+510,y+255,440,55),"RETRY  [R]"))session.Restart();
                if(Button(new Rect(x+35,y+333,440,55),"SIGNS & RULEBOOK"))guide=true;
                if(Button(new Rect(x+510,y+333,440,55),"MISSION SELECT"))session.Menu();
            }
            else
            {
                GUI.Label(new Rect(x+35,y+25,900,28),session.State==SessionState.Complete?"MISSION PASSED":"ATTEMPT ENDED",small);
                GUI.Label(new Rect(x+35,y+65,900,55),session.ResultTitle,title);
                GUI.Label(new Rect(x+35,y+140,900,105),session.ResultDetail,body);
                if(!string.IsNullOrEmpty(session.Penalty)){Fill(new Rect(x+30,y+255,940,125),new Color(.12f,.16f,.19f));GUI.Label(new Rect(x+45,y+270,910,98),session.Penalty,body);}
                GUI.Label(new Rect(x+35,y+400,900,50),session.ResultSource,small);
                if(Button(new Rect(x+35,y+490,280,54),"RETRY  [R]"))session.Restart();
                if(Button(new Rect(x+355,y+490,280,54),"MISSION SELECT"))session.Menu();
                if(session.State==SessionState.Complete && Button(new Rect(x+675,y+490,280,54),"NEXT MISSION",true))session.NextMission();
            }
        }
        void Guide(float x,float y)
        {
            GUI.Label(new Rect(x+28,y+24,940,42),"SIGN & RULE LIBRARY",title);
            GUI.Label(new Rect(x+28,y+70,940,40),"Green labels are assessed in these missions. Other signs are reference lessons; supplementary signs can change their meaning.",small);
            for(int i=0;i<6;i++)
            {
                int index=page*6+i;if(index>=SignCatalog.All.Length)break;var sign=SignCatalog.All[index];float row=y+123+i*57;
                Fill(new Rect(x+28,row,84,40),sign.Enforced?new Color(.1f,.4f,.31f):new Color(.2f,.27f,.33f));GUI.Label(new Rect(x+37,row+7,75,28),sign.Code,body);
                GUI.Label(new Rect(x+128,row-2,270,30),sign.Name,body);GUI.Label(new Rect(x+400,row-2,565,49),sign.Meaning,small);
            }
            if(Button(new Rect(x+28,y+494,140,42),"BACK"))guide=false;
            if(Button(new Rect(x+184,y+494,140,42),"PREVIOUS"))page=Mathf.Max(0,page-1);
            if(Button(new Rect(x+340,y+494,140,42),"NEXT"))page=(page+1)%Mathf.CeilToInt(SignCatalog.All.Length/6f);
            if(Button(new Rect(x+503,y+494,222,42),"OFFICIAL SIGN CATALOG"))Application.OpenURL("https://www.gesetze-im-internet.de/stvo_2013/");
            if(Button(new Rect(x+743,y+494,222,42),"OFFICIAL FINE CATALOG"))Application.OpenURL(FineCatalog.Source);
        }
        void OnDestroy(){if(disk!=null)Destroy(disk);}
    }
}
