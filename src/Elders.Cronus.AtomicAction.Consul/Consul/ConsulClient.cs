using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Elders.Cronus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cronus.AtomicAction.Consul
{
    internal partial class ConsulClient : IConsulClient
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger<ConsulClient>();

        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly HttpClient httpClient;
        private readonly ConsulAggregateRootAtomicActionOptions options;

        public ConsulClient(HttpClient httpClient, IOptionsMonitor<ConsulAggregateRootAtomicActionOptions> options)
        {
            this.httpClient = httpClient;
            this.options = options.CurrentValue;
        }

        const string CreateSessionPath = "/v1/session/create";

        public CreateSessionResponse CreateSession(string name)
        {
            return CreateSessionAsync(new CreateSessionRequest(name, options.LockTtl)).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<CreateSessionResponse> CreateSessionAsync(CreateSessionRequest request)
        {
            var bodyAsJson = JsonSerializer.Serialize(request);
            var content = new StringContent(bodyAsJson);

            HttpResponseMessage response = await httpClient.PutAsync(CreateSessionPath, content).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonSerializer.Deserialize<CreateSessionResponse>(responseString, serializerOptions);
            }

            return new CreateSessionResponse();
        }

        public async Task<IEnumerable<ReadSessionResponse>> ReadSessionAsync(string id)
        {
            var path = $"/session/info/{id}?consistent";

            HttpResponseMessage response = await httpClient.GetAsync(path).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonSerializer.Deserialize<List<ReadSessionResponse>>(responseString, serializerOptions);
            }

            return Enumerable.Empty<ReadSessionResponse>();
        }

        public async Task<bool> DeleteSessionAsync(string id)
        {
            var path = $"/v1/session/destroy/{id}";

            HttpResponseMessage response = await httpClient.PutAsync(path, null).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return bool.Parse(responseString);
            }

            return false;
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
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return bool.Parse(responseString);
            }

            return false;
        }

        public async Task<IEnumerable<ReadKeyValueResponse>> ReadKeyValueAsync(string key, bool recurce = false)
        {
            var path = $"/v1/kv/{key}?recurce={recurce}&consistent";

            var response = await httpClient.GetAsync(path).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                List<ReadKeyValueResponse> result = JsonSerializer.Deserialize<List<ReadKeyValueResponse>>(responseString, serializerOptions);
                return result;
            }

            return Enumerable.Empty<ReadKeyValueResponse>();
        }

        public async Task<bool> DeleteKeyValueAsync(string key)
        {
            var path = $"/v1/kv/{key}";

            var response = await httpClient.DeleteAsync(path).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return bool.Parse(responseString);
            }

            return false;
        }

        public bool CreateKeyValue(string revisionKey, int revision, string session)
        {
            return CreateKeyValueAsync(new CreateKeyValueRequest(revisionKey, revision, session)).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
