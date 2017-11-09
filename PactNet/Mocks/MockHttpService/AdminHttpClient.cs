using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using PactNet.Configuration.Json;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Mocks.MockHttpService.Models;
using static System.String;

namespace PactNet.Mocks.MockHttpService
{
    internal class AdminHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpMethodMapper _httpMethodMapper;

        internal AdminHttpClient(
            Uri baseUri,
            HttpMessageHandler handler)
        {
            if (baseUri.Host == "0.0.0.0")
            {
                baseUri = GetLocalhostUri(baseUri);
            }
            _httpClient = new HttpClient(handler) { BaseAddress = baseUri };
            _httpMethodMapper = new HttpMethodMapper();
        }

        private static Uri GetLocalhostUri(Uri baseUri)
        {
            return new Uri(baseUri.AbsolutePath.Replace("0.0.0.0", "localhost"));
        }

        public AdminHttpClient(Uri baseUri) : 
            this(baseUri, new HttpClientHandler())
        {
        }

        public void SendAdminHttpRequest(HttpVerb method, string path)
        {
            SendAdminHttpRequest<object>(method, path, null);
        }

        public void SendAdminHttpRequest<T>(HttpVerb method, string path, T requestContent, IDictionary<string, string> headers = null) where T : class
        {
            var responseContent = Empty;

            var request = new HttpRequestMessage(_httpMethodMapper.Convert(method), path);
            request.Headers.Add(Constants.AdministrativeRequestHeaderKey, "true");

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (requestContent != null)
            {
                var requestContentJson = JsonConvert.SerializeObject(requestContent, JsonConfig.ApiSerializerSettings);
                request.Content = new StringContent(requestContentJson, Encoding.UTF8, "application/json");
            }

            var response = _httpClient.SendAsync(request, CancellationToken.None).Result;
            var responseStatusCode = response.StatusCode;

            if (response.Content != null)
            {
                responseContent = response.Content.ReadAsStringAsync().Result;
            }

            Dispose(request);
            Dispose(response);

            if (responseStatusCode != HttpStatusCode.OK)
            {
                throw new PactFailureException(responseContent);
            }
        }

        private void Dispose(IDisposable disposable)
        {
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}