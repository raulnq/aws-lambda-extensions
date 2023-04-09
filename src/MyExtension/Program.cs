using Microsoft.AspNetCore.Http.Json;
using System.Collections.Concurrent;
using System.Reflection;

namespace MyExtension;

class Program
{
    static async Task Main(string[] args)
    {
        var name = (1 == args.Length)
            ? args[0]
            : Assembly.GetEntryAssembly()?.GetName()?.Name;

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive= true;
        });

        var queue = new ConcurrentQueue<string>();

        var app = builder.Build();

        app.Urls.Add("http://sandbox.localdomain:8080");

        app.MapPost("/", (Log[] logs) =>
        {
            foreach (var log in logs)
            {
                queue.Enqueue(log.Record);
            }
            return Results.Ok();
        });

        await app.StartAsync();

        Console.WriteLine("Web server started");

        await new Extension(name).Start(queue);
    }
}