using System;
using System.Data;
using System.IO;
using System.Linq;
using Nuke.Source.Enums;
using Nuke.Source.Tooling;

namespace Nuke.Source
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
            public int AppId { get; set; }
            public Configuration Configuration { get; set; }
        }

        // ReSharper disable once CognitiveComplexity
        public static void Maps(Action<MapsOptions> options)
        {
            var op = new MapsOptions();
            options?.Invoke(op);

            Abstractions.Tasks.Source(_ => new VBSP()
                //.SetProcessWorkingDirectory(op.InstallDirectory)
                .SetVerbose(op.Verbose)
                .SetAppId(op.AppId)
                .SetGamePath(Path.GetFullPath(op.GameDirectory))
                .SetInstallDir(op.InstallDirectory)
                //
                .SetInput(op.File)
                .EnableUsingSlammin()
            );

            var bspFile = Path.ChangeExtension(op.File, "bsp");
            Abstractions.Tasks.Source(_ => new VVIS()
                //.SetProcessWorkingDirectory(op.InstallDirectory)
                .SetVerbose(op.Verbose)
                .SetAppId(op.AppId)
                .SetGamePath(Path.GetFullPath(op.GameDirectory))
                .SetInstallDir(op.InstallDirectory)
                //
                .SetFast(op.Configuration == Configuration.Fast)
                .SetNoSort(op.Configuration == Configuration.Fast)
                //
                .SetInput(bspFile)
                .EnableUsingSlammin()
            );

            Abstractions.Tasks.Source(_ => new VRAD()
                //.SetProcessWorkingDirectory(op.InstallDirectory)
                .SetVerbose(op.Verbose)
                .SetAppId(op.AppId)
                .SetGamePath(Path.GetFullPath(op.GameDirectory))
                .SetInstallDir(op.InstallDirectory)
                //
                .SetFast(op.Configuration == Configuration.Fast)
                .SetBounce((ushort)(op.Configuration == Configuration.Fast ? 2 : 100))
                //
                .SetInput(bspFile)
                .EnableUsingSlammin()
            );

            if (op.Configuration != Configuration.Fast)
            {
                if (bspFile == null) throw new FileNotFoundException();
                var mapDir = Path.Combine(op.GameDirectory, "maps");
                var mapName = Path.GetFileNameWithoutExtension(bspFile);
                if (!Directory.Exists(mapDir)) Directory.CreateDirectory(mapDir);
                var bspGameTargetFile = Path.Combine(mapDir, Path.GetFileName(bspFile));

                var initialSize = new FileInfo(bspFile).Length;
                File.Move(bspFile, bspGameTargetFile, true);

                Abstractions.Tasks.Source(_ => new CUBEMAP()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    //
                    .SetInput(mapName)
                );

                var finalSize = new FileInfo(bspGameTargetFile).Length;
                if (initialSize == finalSize) throw new EvaluateException("File have the same size, changes not detected");
                File.Move(bspGameTargetFile, bspFile, true);
            }

            if (op.Configuration == Configuration.Publish)
            {
                if (bspFile == null) throw new FileNotFoundException();
                var mapDir = Path.Combine(op.GameDirectory, "maps");
                //var mapName = Path.GetFileNameWithoutExtension(bspFile);
                if (!Directory.Exists(mapDir)) Directory.CreateDirectory(mapDir);
                var bspGameTargetFile = Path.Combine(mapDir, Path.GetFileName(bspFile));

                //var initialSize = new FileInfo(bspFile).Length;
                File.Move(bspFile, bspGameTargetFile, true);

                var bspzipLogs = Path.ChangeExtension(bspGameTargetFile, "log");
                Abstractions.Tasks.Source(_ => new BSPZIP()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    //
                    .SetInput(bspGameTargetFile)
                    //
                    .SetStdOutput(bspzipLogs)
                    .SetCallback(() =>
                    {
                        var content = File.ReadAllLines(bspzipLogs).Skip(3).Where(s => !s.EndsWith(".vhv"));
                        File.WriteAllLines(bspzipLogs, content);
                    })
                );

                Abstractions.Tasks.Source(_ => new BSPZIP()
                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInstallDir(op.InstallDirectory)
                    //
                    .SetInput(bspGameTargetFile)
                    //
                    .SetFileList(bspzipLogs)
                    .SetCallback(() =>
                    {
#if !DEBUG
						File.Delete(bspzip_logs);
#endif
                        File.Move(Path.ChangeExtension(bspGameTargetFile, "bzp"), Path.ChangeExtension(bspGameTargetFile, "bsp"), true);
                    })
                );

                //var finalSize = new FileInfo(bspGameTargetFile).Length;
                //if (initialSize == finalSize) throw new EvaluateException("File have the same size, changes not detected");
                File.Move(bspGameTargetFile, bspFile, true);
            }
        }
    }
}
