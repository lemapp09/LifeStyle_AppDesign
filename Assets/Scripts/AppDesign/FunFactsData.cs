using Newtonsoft.Json;

namespace AppDesign
{
    public class FunFactsData
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("html")]
        public string Html { get; set; }
    }
}
