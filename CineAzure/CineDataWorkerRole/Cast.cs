// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cineworld
{
    public class Cast
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("character")]
        public string Character { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("cast_id")]
        public int CastId { get; set; }

        [JsonProperty("profile_path")]
        public string ProfilePath { get; set; }
    }
}
