using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Nuke.Common.Tooling;
using Nuke.Source.Tooling;
using ValveKeyValue;

namespace Nuke.Source
{
    public static partial class Tasks
    {
        public class MaterialsOptions
        {
            public string Folder { get; set; }
            public string GameDirectory => Path.Combine(DepotDirectory, AppId.ToString(), GameName);
            public string InstallDirectory => Path.Combine(DepotDirectory, AppId.ToString(), "bin");
            public bool Verbose { get; set; }
            public string DepotDirectory { get; set; }
            public string Format { get; set; } = "tga";
            public string GameName { get; set; }
            public int AppId { get; set; }
        }

        // ReSharper disable once CognitiveComplexity
        public static void Materials(Action<MaterialsOptions> options)
        {
            var op = new MaterialsOptions();
            options?.Invoke(op);

            var decodeVmt = (string source) =>
            {
                var stream = File.OpenRead(source); // or any other Stream

                var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
                var data = kv.Deserialize(stream);
                return data;
            };

            var ext = op.Format;
            var source = from file in Directory.GetFiles(op.Folder, $"*.vmt", SearchOption.AllDirectories)
                select new {Definition = new FileInfo(file), Contents = File.Exists(file) ? decodeVmt(file) : null};

            var vtf = new VTF();
            foreach (var file in source)
            {
                if (file.Definition.Directory != null)
                {
                    var relative = Path.GetRelativePath(op.Folder, file.Definition.Directory.FullName);
                    var output = Path.Combine(op.GameDirectory, relative);
                    if (!Directory.Exists(output)) Directory.CreateDirectory(output);

                    var normalizePathFromContent = new Func<string, string>(r =>
                    {
                        if (string.IsNullOrWhiteSpace(r)) return r;
                        if (r.StartsWith("/")) r = r.Substring(1);
                        var x = Path.GetExtension(r);
                        if (!string.IsNullOrWhiteSpace(x))
                        {
                            var xl = x.Length;
                            var rl = r.Length;
                            r = r.Substring(0, rl - xl);
                        }

                        return r.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    });

                    var nomip = file.Contents?["$nomip"]?.ToString(CultureInfo.InvariantCulture);

                    var basetexture = file.Contents?["$basetexture"]?.ToString(CultureInfo.InvariantCulture);
                    var bumpmap = file.Contents?["$bumpmap"]?.ToString(CultureInfo.InvariantCulture);
                    var phong = file.Contents?["$phongexponenttexture"]?.ToString(CultureInfo.InvariantCulture);
                    var envmapmask = file.Contents?["envmapmask"]?.ToString(CultureInfo.InvariantCulture);

                    basetexture = normalizePathFromContent(basetexture);
                    bumpmap = normalizePathFromContent(bumpmap);
                    phong = normalizePathFromContent(phong);
                    envmapmask = normalizePathFromContent(envmapmask);

                    var outputPath = Path.Combine(op.GameDirectory, "materials", basetexture.Substring(0, basetexture.Length - Path.GetFileName(basetexture).Length));
                    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                    var inputPath = Path.Combine(op.Folder, "materials", $"{basetexture}.{ext}");

                    Abstractions.Tasks.Source(_ => vtf
                        .SetVerbose(op.Verbose)
                        .SetNoMipmaps(nomip == "1")
                        //.SetProcessWorkingDirectory(op.InstallDirectory)
                        .SetInstallDir(op.InstallDirectory)
                        .SetInput(inputPath)
                        .SetOutput(outputPath)
                        .SetCallback(() =>
                        {
                            var vtfFile = Path.Combine(op.GameDirectory, "materials", $"{basetexture}.vtf");
                            if (!File.Exists(vtfFile)) throw new FileNotFoundException(vtfFile);
                            if (!string.IsNullOrWhiteSpace(bumpmap))
                            {
                                Abstractions.Tasks.Source(_ => vtf
                                    .SetVerbose(op.Verbose)
                                    .SetNoMipmaps(nomip == "1")
                                    .EnableNormal()
                                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                                    .SetInstallDir(op.InstallDirectory)
                                    .SetInput(Path.Combine(op.Folder, "materials", $"{bumpmap}.{ext}"))
                                    .SetOutput(Path.Combine(op.GameDirectory, "materials"))
                                );
                            }

                            if (!string.IsNullOrWhiteSpace(phong))
                            {
                                Abstractions.Tasks.Source(_ => vtf
                                    .SetVerbose(op.Verbose)
                                    .SetNoMipmaps(nomip == "1")
                                    //.EnableNormal()
                                    //.SetProcessWorkingDirectory(op.InstallDirectory)
                                    .SetInstallDir(op.InstallDirectory)
                                    .SetInput(Path.Combine(op.Folder, "materials", $"{phong}.{ext}"))
                                    .SetOutput(Path.Combine(op.GameDirectory, "materials"))
                                );
                            }

                            if (!string.IsNullOrWhiteSpace(envmapmask))
                            {
                                Abstractions.Tasks.Source(_ => vtf
                                    .SetVerbose(op.Verbose)
                                    .SetNoMipmaps(nomip == "1")
                                    //.EnableNormal()
                                    .SetProcessWorkingDirectory(op.InstallDirectory)
                                    .SetInstallDir(op.InstallDirectory)
                                    .SetInput(Path.Combine(op.Folder, "materials", $"{envmapmask}.{ext}"))
                                    .SetOutput(Path.Combine(op.GameDirectory, "materials"))
                                );
                            }

                            if (file.Definition.Exists)
                                File.Copy(file.Definition.FullName, Path.Combine(output, file.Definition.Name), true);
                        })
                    );
                }
            }
        }
    }
}
