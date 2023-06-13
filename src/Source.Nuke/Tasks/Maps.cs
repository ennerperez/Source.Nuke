using System;
using System.IO;
using Nuke.Common.Tools.Source.Tooling;

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
            public int AppId { get; set; }
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
                .SetInput(op.File)
                .EnableUsingSlammin()
            );

            var bspFile = Path.ChangeExtension(op.File, "bsp");
            Source(_ => new VVIS()
                //.SetProcessWorkingDirectory(op.InstallDirectory)
                .SetVerbose(op.Verbose)
                .SetAppId(op.AppId)
                .SetGamePath(Path.GetFullPath(op.GameDirectory))
                .SetInstallDir(op.InstallDirectory)
                //
                .EnableFast()
                .EnableNoSort()
                //
                .SetInput(bspFile)
                .EnableUsingSlammin()
            );

            Source(_ => new VRAD()
                //.SetProcessWorkingDirectory(op.InstallDirectory)
                .SetVerbose(op.Verbose)
                .SetAppId(op.AppId)
                .SetGamePath(Path.GetFullPath(op.GameDirectory))
                //
                .EnableFast()
                .SetBounce(2)
                //
                .SetInput(bspFile)
                .EnableUsingSlammin()
            );
        }
    }
}
