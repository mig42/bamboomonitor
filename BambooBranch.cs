using Newtonsoft.Json;

namespace BambooMonitor
{
    public class BambooBranch
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "shortName")]
        public string ShortName { get; set; }
        [JsonProperty(PropertyName = "shortKey")]
        public string ShortKey { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
