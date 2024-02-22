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

            public List<string> GetSourceDirectories(bool verbose = true)
            {
                var sourceDirectories = new List<string>();
                var gameInfoPath = Path.Combine(GameDirectory, "gameinfo.txt");
                var rootPath = Directory.GetParent(GameDirectory).ToString();

                if (!System.IO.File.Exists(gameInfoPath))
                {
                    Trace.TraceError($"Couldn't find gameinfo.txt at {gameInfoPath}");
                    return new();
                }

                var fileStream = System.IO.File.OpenRead(gameInfoPath);
                var gameInfo = KVSerializer.Create(KVSerializationFormat.KeyValues1Text).Deserialize(fileStream);

                //var gameInfo = new KV.FileData(gameInfoPath).headnode.GetFirstByName("GameInfo");
                if (gameInfo == null)
                {
                    Trace.TraceInformation($"Failed to parse GameInfo: {gameInfo}");
                    Trace.TraceError($"Failed to parse GameInfo, did not find GameInfo block");
                    return new();
                }

                //var searchPaths = gameInfo.GetFirstByName("FileSystem")?.GetFirstByName("SearchPaths");
                var searchPaths = gameInfo["FileSystem"]?["SearchPaths"];
                if (searchPaths == null)
                {
                    Trace.TraceInformation($"Failed to parse GameInfo: {gameInfo}");
                    Trace.TraceError($"Failed to parse GameInfo, did not find GameInfo block");
                    return new();
                }

                var collection = searchPaths.AsEnumerable<KVObject>().Select(m => new KeyValuePair<string, string>(m.Name, m.Value.ToString()));
                foreach (var searchPath in collection)
                {
                    // ignore unsearchable paths. TODO: will need to remove .vpk from this check if we add support for packing from assets within vpk files
                    if (searchPath.Value.Contains("|") && !searchPath.Value.Contains("|gameinfo_path|") || searchPath.Value.Contains(".vpk")) continue;

                    // wildcard paths
                    if (searchPath.Value.Contains("*"))
                    {
                        var fullPath = searchPath.Value;
                        if (fullPath.Contains(("|gameinfo_path|")))
                        {
                            var newPath = searchPath.Value.Replace("*", "").Replace("|gameinfo_path|", "");
                            fullPath = Path.GetFullPath(GameDirectory + "\\" + newPath.TrimEnd('\\'));
                        }
                        if (Path.IsPathRooted(fullPath.Replace("*", "")))
                        {
                            fullPath = fullPath.Replace("*", "");
                        }
                        else
                        {
                            var newPath = fullPath.Replace("*", "");
                            fullPath = Path.GetFullPath(rootPath + "\\" + newPath.TrimEnd('\\'));
                        }

                        if (verbose)
                            Trace.TraceInformation("Found wildcard path: {0}", fullPath);

                        try
                        {
                            var directories = Directory.GetDirectories(fullPath);
                            sourceDirectories.AddRange(directories);
                        }
                        catch { }
                    }
                    else if (searchPath.Value.Contains("|gameinfo_path|"))
                    {
                        var fullPath = GameDirectory;

                        if (verbose)
                            Trace.TraceInformation("Found search path: {0}", fullPath);

                        sourceDirectories.Add(fullPath);
                    }
                    else if (Directory.Exists(searchPath.Value))
                    {
                        if (verbose)
                            Trace.TraceInformation("Found search path: {0}", searchPath);

                        sourceDirectories.Add(searchPath.Value);
                    }
                    else
                    {
                        try
                        {
                            var fullPath = Path.GetFullPath(rootPath + "\\" + searchPath.Value.TrimEnd('\\'));

                            if (verbose)
                                Trace.TraceInformation("Found search path: {0}", fullPath);

                            sourceDirectories.Add(fullPath);
                        }
                        catch (Exception e)
                        {
                            Trace.TraceInformation("Failed to find search path: " + e);
                            Trace.TraceWarning($"Search path invalid: {rootPath + "\\" + searchPath.Value.TrimEnd('\\')}");
                        }
                    }
                }

                // find Chaos engine game mount paths
                // var mountedDirectories = GetMountedGamesSourceDirectories(gameInfo, Path.Combine(GameDirectory, "cfg", "mounts.kv"));
                // if (mountedDirectories != null)
                // {
                //     sourceDirectories.AddRange(mountedDirectories);
                //     foreach (var directory in mountedDirectories)
                //     {
                //         Trace.TraceInformation($"Found mounted search path: {directory}");
                //     }
                // }

                return sourceDirectories.Distinct().ToList();
            }

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
                    .SetInput(Path.GetFullPath(bspGameTargetFile))
                    .SetOutputDir(unpackDir)
                    .SetInput(bspGameTargetFile)
                );
                var bspFileData = new BSP(new FileInfo(bspGameTargetFile), Path.GetFullPath(op.GameDirectory));
                bspFileData.findBspPakDependencies(unpackDir);

                var sourceDirectories = new List<string>();
                var sourceDirs = op.GetSourceDirectories(true);
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
