using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace StatsBot
{
    public class Program
    {
        public static void Main(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls($"http://+:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}")
                .Build()
                .Run();
    }
}
