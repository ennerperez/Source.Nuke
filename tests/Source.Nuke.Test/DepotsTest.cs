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

        [Theory, Order(1)]
        [InlineData(243730, "hl2")]
        [InlineData(243750, "hl2mp")]
        public void DepotDownloader(long appId, string gameName)
        {
            try
            {
                Tasks.Depots(options =>
                {
                    options.AppId = appId;
                    options.Verbose = true;
                    options.Mode = Tasks.DepotsMode.DEPOT_DOWNLOADER;
                    options.GameName = gameName;
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

        [Theory, Order(2)]
        [InlineData(243730, "hl2")]
        [InlineData(243750, "hl2mp")]
        public void SteamDownloader(long appId, string gameName)
        {
            try
            {
                Tasks.Depots(options =>
                {
                    options.AppId = appId;
                    options.Verbose = true;
                    options.Mode = Tasks.DepotsMode.STEAM_CMD;
                    options.GameName = gameName;
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
