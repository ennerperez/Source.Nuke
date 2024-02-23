using Microsoft.Win32;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Source.Formats;
using System;
using System.IO;
using Nuke.Common.Tools.Source.Tooling;
using Nuke.Common.Utilities.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ValveKeyValue;

namespace Nuke.Common.Tools.Source
{
    public static partial class Tasks
    {
        public class MapsOptions
        {
            public string File { get; set; }
            public string GameDirectory => Path.Combine(DepotDirectory, AppId.ToString(), GameName);
            public string InstallDirectory => Path.Combine(DepotDirectory, AppId.ToString(), "bin");
            public bool Verbose { get; set; }
            public string DepotDirectory { get; set; }
            public string GameName { get; set; }
            public long AppId { get; set; }
            public bool Fast { get; set; }
            public bool Slammin { get; set; } = true;

        }

        // ReSharper disable once CognitiveComplexity
        public static void Maps(Action<MapsOptions> options)
        {
            var op = new MapsOptions();
            options?.Invoke(op);

            Source(_ => new VBSP()
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

            Source(_ => new VVIS()
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
                .EnableUsingSlammin()
            );

            Source(_ => new VRAD()
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
                .EnableUsingSlammin()
            );

            if (!op.Fast)
            {
                var mapDir = Path.Combine(op.GameDirectory, "maps");
                if (!Directory.Exists(mapDir)) Directory.CreateDirectory(mapDir);
                var bspGameTargetFile = Path.Combine(mapDir, Path.GetFileName(bspFile) ?? throw new InvalidOperationException());
                File.Move(bspFile, bspGameTargetFile, true);

                Source(_ => new CUBEMAP()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    .SetInput(bspGameTargetFile)
                );

                var unpackDir = Path.GetTempPath() + Guid.NewGuid();
                Source(_ => new UNPACK()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    .SetOutputDir(unpackDir)
                    .SetInput(bspGameTargetFile)
                );
                var bspFileData = new BSP(new FileInfo(bspGameTargetFile), Path.GetFullPath(op.GameDirectory));
                bspFileData.findBspPakDependencies(unpackDir);

                var sourceDirectories = new List<string>();

                // PAK.GetSourceDirectories();
                // var pakfile = new PAK(bspFileData, sourceDirectories, includeFiles, excludeFiles, excludeDirs,
                //     excludedVpkFiles, outputFile, noswvtx);


                var bspzipLogs = Path.ChangeExtension(bspGameTargetFile, "log");
                Source(_ => new PACK()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    .SetInput(Path.GetFullPath(bspGameTargetFile))
                    .SetCallback((o) =>
                    {
                        IEnumerable<string> content;
                        if (File.Exists(bspzipLogs))
                            content = File.ReadAllLines(bspzipLogs).Skip(3).Where(s => !s.EndsWith(".vhv"));
                        else
                            content = o.Select(m => m.Text).Skip(3).Where(s => !s.EndsWith(".vhv"));
                        File.WriteAllLines(Path.ChangeExtension(bspGameTargetFile, "tmp"), content);
                        File.Move(Path.ChangeExtension(bspGameTargetFile, "tmp"), Path.ChangeExtension(bspGameTargetFile, "log"), true);
                    })
                );

                Source(_ => new PACK()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    .SetInput(bspGameTargetFile)
                    .SetFileList(bspzipLogs)
                    .SetCallback((o) =>
                    {
#if !DEBUG
                        if (File.Exists(bspzipLogs)) File.Delete(bspzipLogs);
#endif
                        File.Move(Path.ChangeExtension(bspGameTargetFile, "bzp"), Path.ChangeExtension(bspGameTargetFile, "bsp"), true);
                    })
                );

                File.Move(bspGameTargetFile, bspFile, true);
            }
        }

        // public static void UnpackBSP(string bspPath, string unpackDir)
        // {
        //     // unpacks the pak file and extracts it to a temp location
        //
        //     /* info: vbsp.exe creates files in the pak file that may have
        //      * dependencies that are not listed anywhere else, as is the
        //      * case for water materials. We use this method to extract the
        //      * pak file to a temp folder and read the dependencies of its files. */
        //
        //     string arguments = "-extractfiles \"$bspold\" \"$dir\"";
        //     arguments = arguments.Replace("$bspold", bspPath);
        //     arguments = arguments.Replace("$dir", unpackDir);
        //
        //     var startInfo = new ProcessStartInfo(bspZip, arguments);
        //     startInfo.UseShellExecute = false;
        //     startInfo.CreateNoWindow = true;
        //     startInfo.RedirectStandardOutput = true;
        //     startInfo.EnvironmentVariables["VPROJECT"] = gameFolder;
        //
        //     var p = new Process { StartInfo = startInfo };
        //     p.Start();
        //     string output = p.StandardOutput.ReadToEnd();
        //
        //     p.WaitForExit();
        //
        // }
    }
}
