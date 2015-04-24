﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cineworld
{

    public partial class Person
    {

        [JsonProperty("adult")]
        public bool Adult { get; set; }

        [JsonProperty("also_known_as")]
        public object[] AlsoKnownAs { get; set; }

        [JsonProperty("biography")]
        public string Biography { get; set; }

        [JsonProperty("birthday")]
        public string Birthday { get; set; }

        [JsonProperty("deathday")]
        public string Deathday { get; set; }

        [JsonProperty("homepage")]
        public string Homepage { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("imdb_id")]
        public string ImdbId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("place_of_birth")]
        public string PlaceOfBirth { get; set; }

        [JsonProperty("popularity")]
        public double Popularity { get; set; }

        [JsonProperty("profile_path")]
        public string ProfilePath { get; set; }

        [JsonProperty("credits")]
        public Credits2 Credits { get; set; }
    }

}
