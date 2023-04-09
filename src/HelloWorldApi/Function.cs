using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace HelloWorldApi
{
    public class Function
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
        {
            context.Logger.LogInformation($"log from function: {Guid.NewGuid()}");

            await Task.Delay(Random.Shared.Next(25, 100));

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = @"{""Message"":""Hello world!""}",
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}