﻿using Newtonsoft.Json;
using PactNet.Matchers;

namespace PactNet.Mocks.MockHttpService.Matchers.Type
{
    public class TypeMatcher : IMatcher
    {
        //Generate JSON using the Ruby spec for now

        [JsonProperty(PropertyName = "json_class")]
        public string Match { get; set; }

        [JsonProperty(PropertyName = "contents")]
        public dynamic Example { get; set; }

        internal TypeMatcher(dynamic example)
        {
            
            Match = "Pact::SomethingLike";
            Example = example;
        }
    }
}