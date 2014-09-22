// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cineworld
{

    public class Casts
    {

        [JsonProperty("cast")]
        public IList<Cast> Cast { get; set; }

        [JsonProperty("crew")]
        public IList<Crew> Crew { get; set; }
    }
}
