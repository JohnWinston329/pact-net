﻿using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Nancy;
using Newtonsoft.Json;
using PactNet.Configuration.Json;
using PactNet.Mocks.MockHttpService.Comparers;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Models;
using PactNet.Reporters;

namespace PactNet.Mocks.MockHttpService.Nancy
{
    public class MockProviderAdminRequestHandler : IMockProviderAdminRequestHandler
    {
        private readonly IMockProviderRepository _mockProviderRepository;
        private readonly IProviderServiceRequestComparer _requestComparer;
        private readonly IReporter _reporter;
        private readonly IFileSystem _fileSystem;

        public MockProviderAdminRequestHandler(
            IMockProviderRepository mockProviderRepository,
            IReporter reporter,
            IProviderServiceRequestComparer requestComparer,
            IFileSystem fileSystem)
        {
            _mockProviderRepository = mockProviderRepository;
            _reporter = reporter;
            _requestComparer = requestComparer;
            _fileSystem = fileSystem;
        }

        public Response Handle(NancyContext context)
        {
            if (context.Request.Method.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase) &&
                context.Request.Path == Constants.InteractionsPath)
            {
                return HandleDeleteInteractionsRequest();
            }

            if (context.Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) &&
                context.Request.Path == Constants.InteractionsPath)
            {
                return HandlePostInteractionsRequest(context);
            }

            if (context.Request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase) &&
                context.Request.Path == Constants.InteractionsVerificationPath)
            {
                return HandleGetInteractionsVerificationRequest();
            }

            if (context.Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) &&
                context.Request.Path == Constants.PactPath)
            {
                return HandlePostPactRequest(context);
            }

            return GenerateResponse(HttpStatusCode.NotFound,
                String.Format("The {0} request for path {1}, does not have a matching mock provider admin action.", context.Request.Method, context.Request.Path));
        }

        private Response HandleDeleteInteractionsRequest()
        {
            _mockProviderRepository.ClearHandledRequests();
            _mockProviderRepository.ClearTestScopedInteractions();
            return GenerateResponse(HttpStatusCode.OK, "Successfully deleted interactions.");
        }

        private Response HandlePostInteractionsRequest(NancyContext context)
        {
            var interactionJson = ReadContent(context.Request.Body);
            var interaction = JsonConvert.DeserializeObject<ProviderServiceInteraction>(interactionJson);

            _mockProviderRepository.AddInteraction(interaction);

            //TODO: Should we return the interaction as the response? Check Pact ruby responses
            return GenerateResponse(HttpStatusCode.OK, "Successfully registered interaction.");
        }

        private Response HandleGetInteractionsVerificationRequest()
        {
            //Check all registered interactions have been used once and only once
            var registeredInteractions = _mockProviderRepository.TestScopedInteractions;

            if (registeredInteractions.Any())
            {
                foreach (var registeredInteraction in registeredInteractions)
                {
                    var interactionUsages = _mockProviderRepository.HandledRequests.Where(x => x.MatchedInteraction == registeredInteraction).ToList();

                    if (interactionUsages == null || !interactionUsages.Any())
                    {
                        _reporter.ReportError(String.Format("Registered mock interaction with description '{0}' and provider state '{1}', was not used by the test.", registeredInteraction.Description, registeredInteraction.ProviderState));
                    }
                    else if (interactionUsages.Count() > 1)
                    {
                        _reporter.ReportError(String.Format("Registered mock interaction with description '{0}' and provider state '{1}', was used {2} time/s by the test.", registeredInteraction.Description, registeredInteraction.ProviderState, interactionUsages.Count()));
                    }
                }
            }
            else
            {
                if (_mockProviderRepository.HandledRequests != null && _mockProviderRepository.HandledRequests.Any())
                {
                    _reporter.ReportError("No mock interactions were registered, however the mock provider service was called.");
                }
            }

            //Check all handled requests actually match the registered interaction
            if (_mockProviderRepository.HandledRequests != null &&
                _mockProviderRepository.HandledRequests.Any())
            {
                foreach (var handledRequest in _mockProviderRepository.HandledRequests)
                {
                    _requestComparer.Compare(handledRequest.MatchedInteraction.Request, handledRequest.ActualRequest);
                }
            }

            try
            {
                _reporter.ThrowIfAnyErrors();
            }
            catch (Exception ex)
            {
                _reporter.ClearErrors();
                return GenerateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return GenerateResponse(HttpStatusCode.OK, "Successfully verified mock provider interactions.");
        }

        private Response HandlePostPactRequest(NancyContext context)
        {
            //TODO: Have a way to inject a NoOp pact file writer
            //TODO: Path is not going to be correct when running standalone mode
            //TODO: Check to make sure we handle the new ProviderState serialisation name

            var pactDetailsJson = ReadContent(context.Request.Body);
            var pactDetails = JsonConvert.DeserializeObject<PactDetails>(pactDetailsJson);
            var pactFilePath = Path.Combine(Constants.PactFileDirectory, pactDetails.GeneratePactFileName());

            var pactFile = new ProviderServicePactFile
            {
                Provider = pactDetails.Provider,
                Consumer = pactDetails.Consumer,
                Interactions = _mockProviderRepository.Interactions
            };

            var pactFileJson = JsonConvert.SerializeObject(pactFile, JsonConfig.PactFileSerializerSettings);

            try
            {
                _fileSystem.File.WriteAllText(pactFilePath, pactFileJson);
            }
            catch (DirectoryNotFoundException)
            {
                _fileSystem.Directory.CreateDirectory(Constants.PactFileDirectory);
                _fileSystem.File.WriteAllText(pactFilePath, pactFileJson);
            }

            return GenerateResponse(HttpStatusCode.OK, pactFileJson);
        }

        private Response GenerateResponse(HttpStatusCode statusCode, string message)
        {
            return new Response
            {
                StatusCode = statusCode,
                Contents = s => SetContent(message, s)
            };
        }

        private void SetContent(string content, Stream stream)
        {
            var contentBytes = Encoding.UTF8.GetBytes(content);
            stream.Write(contentBytes, 0, contentBytes.Length);
            stream.Flush();
        }

        private string ReadContent(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}