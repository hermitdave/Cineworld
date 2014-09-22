// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cineworld
{

    public class Country
    {

        [JsonProperty("iso_3166_1")]
        public string Iso31661 { get; set; }

        [JsonProperty("certification")]
        public string Certification { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
    }
}
