using Build.Test.Fixtures;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Source;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Source.Nuke.Test
{
    [Collection("Assets"), Order(2)]
    public class AssetsTest : Abstracts.Test
    {

        public AssetsTest([NotNull] ITestOutputHelper testOutputHelper, [NotNull] TestFixture fixture) : base(testOutputHelper, fixture)
        {
        }

        [Theory, Order(1)]
        [InlineData(243730, "hl2")]
        [InlineData(243750, "hl2mp")]
        public void Materials(long appId, string gameName)
        {
            try
            {
                global::Nuke.Common.Tools.Source.Tasks.Materials(options =>
                {
                    options.AppId = appId;
                    options.Verbose = true;
                    options.GameName = gameName;
                    options.Folder = "assets";
                    options.DepotDirectory = "depots";
                });
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
        public void Models(long appId, string gameName)
        {
            try
            {
                global::Nuke.Common.Tools.Source.Tasks.Models(options =>
                {
                    options.AppId = appId;
                    options.Verbose = true;
                    options.GameName = gameName;
                    options.Folder = "assets";
                    options.DepotDirectory = "depots";
                });
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

        [Theory, Order(3)]
        [InlineData(243750, "hl2mp", true, true)]
        [InlineData(243750, "hl2mp", false, true)]
        [InlineData(243750, "hl2mp", false, false)]
        [InlineData(243750, "hl2mp", true, false)]
        public void Maps(long appId, string gameName, bool fast, bool slammin)
        {
            try
            {
                global::Nuke.Common.Tools.Source.Tasks.Maps(options =>
                {
                    options.AppId = appId;
                    options.Verbose = true;
                    options.GameName = gameName;
                    options.DepotDirectory = "depots";
                    options.File =  Path.Combine("assets", "maps", "sdk_background.vmf");
                    options.Fast = fast;
                    options.Slammin = slammin;
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
