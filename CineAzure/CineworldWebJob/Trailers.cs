// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cineworld
{

    public class Trailers
    {

        [JsonProperty("quicktime")]
        public IList<object> Quicktime { get; set; }

        [JsonProperty("youtube")]
        public IList<Youtube> Youtube { get; set; }
    }
}
