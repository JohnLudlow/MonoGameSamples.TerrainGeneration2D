using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D
{
    internal static class Log
    {
        private static readonly IConfiguration _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        public static readonly ILoggerFactory Factory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConfiguration(_config.GetSection("Logging"))
                .AddConsole();
        });

        public static ILogger Create<T>() => Factory.CreateLogger<T>();

        public static IConfiguration Config => _config;
    }
}
