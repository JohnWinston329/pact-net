﻿using System.Collections.Generic;
using NSubstitute;
using Nancy;
using Nancy.Culture;
using Nancy.Diagnostics;
using Nancy.Localization;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Mocks.MockHttpService.Nancy;
using Xunit;

namespace PactNet.Tests.Mocks.MockHttpService.Nancy
{
    public class PactAwareContextFactoryTests
    {
        [Fact]
        public void Create_WithRequest_SetRequestOnContext()
        {
            var request = new Request("GET", "/events", "HTTP");

            var mockMockContextService = Substitute.For<IMockContextService>();
            var mockCultureService = Substitute.For<ICultureService>();
            var mockRequestTraceFactory = Substitute.For<IRequestTraceFactory>();
            var mockTextResource = Substitute.For<ITextResource>();

            INancyContextFactory nancyContextFactory = new PactAwareContextFactory(
                mockMockContextService,
                mockCultureService,
                mockRequestTraceFactory,
                mockTextResource);

            var context = nancyContextFactory.Create(request);

            Assert.Equal(request, context.Request);
        }

        [Fact]
        public void Create_WithRequest_CallsMockContentServiceAndAssignsRequestResponsePairsOnNancyContextItem()
        {
            var request = new Request("GET", "/events", "HTTP");
            var requestResponsePairs = new List<KeyValuePair<PactProviderServiceRequest, PactProviderServiceResponse>>
            {
                new KeyValuePair<PactProviderServiceRequest, PactProviderServiceResponse>(new PactProviderServiceRequest { Method = HttpVerb.Get, Path = "/events" }, new PactProviderServiceResponse()),
                new KeyValuePair<PactProviderServiceRequest, PactProviderServiceResponse>(new PactProviderServiceRequest { Method = HttpVerb.Post, Path = "/events" }, new PactProviderServiceResponse()),
            };

            var mockMockContextService = Substitute.For<IMockContextService>();
            var mockCultureService = Substitute.For<ICultureService>();
            var mockRequestTraceFactory = Substitute.For<IRequestTraceFactory>();
            var mockTextResource = Substitute.For<ITextResource>();

            mockMockContextService.GetExpectedRequestResponsePairs().Returns(requestResponsePairs);

            INancyContextFactory nancyContextFactory = new PactAwareContextFactory(
                mockMockContextService,
                mockCultureService,
                mockRequestTraceFactory,
                mockTextResource);

            var context = nancyContextFactory.Create(request);

            Assert.Equal(requestResponsePairs, context.Items["PactMockRequestResponsePairs"]);
            mockMockContextService.Received(1).GetExpectedRequestResponsePairs();
        }
    }
}
