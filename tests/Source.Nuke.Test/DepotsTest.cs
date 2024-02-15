using Build.Test.Fixtures;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Source;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Source.Nuke.Test
{
    [Collection("Depots"), Order(1)]
    public class DepotsTest : Abstracts.Test
    {

        public DepotsTest([NotNull] ITestOutputHelper testOutputHelper, [NotNull] TestFixture fixture) : base(testOutputHelper, fixture)
        {
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
                    options.SteamCredentials = TestFixture.SteamCredentials;
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
                Assert.Fail(process != null ? string.Join(Environment.NewLine, process?.Output.ToArray()) : pe.Message);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
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
                    options.SteamCredentials = TestFixture.SteamCredentials;
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
                Assert.Fail(process != null ? string.Join(Environment.NewLine, process?.Output.ToArray()) : pe.Message);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

    }
}
