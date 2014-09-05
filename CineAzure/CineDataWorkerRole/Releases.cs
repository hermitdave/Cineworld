// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cineworld
{

    public class Releases
    {

        [JsonProperty("countries")]
        public IList<Country> Countries { get; set; }
    }
}
