using System;
using System.IO;
using System.Net;
using Nuke.Source;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Source.Nuke.Test
{
    [Collection("Depots"), Order(1)]
    public class DepotsTest
    {
        private NetworkCredential _steamCredentials;

        public DepotsTest()
        {
            if (!File.Exists(".env")) return;
            foreach (var line in File.ReadAllLines(".env"))
            {
                var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }

            var steamUsername = Environment.GetEnvironmentVariable("STEAM_USERNAME");
            var steamPassword = Environment.GetEnvironmentVariable("STEAM_PASSWORD");

            _steamCredentials = new NetworkCredential(steamUsername, steamPassword);
        }

        [Fact, Order(1)]
        public void DownloadAppIdWithDepotDownloader()
        {
            try
            {
                Tasks.Depots(options =>
                {
                    options.AppId = 243750;
                    options.Verbose = true;
                    options.Mode = Tasks.DepotsMode.DEPOT_DOWNLOADER;
                    options.GameName = "hl2mp";
                    options.DepotDirectory = "depots";
                    options.SteamCredentials = _steamCredentials;
                });
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.False(true, e.GetMessage());
            }
        }

        [Fact, Order(2)]
        public void DownloadAppIdWithSteamCmd()
        {
            try
            {
                Tasks.Depots(options =>
                {
                    options.AppId = 243730;
                    options.Verbose = true;
                    options.Mode = Tasks.DepotsMode.STEAM_CMD;
                    options.GameName = "hl2mp";
                    options.DepotDirectory = "depots";
                    options.SteamCredentials = _steamCredentials;
                });
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.False(true, e.GetMessage());
            }
        }
    }
}
