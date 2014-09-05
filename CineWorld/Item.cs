// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using Newtonsoft.Json;

namespace Cineworld
{

    public class Item
    {

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
