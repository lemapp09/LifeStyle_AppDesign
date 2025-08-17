using System.Collections.Generic;
using Newtonsoft.Json;

namespace AppDesign
{

    public class MoneyData
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("documentation")]
        public string Documentation { get; set; }

        [JsonProperty("terms_of_use")]
        public string TermsOfUse { get; set; }

        [JsonProperty("time_last_update_unix")]
        public long TimeLastUpdateUnix { get; set; }

        [JsonProperty("time_last_update_utc")]
        public string TimeLastUpdateUtc { get; set; }

        [JsonProperty("time_next_update_unix")]
        public long TimeNextUpdateUnix { get; set; }

        [JsonProperty("time_next_update_utc")]
        public string TimeNextUpdateUtc { get; set; }

        [JsonProperty("time_eol_unix")]
        public int TimeEolUnix { get; set; }

        [JsonProperty("base_code")]
        public string BaseCode { get; set; }

        [JsonProperty("rates")]
        public Dictionary<string, double> Rates { get; set; }
    }
}