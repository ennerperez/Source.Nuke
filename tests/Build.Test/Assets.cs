using System;
using System.Linq;
using System.Reflection;
using Nuke.Common.Tooling;
using Xunit;
using Xunit.Microsoft.DependencyInjection.Attributes;

namespace Build.Test
{
    [Collection("Assets"), TestCaseOrder(1)]
    public class Assets
    {
        [Fact, TestOrder(1)]
        public void GenerateMaterials()
        {
            try
            {
                Nuke.Common.Tools.Source.Tasks.Materials(options =>
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
                Assert.False(true, process != null ? string.Join(Environment.NewLine, process?.Output.ToArray()) : pe.Message);
            }
            catch (Exception e)
            {
                Assert.False(true, e.Message);
            }
        }

        [Fact, TestOrder(2)]
        public void GenerateModels()
        {
            try
            {
                Nuke.Common.Tools.Source.Tasks.Models(options =>
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
                Assert.False(true, process != null ? string.Join(Environment.NewLine, process?.Output.ToArray()) : pe.Message);
            }
            catch (Exception e)
            {
                Assert.False(true, e.Message);
            }
        }
    }
}
