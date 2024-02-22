using Build.Test.Fixtures;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Source;
using Nuke.Common.Tools.Source.Formats;
using Nuke.Common.Tools.Source.Tooling;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;
using Xunit.Sdk;

namespace Source.Nuke.Test
{
    [Collection("Tools"), Order(3)]
    public class ToolsTest : Abstracts.Test
    {

        public ToolsTest([NotNull] ITestOutputHelper testOutputHelper, [NotNull] TestFixture fixture) : base(testOutputHelper, fixture)
        {
        }

        private Tasks.MapsOptions op = new Tasks.MapsOptions()
        {
            AppId = 243750,
            Verbose = true,
            GameName = "hl2mp",
            DepotDirectory = "depots",
            File = Path.Combine("assets", "maps", "sdk_background.vmf"),
        };

        [Theory, Order(2)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        public void VBSP(bool fast, bool slammin)
        {
            try
            {
                Tasks.Source(_ => new VBSP()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    //
                    .SetFast(fast)
                    //
                    .SetInput(op.File)
                    .SetUsingSlammin(slammin)
                );

                var bspFile = Path.ChangeExtension(op.File, "bsp");
                if (string.IsNullOrWhiteSpace(bspFile) || !File.Exists(bspFile))
                    throw new FileNotFoundException(bspFile);

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

        [Theory, Order(3)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        public void VVIS(bool fast, bool slammin)
        {
            try
            {
                var bspFile = Path.ChangeExtension(op.File, "bsp");

                Tasks.Source(_ => new VVIS()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    //
                    .SetFast(fast)
                    .SetNoSort(fast)
                    //
                    .SetInput(bspFile)
                    .SetUsingSlammin(slammin)
                );

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

        [Theory, Order(4)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        public void VRAD(bool fast, bool slammin)
        {
            try
            {
                var bspFile = Path.ChangeExtension(op.File, "bsp");

                Tasks.Source(_ => new VRAD()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    //
                    .SetFast(op.Fast)
                    .SetBounce((ushort)(op.Fast ? 2 : 100))
                    //
                    .SetInput(bspFile)
                    .SetUsingSlammin(slammin)
                );

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

        [Fact, Order(5)]
        public void CUBEMAP()
        {
            try
            {
                var bspFile = Path.ChangeExtension(op.File, "bsp");

                var mapDir = Path.Combine(op.GameDirectory, "maps");
                if (!Directory.Exists(mapDir)) Directory.CreateDirectory(mapDir);
                var bspGameTargetFile = Path.Combine(mapDir, Path.GetFileName(bspFile) ?? throw new InvalidOperationException());
                File.Copy(bspFile, bspGameTargetFile, true);

                Tasks.Source(_ => new CUBEMAP()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    .SetInput(bspGameTargetFile)
                );

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

        [Fact, Order(6)]
        public void UNPACK()
        {
            try
            {
                var bspFile = Path.ChangeExtension(op.File, "bsp");

                var mapDir = Path.Combine(op.GameDirectory, "maps");
                if (!Directory.Exists(mapDir)) Directory.CreateDirectory(mapDir);
                var bspGameTargetFile = Path.Combine(mapDir, Path.GetFileName(bspFile) ?? throw new InvalidOperationException());
                File.Copy(bspFile, bspGameTargetFile, true);

                var unpackDir = Path.GetTempPath() + Guid.NewGuid();
                Tasks.Source(_ => new UNPACK()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    .SetInput(Path.GetFullPath(bspGameTargetFile))
                    .SetOutputDir(unpackDir)
                    .SetInput(bspGameTargetFile)
                );

                if (!Directory.Exists(unpackDir))
                    throw new DirectoryNotFoundException(unpackDir);

                var bspFileData = new BSP(new FileInfo(bspGameTargetFile), Path.GetFullPath(op.GameDirectory));
                bspFileData.findBspPakDependencies(unpackDir);

                if (!bspFileData.TextureList.Any())
                    throw new Exception("No elements in textures list");

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
