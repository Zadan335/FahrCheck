using UnityEngine;
namespace FahrCheck
{
    public sealed class MissionDefinition
    {
        public readonly string Name, Brief; public readonly float Start, Finish; public readonly bool Signals, Priority, Crossing, Turn, Stop, Highway;
        public MissionDefinition(string name,string brief,float start,float finish,bool signals=false,bool priority=false,bool crossing=false,bool turn=false,bool stop=false,bool highway=false)
        { Name=name; Brief=brief; Start=start; Finish=finish; Signals=signals; Priority=priority; Crossing=crossing; Turn=turn; Stop=stop; Highway=highway; }
    }
    public static class MissionCatalog
    {
        public static readonly MissionDefinition[] All = {
            new MissionDefinition("01  /  FIRST DRIVE","Control your speed, pass the 30 sign and reach the practice gate.",18,110),
            new MissionDefinition("02  /  CITY PRIORITY","Cross on green, then yield at the unsigned junction.",90,210,true,true),
            new MissionDefinition("03  /  CROSSING & STOP","Let the pedestrian cross, stop completely at STOP, then reach the gate.",211,275,false,false,true,false,true),
            new MissionDefinition("04  /  CITY EXAM","Signals, priority, pedestrian crossing and your selected final turn.",18,55,true,true,true,true),
            new MissionDefinition("05  /  AUTOBAHN","Drive 2 km on a three-lane Autobahn. No posted maximum; 130 km/h advisory. Keep right and maintain distance.",0,2000,false,false,false,false,false,true)
        };
    }
    public static class RouteRules
    {
        public static bool TurnArea(float x,float z) => Mathf.Abs(x)<16 && z>=278 && z<=300;
        public static bool Destination(float x,float z,int direction) => x*direction>=53 && x*direction<=64 && z>=283 && z<=297;
        public static bool TurnEstablished(float x,float z,float signedHeading,int direction) => x*direction>6 && z>=283 && z<=297 && signedHeading>=50 && signedHeading<=135;
    }
}
