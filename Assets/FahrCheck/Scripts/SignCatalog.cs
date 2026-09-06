namespace FahrCheck
{
    public sealed class SignEntry
    {
        public string Code, Name, Meaning; public bool Enforced;
        public SignEntry(string code,string name,string meaning,bool enforced=false) { Code=code; Name=name; Meaning=meaning; Enforced=enforced; }
    }
    public static class SignCatalog
    {
        public static readonly SignEntry[] All={
            new SignEntry("274","Speed limit","The displayed maximum starts at the sign.",true),
            new SignEntry("278","End of speed limit","The signed maximum ends; general limits still apply. Here: urban 50 km/h."),
            new SignEntry("282","End of restrictions","Ends route speed and overtaking restrictions, not general road rules. Here: urban 50 km/h.",true),
            new SignEntry("206","STOP","Stop completely at the line, then yield.",true),
            new SignEntry("205","Give way","Allow priority traffic through; stop when needed."),
            new SignEntry("306","Priority road","You have priority at following junctions until cancelled."),
            new SignEntry("307","End of priority road","Reassess the priority rules at the next junction."),
            new SignEntry("301","Priority at next junction","Priority applies only at the next junction."),
            new SignEntry("267","No entry","Do not enter from this direction."),
            new SignEntry("250","Vehicles prohibited","Vehicles may not enter unless an exception applies."),
            new SignEntry("209","Mandatory direction","Follow the arrow direction."),
            new SignEntry("220","One-way street","Travel only in the indicated direction."),
            new SignEntry("276","No overtaking","Do not overtake the vehicles covered by this prohibition."),
            new SignEntry("283","No stopping","Voluntary stopping is prohibited."),
            new SignEntry("286","No parking","Parking is prohibited; short permitted stops have exceptions."),
            new SignEntry("350","Pedestrian crossing","Allow intending pedestrians to cross.",true),
            new SignEntry("101","Danger","Be prepared to slow for the indicated hazard."),
            new SignEntry("136","Children","Expect children near the road."),
            new SignEntry("123","Road works","Expect changes to lanes and road surface."),
            new SignEntry("330.1","Autobahn","Motorway rules apply; this is not by itself an unlimited-speed guarantee."),
            new SignEntry("310","Town entry","General urban rules and speed limits begin."),
            new SignEntry("314","Parking","Parking area; read any supplementary conditions.")
        };
    }
}
