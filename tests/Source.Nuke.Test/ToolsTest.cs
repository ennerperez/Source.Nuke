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
using System.Diagnostics;
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
            Fast = true,
            Slammin = true
        };

        [Fact, Order(1)]
        public void VBSP()
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
                    .SetFast(op.Fast)
                    //
                    .SetInput(op.File)
                    .SetUsingSlammin(op.Slammin)
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

        [Fact, Order(2)]
        public void VVIS()
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
                    .SetFast(op.Fast)
                    .SetNoSort(op.Fast)
                    //
                    .SetInput(bspFile)
                    .SetUsingSlammin(op.Slammin)
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

        [Fact, Order(3)]
        public void VRAD()
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
                    .SetUsingSlammin(op.Slammin)
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

        [Fact, Order(4)]
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

        [Fact, Order(5)]
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
                    .SetOutputDir(unpackDir)
                    .SetInput(bspGameTargetFile)
                );

                if (!Directory.Exists(unpackDir))
                    throw new DirectoryNotFoundException(unpackDir);

                Tasks.Source(_ => new PACK()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    .SetInput(bspGameTargetFile)
                    .SetRenameNav(true)
                    .SetGenParticleManifest(true)
                    .SetMethod((t) =>
                    {
                        var bspFileData = new BSP(new FileInfo(t.Input), t.Game)
                        {
                            GenParticleManifest = (t as PACK).GenParticleManifest,
                            RenameNav = (t as PACK).RenameNav,
                            Verbose = t.Verbose ?? false
                        };
                        bspFileData.findBspUtilityFiles(op.DepotDirectory, t.AppId, op.GameName);
                        bspFileData.findBspPakDependencies(unpackDir);

                        if (!bspFileData.TextureList.Any())
                            throw new Exception("No elements in textures list");

                        Trace.TraceInformation("Initializing pak file...");

                        return null;
                    })
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

    }
}
