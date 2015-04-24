// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cineworld
{

    public class Images
    {

        [JsonProperty("base_url")]
        public string BaseUrl { get; set; }

        [JsonProperty("poster_sizes")]
        public IList<string> PosterSizes { get; set; }

        [JsonProperty("backdrop_sizes")]
        public IList<string> BackdropSizes { get; set; }

        [JsonProperty("profile_sizes")]
        public IList<string> ProfileSizes { get; set; }

        [JsonProperty("logo_sizes")]
        public IList<string> LogoSizes { get; set; }
    }
}
