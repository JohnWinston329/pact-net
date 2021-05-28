﻿using System;

namespace PactNet.Remote
{
    /// <summary>
    /// Pact builder for an existing remote server
    /// </summary>
    public class RemotePactBuilder : IPactBuilder
    {
        private readonly string consumer;
        private readonly string provider;
        private readonly Uri uri;
        private readonly PactConfig config;

        /// <summary>
        /// Initialises a new instance of the <see cref="RemotePactBuilder"/> class.
        /// </summary>
        /// <param name="consumer">Consumer name</param>
        /// <param name="provider">Provider name</param>
        /// <param name="uri">Remote server URI</param>
        /// <param name="config">Configuration</param>
        internal RemotePactBuilder(string consumer, string provider, Uri uri, PactConfig config)
        {
            this.consumer = consumer;
            this.provider = provider;
            this.uri = uri;
            this.config = config;
        }

        /// <summary>
        /// Add a new interaction to the pact
        /// </summary>
        /// <param name="description">Interaction description</param>
        /// <returns>Fluent builder</returns>
        public IRequestBuilder UponReceiving(string description)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finalise the pact
        /// </summary>
        /// <returns>Pact context in which to run interactions</returns>
        public IPactContext Build()
        {
            throw new NotImplementedException();
        }
    }
}