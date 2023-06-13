using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Source;
using Xunit;
using Xunit.Microsoft.DependencyInjection.Attributes;

namespace Build.Test
{
    [Collection("Depots"), TestCaseOrder(2)]
    public class Depots
    {
        private NetworkCredential _steamCredentials;

        public Depots()
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

        [Fact, TestOrder(1)]
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
            catch (ProcessException pe)
            {
                var process = (Process2)pe.GetType()
                    .GetProperty("Process",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.Public)?
                    .GetValue(pe);
                Assert.False(true, process != null ? string.Join(Environment.NewLine, process?.Output.ToArray()) : pe.Message);
            }
            catch (Exception e)
            {
                Assert.False(true, e.Message);
            }
        }

        [Fact, TestOrder(2)]
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
            catch (ProcessException pe)
            {
                var process = (Process2)pe.GetType()
                    .GetProperty("Process",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.Public)?
                    .GetValue(pe);
                Assert.False(true, process != null ? string.Join(Environment.NewLine, process?.Output.ToArray()) : pe.Message);
            }
            catch (Exception e)
            {
                Assert.False(true, e.Message);
            }
        }
    }
}
