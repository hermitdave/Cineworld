// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cineworld
{

    public class Results
    {

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("results")]
        public IList<Result> SearchResults { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        [JsonProperty("total_results")]
        public int TotalResults { get; set; }
    }
}
