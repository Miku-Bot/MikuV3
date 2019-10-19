using MikuV3.Music.ServiceManager.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.Entities
{
    public class BotConfig
    {
        public class Root
        {
            [JsonProperty("discordToken")]
            public string DiscordToken { get; set; }

            [JsonProperty("weebShToken")]
            public string WeebShToken { get; set; }

            [JsonProperty("youtubeApiToken")]
            public string YoutubeApiToken { get; set; }

            [JsonProperty("ksoftSiToken")]
            public string KsoftSiToken { get; set; }

            public string DbConnectString { get; set; }

            [JsonProperty("dbConfig")]
            public DatabaseConfig DatabaseConfig { get; set; }

            [JsonProperty("lavaConfig")]
            public LavalinkConfig LavalinkConfig { get; set; }

            [JsonProperty("nndConfig")]
            public NicoNicoDougaConfig NicoNicoDougaConfig { get; set; }
        }

        public class DatabaseConfig
        {
            [JsonProperty("hostname")]
            public string Hostname { get; set; }

            [JsonProperty("user")]
            public string User { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }
        }

        public class LavalinkConfig
        {
            [JsonProperty("hostname")]
            public string Hostname { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }

            [JsonProperty("port")]
            public int Port { get; set; }
        }
    }
}
