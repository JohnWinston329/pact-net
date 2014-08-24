﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Nancy;
using PactNet.Mocks.MockHttpService.Comparers;
using PactNet.Reporters;

namespace PactNet.Mocks.MockHttpService.Nancy
{
    public class MockProviderAdminRequestHandler : IMockProviderAdminRequestHandler
    {
        private readonly IMockProviderRepository _mockProviderRepository;
        private readonly IProviderServiceRequestComparer _requestComparer;
        private readonly IReporter _reporter;

        public MockProviderAdminRequestHandler(
            IMockProviderRepository mockProviderRepository,
            IReporter reporter,
            IProviderServiceRequestComparer requestComparer)
        {
            _mockProviderRepository = mockProviderRepository;
            _reporter = reporter;
            _requestComparer = requestComparer;
        }

        public Response Handle(NancyContext context)
        {
            return HandleAdminRequest(context);
        }

        private Response HandleAdminRequest(NancyContext context)
        {
            if (context.Request.Method.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase) &&
                context.Request.Path == "/interactions")
            {
                _mockProviderRepository.ClearHandledRequests();

                return GenerateResponse(HttpStatusCode.OK, "Successfully cleared the handled requests.");
            }

            //TODO: Add tests for this
            if (context.Request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase) &&
                context.Request.Path == "/interactions/verification")
            {
                if (_mockProviderRepository == null)
                {
                    return new Response
                    {
                        StatusCode = HttpStatusCode.InternalServerError
                    };
                }

                //Check when no interactions, registered but there have been some calls made (should never happed)
                //Check when interactions 

                var interactions = context.GetMockInteractions();

                //Need to handle nulls etc here
                foreach (var interaction in interactions)
                {
                    var interactionUsages = _mockProviderRepository.HandledRequests.Where(x => x.MatchedInteraction == interaction).ToList();

                    if (interactionUsages == null || !interactionUsages.Any())
                    {
                        _reporter.ReportError(String.Format("Interaction with description '{0}' and provider state '{1}', was not used by the test.", interaction.Description, interaction.ProviderState));
                    }

                    if (interactionUsages.Count() > 1)
                    {
                        _reporter.ReportError(String.Format("Interaction with description '{0}' and provider state '{1}', was used {2} time/s by the test.", interaction.Description, interaction.ProviderState, interactionUsages.Count()));
                    }
                }

                if (_mockProviderRepository.HandledRequests != null &&
                    _mockProviderRepository.HandledRequests.Any())
                {
                    foreach (var stat in _mockProviderRepository.HandledRequests)
                    {
                        _requestComparer.Compare(stat.MatchedInteraction.Request, stat.ActualRequest);
                    }
                }

                try
                {
                    _reporter.ThrowIfAnyErrors();
                }
                catch (Exception ex)
                {
                    return GenerateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }

                return GenerateResponse(HttpStatusCode.OK, "Successfully verified mock provider interactions.");
            }

            return GenerateResponse(HttpStatusCode.NotFound, 
                String.Format("The {0} request for path {1}, does not have a matching mock provider admin action.", context.Request.Method, context.Request.Path));
        }

        private Response GenerateResponse(HttpStatusCode statusCode, string message)
        {
            var responseMessage = message
                .Replace("\r", " ")
                .Replace("\n", "")
                .Replace("\t", " ")
                .Replace(@"\", "");

            return new Response
            {
                StatusCode = statusCode,
                ReasonPhrase = responseMessage,
                Contents = s => SetContent(responseMessage, s)
            };
        }

        private void SetContent(string content, Stream stream)
        {
            var contentBytes = Encoding.UTF8.GetBytes(content);
            stream.Position = 0;
            stream.Write(contentBytes, 0, contentBytes.Length);
            stream.Flush();
        }
    }
}