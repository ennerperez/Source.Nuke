using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Net;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Build.Test.Fixtures
{
    public class TestFixture : TestBedFixture
    {
        public static NetworkCredential SteamCredentials;
        protected override void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            // Initialize Logger
            var logger = Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            services.AddLogging(l => l.AddSerilog(logger, dispose: true));

            services.AddSingleton(configuration);

            if (!File.Exists(".env")) return;
            foreach (var line in File.ReadAllLines(".env"))
            {
                var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }

            var steamUsername = Environment.GetEnvironmentVariable("STEAM_USERNAME");
            var steamPassword = Environment.GetEnvironmentVariable("STEAM_PASSWORD");

            SteamCredentials = new NetworkCredential(steamUsername, steamPassword);

        }

        protected override ValueTask DisposeAsyncCore() => new();

        protected override IEnumerable<TestAppSettings> GetTestAppSettings()
        {
            var settings = new[]
            {
                new TestAppSettings
                {
                    Filename = "appsettings.json", IsOptional = false
                },
#if DEBUG
                new TestAppSettings
                {
                    Filename = "appsettings.Development.json", IsOptional = true
                }
#endif
            };
            //yield return new() { Filename = "appsettings.json", IsOptional = false };
            return settings;
        }

    }
}
