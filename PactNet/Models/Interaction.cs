﻿using Newtonsoft.Json;

namespace PactNet.Models
{
    public class Interaction
    {
        [JsonProperty(Order = -3, PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(Order = -2, PropertyName = "provider_state")]
        public string ProviderState { get; set; }
    }
}