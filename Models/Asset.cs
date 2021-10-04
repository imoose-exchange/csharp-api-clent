using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ImooseApiClient.Models.Response
{
    public class Asset
    {

        public String id { get; set; }
        public String name { get; set; }
        [JsonProperty("class")]
        public String assetClass { get; set; }
        [JsonProperty("subclass")]
        public String assetSubClass { get; set; }
        public int precision { get; set; }
        public String state { get; set; }


    }
}
