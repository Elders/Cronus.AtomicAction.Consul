using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Elders.Cronus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cronus.AtomicAction.Consul
{
    public partial class ConsulClient : IConsulClient, IDisposable
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger<ConsulClient>();

        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private HttpClient httpClient;
        private ConsulClientOptions options;

        public ConsulClient(IOptionsMonitor<ConsulClientOptions> options)
        {
            options.OnChange(CreateHttpClient);
            CreateHttpClient(options.CurrentValue);
        }

        public async Task<IEnumerable<ReadKeyValueResponse>> ReadKeyValueAsync(string key, bool recurce = false)
        {
            var path = $"/v1/kv/{key}?recurce={recurce}";
            var response = await httpClient.GetAsync(path).ConfigureAwait(false);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return new ReadKeyValueResponse[0]; // key was not found

            if (response.IsSuccessStatusCode == false)
            {
                logger.Error($"Status code: {response.StatusCode} Request: GET {path}; Response: {responseString}");
                return new ReadKeyValueResponse[0];
            }

            var result = JsonSerializer.Deserialize<IEnumerable<ReadKeyValueResponse>>(responseString, serializerOptions);
            return result;
        }

        public async Task<CreateSessionResponse> CreateSessionAsync(CreateSessionRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json);
            var path = "/v1/session/create";

            var response = await httpClient.PutAsync(path, content).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode == false)
            {
                logger.Error($"Status code: {response.StatusCode} Request: PUT {path} {json}; Response: {responseString}");
                return new CreateSessionResponse();
            }

            var result = JsonSerializer.Deserialize<CreateSessionResponse>(responseString, serializerOptions);
            return result;
        }

        public async Task<bool> DeleteSessionAsync(string id)
        {
            var path = $"/v1/session/destroy/{id}";
            var response = await httpClient.PutAsync(path, null).ConfigureAwait(false);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode == false)
            {
                logger.Error($"Status code: {response.StatusCode} Request: PUT {path}; Response: {responseString}");
                return false;
            }

            return bool.Parse(responseString);
        }

        public async Task<bool> CreateKeyValueAsync(CreateKeyValueRequest request)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (string.IsNullOrEmpty(request.SessionId))
                query["cas"] = request.Cas.ToString();
            else
                query["acquire"] = request.SessionId;

            var queryString = query.ToString();

            var json = JsonSerializer.Serialize(request.Value);
            var content = new StringContent(json);
            var path = $"/v1/kv/{request.Key}?{queryString}";
            var response = await httpClient.PutAsync(path, content).ConfigureAwait(false);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode == false)
            {
                logger.Error($"Status code: {response.StatusCode} Request: PUT {path} {json}; Response: {responseString}");
                return false;
            }

            return bool.Parse(responseString);
        }

        public async Task<bool> DeleteKeyValueAsync(string key)
        {
            var path = $"/v1/kv/{key}";
            var response = await httpClient.DeleteAsync(path).ConfigureAwait(false);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode == false)
            {
                logger.Error($"Status code: {response.StatusCode} Request: DELETE {path}; Response: {responseString}");
                return false;
            }

            return bool.Parse(responseString);
        }

        public void Dispose()
        {
            httpClient?.Dispose();
            httpClient = null;
        }

        private void CreateHttpClient(ConsulClientOptions newOptions)
        {
            Dispose();
            options = newOptions;

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(options.Endpoint);

            if (string.IsNullOrEmpty(options.Token) == false)
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
        }
    }
}
