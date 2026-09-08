using System;
namespace FahrCheck
{
    public struct FineAssessment
    {
        public int Euros, Points, BanMonths; public string Reference, Note;
        public FineAssessment(int euros,int points,string reference,int ban=0,string note="Standard negligent case; excludes fees and case-specific adjustments.") { Euros=euros; Points=points; Reference=reference; BanMonths=ban; Note=note; }
        public string Display => "Illustrative standard rate: EUR " + Euros + "  |  " + Points + " point(s)" + (BanMonths>0 ? "  |  "+BanMonths+" month driving ban" : "") + "\n" + Reference + ". " + Note;
    }
    public static class FineCatalog
    {
        public const string Source="https://www.gesetze-im-internet.de/bkatv_2013/anlage.html";
        public static FineAssessment Speed(float excess)
        {
            int[] bands={10,15,20,25,30,40,50,60,70,int.MaxValue}; int[] euros={30,50,70,115,180,260,400,560,700,800};
            for(int i=0;i<bands.Length;i++) if(excess<=bands[i]) return new FineAssessment(euros[i],i<3?0:i<5?1:2,"BKat 11.3 / Table 1(c), passenger car, urban",i<5?0:i<7?1:i<8?2:3);
            return default;
        }
        public static FineAssessment Indicator => new FineAssessment(10,0,"BKat 29");
        public static FineAssessment Priority => new FineAssessment(25,0,"BKat 33",0,"Scenario assumes material obstruction, not proven endangerment.");
        public static FineAssessment Pedestrian => new FineAssessment(80,1,"BKat 113 / FeV Annex 13");
        public static FineAssessment Stop => new FineAssessment(10,0,"BKat 136.1");
        public static FineAssessment StopLine => new FineAssessment(10,0,"BKat 154",0,"Stop-line crossing only; not automatically a protected-area red-light offence.");
        public static FineAssessment LeftPriority => new FineAssessment(40,0,"BKat 39");
        public static FineAssessment KeepRight => new FineAssessment(80,1,"BKat 4.2",0,"Scenario records persistent left/middle-lane use that obstructs traffic.");
        public static FineAssessment HighwayDistance => new FineAssessment(35,0,"BKat 12.4",0,"Prototype applies this only above 80 km/h and uses a conservative gameplay threshold.");
        public static FineAssessment HighwayIndicator => new FineAssessment(10,0,"BKat 29");
        public static FineAssessment Red(float seconds) => new FineAssessment(seconds>1?200:90,seconds>1?2:1,seconds>1?"BKat 132.3":"BKat 132",seconds>1?1:0);
    }
}
