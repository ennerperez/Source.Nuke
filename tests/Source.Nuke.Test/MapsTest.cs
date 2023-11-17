using System;
using System.IO;
using Nuke.Source.Enums;
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
                    options.File = Path.Combine("Assets", "maps", "sdk_background.vmf");
                    options.Configuration = Configuration.Fast;
                });
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.False(true, e.GetMessage());
            }
        }

        [Fact, TestOrder(2)]
        public void BuildNormalMap()
        {
            try
            {
                Nuke.Source.Tasks.Maps(options =>
                {
                    options.AppId = 243750;
                    options.Verbose = true;
                    options.GameName = "hl2mp";
                    options.DepotDirectory = "depots";
                    options.File = Path.Combine("Assets", "maps", "sdk_background.vmf");
                    options.Configuration = Configuration.Normal;
                });
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.False(true, e.GetMessage());
            }
        }

        [Fact, TestOrder(3)]
        public void BuildPublishMap()
        {
            try
            {
                Nuke.Source.Tasks.Maps(options =>
                {
                    options.AppId = 243750;
                    options.Verbose = true;
                    options.GameName = "hl2mp";
                    options.DepotDirectory = "depots";
                    options.File = Path.Combine("Assets", "maps", "sdk_background.vmf");
                    options.Configuration = Configuration.Publish;
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
