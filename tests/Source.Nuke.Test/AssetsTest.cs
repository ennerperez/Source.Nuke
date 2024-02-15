using Build.Test.Fixtures;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;
using Nuke.Common.Tooling;
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

        [Fact, Order(1)]
        public void GenerateMaterials()
        {
            try
            {
                global::Nuke.Common.Tools.Source.Tasks.Materials(options =>
                {
                    options.AppId = 243750;
                    options.Verbose = true;
                    options.GameName = "hl2mp";
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

        [Fact, Order(2)]
        public void GenerateModels()
        {
            try
            {
                global::Nuke.Common.Tools.Source.Tasks.Models(options =>
                {
                    options.AppId = 243750;
                    options.Verbose = true;
                    options.GameName = "hl2mp";
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

    }
}
