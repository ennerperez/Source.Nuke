﻿// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Source.Interfaces;

namespace Nuke.Common.Tools.Source.Tooling
{
    /// <summary>
    ///
    /// </summary>
    [PublicAPI]
    [Serializable]
    public class DEPOTDOWNLOADER : Tools, IDownloadable, ICredential
    {
        public DEPOTDOWNLOADER() : base("DepotDownloader.exe")
        {
        }

        public override string ProcessToolPath => Path.Combine(InstallDir, Executable);

        /// <summary>
        /// Overwrite existing output files
        /// </summary>
        public virtual bool? Force { get; internal set; }

        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }

        /// <summary>
        /// Keep (don't delete) input files
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected override Arguments ConfigureProcessArguments(Arguments arguments)
        {
            arguments
                .Add("-username {value}", UserName)
                .Add("-password {value}", Password)
                .Add("-app {value}", AppId)
                .Add("-dir {value}", Path.Combine(InstallDir, AppId.ToString()));
            return base.ConfigureProcessArguments(arguments);
        }

        public string Url => "https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_2.5.0/depotdownloader-2.5.0.zip";

        // ReSharper disable once CognitiveComplexity
        public bool Download()
        {
            var localFile = string.Empty;
            var localDir = string.Empty;
            var fileName = Path.GetFileName(Url);
            if (fileName != null)
            {
                var toolPath = Path.GetDirectoryName(ProcessToolPath);
                if (string.IsNullOrWhiteSpace(toolPath)) return false;
                if (!Directory.Exists(toolPath)) Directory.CreateDirectory(toolPath);
                if (!string.IsNullOrWhiteSpace(toolPath))
                {
                    localFile = Path.Combine(toolPath, fileName);
                    localDir = toolPath;
                }

                if (string.IsNullOrWhiteSpace(localFile)) return false;
                if (!File.Exists(localFile))
                {
                    using (var client = new HttpClient())
                    {
                        var response = client.Send(new HttpRequestMessage(HttpMethod.Get, Url));
                        using var resultStream = response.Content.ReadAsStream();
                        using var fileStream = File.OpenWrite(localFile);
                        resultStream.CopyTo(fileStream);
                    }
                }

                if (File.Exists(localFile) && !File.Exists(Path.Combine(localDir, Executable)))
                    ZipFile.ExtractToDirectory(localFile, localDir, true);
            }

            return !string.IsNullOrWhiteSpace(localFile) && File.Exists(localFile) &&
                   !string.IsNullOrWhiteSpace(localDir) && Directory.Exists(localDir) &&
                   File.Exists(Path.Combine(localDir, Executable));
        }
    }

    public static partial class Extensions
    {
    }
}
