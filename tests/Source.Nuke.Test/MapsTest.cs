using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Nuke.Common.Tooling;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Source.Nuke.Test
{
    [Collection("Maps"), Order(3)]
    public class MapsTest
    {
        [Fact, Order(1)]
        public void BuildFastMap()
        {
            try
            {
                global::Nuke.Common.Tools.Source.Tasks.Maps(options =>
                {
                    options.AppId = 243750;
                    options.Verbose = true;
                    options.GameName = "hl2mp";
                    options.DepotDirectory = "depots";
                    options.File = Path.Combine("assets", "maps", "sdk_background.vmf");
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
