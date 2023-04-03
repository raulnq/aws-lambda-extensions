using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace MyExtension
{
    public class Extension
    {
        private readonly string _name;

        private readonly Uri _registerUrl;

        private readonly Uri _nextUrl;

        private readonly Uri _subscriptionUrl;

        public Extension(string? name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name), "Extension name cannot be null");

            var apiUri = new UriBuilder(Environment.GetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API")!).Uri;

            _registerUrl = new Uri(apiUri, $"2020-01-01/extension/register");

            _nextUrl = new Uri(apiUri, $"2020-01-01/extension/event/next");

            _subscriptionUrl = new Uri(apiUri, $"2022-07-01/telemetry");
        }

        private async Task<string> Register(HttpClient httpClient)
        {
            var options = new JsonSerializerOptions();

            options.Converters.Add(new JsonStringEnumConverter());

            using var content = new StringContent(JsonSerializer.Serialize(new { events = new EventType[] { EventType.INVOKE, EventType.SHUTDOWN } }, options), Encoding.UTF8, "application/json");

            content.Headers.Add("Lambda-Extension-Name", _name);

            using (var response = await httpClient.PostAsync(_registerUrl, content))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[{_name}] Error response received for registration request: {await response.Content.ReadAsStringAsync()}");
                    response.EnsureSuccessStatusCode();
                }

                var id = response.Headers.GetValues("Lambda-Extension-Identifier").FirstOrDefault();

                if (string.IsNullOrEmpty(id))
                {
                    throw new ApplicationException("Extension API register call didn't return a valid identifier.");
                }

                return id;
            }
        }

        public async Task Start()
        {
            using (var httpClient = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan })
            {
                var id = await Register(httpClient);

                Console.WriteLine($"[{_name}] Registered extension with id = {id}");

                httpClient.DefaultRequestHeaders.Add("Lambda-Extension-Identifier", id);

                while (true)
                {
                    var payload = await GetNext(httpClient);

                    if(payload.EventType== EventType.SHUTDOWN)
                    {
                        Console.WriteLine($"[{_name}] Shutting down extension: {payload.ShutdownReason}");
                        break;
                    }

                    Console.WriteLine($"[{_name}] Handling invoke from extension: {payload.RequestId}");
                }
            }
        }

        private async Task<Payload> GetNext(HttpClient httpClient)
        {
            var contentBody = await httpClient.GetStringAsync(_nextUrl);

            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

            options.Converters.Add(new JsonStringEnumConverter());

            return JsonSerializer.Deserialize<Payload>(contentBody, options)!;
        }
    }
}
