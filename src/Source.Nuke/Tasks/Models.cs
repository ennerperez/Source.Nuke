using System;
using System.IO;
using System.Linq;
using Nuke.Common.Tooling;
using Nuke.Source.Tooling;

namespace Nuke.Source
{
    public static partial class Tasks
    {
        public class ModelsOptions
        {
            public string Folder { get; set; }

            public string GameDirectory => Path.Combine(DepotDirectory, AppId.ToString(), GameName);
            public string InstallDirectory => Path.Combine(DepotDirectory, AppId.ToString(), "bin");
            public bool Verbose { get; set; }
            public string DepotDirectory { get; set; }
            public string GameName { get; set; }
            public long AppId { get; set; }
        }

        // ReSharper disable once CognitiveComplexity
        public static void Models(Action<ModelsOptions> options)
        {
            var op = new ModelsOptions();
            options?.Invoke(op);

            var files = Directory.GetFiles(op.Folder, "*.qc", SearchOption.AllDirectories)
                .Select(m => new FileInfo(m));

            var outputPath = Path.Combine(op.GameDirectory, "models");
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            var installDirectory = op.InstallDirectory;
            var relativeOutputPath = Path.GetRelativePath(installDirectory, outputPath);

            foreach (var file in files)
            {
                Abstractions.Tasks.Source(_ => new QCC()
                    .SetVerbose(op.Verbose)
                    .EnableNoP4()
                    .SetProcessWorkingDirectory(installDirectory)
                    .SetInstallDir(installDirectory)
                    .SetAppId(op.AppId)
                    .SetGamePath(Path.GetFullPath(op.GameDirectory))
                    .SetInput(file.FullName)
                    .SetOutput(relativeOutputPath)
                );
            }
        }
    }
}
