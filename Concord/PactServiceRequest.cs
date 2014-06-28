﻿using System.Collections.Generic;

namespace Concord
{
    public class PactServiceRequest
    {
        public HttpVerb Method { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public dynamic Body { get; set; } //TODO: Handle different Json Formatters CamelCase or PascalCase

        //TODO: Create a customer compare with NancyRequest object
    }
}