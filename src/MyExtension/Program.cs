using System.Reflection;

namespace MyExtension
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var name = (1 == args.Length)
                ? args[0]
                : Assembly.GetEntryAssembly()?.GetName()?.Name;

            await new Extension(name).Start();
        }
    }
}