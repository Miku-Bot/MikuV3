using Newtonsoft.Json;

namespace MikuV3.Music.ServiceManager.Entities
{
    public class NicoNicoDougaConfig
    {
        [JsonProperty("mail")]
        public string Mail { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
