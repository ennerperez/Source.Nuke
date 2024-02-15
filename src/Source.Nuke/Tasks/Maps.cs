using Nuke.Common.Tooling;
using System;
using System.IO;
using Nuke.Common.Tools.Source.Tooling;
using System.Collections.Generic;
using System.Linq;

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
                .EnableUsingSlammin()
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
                            content = o.Select(m=> m.Text).Skip(3).Where(s => !s.EndsWith(".vhv"));
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
    }
}
