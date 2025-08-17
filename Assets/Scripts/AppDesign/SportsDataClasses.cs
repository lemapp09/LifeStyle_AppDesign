using System.Collections.Generic;
using UnityEngine;

namespace AppDesign
{
    public class SportsDataClasses
    {
        // SportsRoot
        public class SportsRoot
        {
            public List<Sport> sportsRoot { get; set; }
        }

        public class Sport
        {
            public string idSport { get; set; }
            public string strSport { get; set; }
            public string strFormat { get; set; }
            public string strSportThumb { get; set; }
            public string strSportThumbBW { get; set; }
            public string strSportIconGreen { get; set; }
            public string strSportDescription { get; set; }
        }

        // SportsTeamRoot
        public class SportsTeamRoot
        {
            public List<SportsTeam> sportsteams { get; set; }
        }

        public class SportsTeam
        {
            public string idTeam { get; set; }
            public string idESPN { get; set; }
            public string idAPIfootball { get; set; }
            public string intLoved { get; set; }
            public string strTeam { get; set; }
            public string strTeamAlternate { get; set; }
            public string strTeamShort { get; set; }
            public string intFormedYear { get; set; }
            public string strSport { get; set; }
            public string strLeague { get; set; }
            public string idLeague { get; set; }
            public string strLeague2 { get; set; }
            public string idLeague2 { get; set; }
            public string strLeague3 { get; set; }
            public string idLeague3 { get; set; }
            public string strLeague4 { get; set; }
            public string idLeague4 { get; set; }
            public string strLeague5 { get; set; }
            public string idLeague5 { get; set; }
            public string strLeague6 { get; set; }
            public object idLeague6 { get; set; }
            public string strLeague7 { get; set; }
            public object idLeague7 { get; set; }
            public object strDivision { get; set; }
            public string idVenue { get; set; }
            public string strStadium { get; set; }
            public string strKeywords { get; set; }
            public string strRSS { get; set; }
            public string strLocation { get; set; }
            public string intStadiumCapacity { get; set; }
            public string strWebsite { get; set; }
            public string strFacebook { get; set; }
            public string strTwitter { get; set; }
            public string strInstagram { get; set; }
            public string strDescriptionEN { get; set; }
            public string strDescriptionDE { get; set; }
            public string strDescriptionFR { get; set; }
            public object strDescriptionCN { get; set; }
            public string strDescriptionIT { get; set; }
            public string strDescriptionJP { get; set; }
            public string strDescriptionRU { get; set; }
            public string strDescriptionES { get; set; }
            public string strDescriptionPT { get; set; }
            public object strDescriptionSE { get; set; }
            public object strDescriptionNL { get; set; }
            public object strDescriptionHU { get; set; }
            public string strDescriptionNO { get; set; }
            public object strDescriptionIL { get; set; }
            public object strDescriptionPL { get; set; }
            public string strColour1 { get; set; }
            public string strColour2 { get; set; }
            public string strColour3 { get; set; }
            public string strGender { get; set; }
            public string strCountry { get; set; }
            public string strBadge { get; set; }
            public string strLogo { get; set; }
            public string strFanart1 { get; set; }
            public string strFanart2 { get; set; }
            public string strFanart3 { get; set; }
            public string strFanart4 { get; set; }
            public string strBanner { get; set; }
            public string strEquipment { get; set; }
            public string strYoutube { get; set; }
            public string strLocked { get; set; }
        }
    }

}
