using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ImooseApiClient.Models
{
    public class ServerTime
    {

        [JsonProperty("unix_time")]
        public long UnixTime { get; set; }

        [JsonProperty("unix_time_ms")]
        public long UnixTimeMs { get; set; }

        [JsonProperty("rfc1123")]
        public string Rfc1123 { get; set; }

    }
}
