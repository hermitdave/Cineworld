// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using Newtonsoft.Json;

namespace Cineworld
{
    public class VideoSearchResults
    {

        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
