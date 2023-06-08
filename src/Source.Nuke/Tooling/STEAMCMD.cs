// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Net;
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
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class STEAMCMD : Tools, IDownloadable, ICredential
    {
        public STEAMCMD() : base("steamcmd.exe")
        {
        }

        public override string ProcessToolPath => Path.Combine(InstallDir, Executable);

        /// <summary>
        /// Overwrite existing output files
        /// </summary>
        public virtual bool? Force { get; internal set; }

        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }

        public virtual bool? Validate { get; internal set; }
        public virtual string InstallDir { get; set; }

        /// <summary>
        /// Keep (don't delete) input files
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected override Arguments ConfigureProcessArguments(Arguments arguments)
        {
            arguments
                .Add("+force_install_dir {value}", AppId.ToString())
                .Add("+login {value}", UserName)
                .Add("{value}", Password)
                .Add("+app_update {value}", AppId)
                .Add("+validate", Validate)
                .Add("+quit");
            return base.ConfigureProcessArguments(arguments);
        }

        public string Url => "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";

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
        #region Validate

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <param name="validate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T SetValidate<T>(this T toolSettings, bool? validate) where T : STEAMCMD
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Validate = validate;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T ResetValidate<T>(this T toolSettings) where T : STEAMCMD
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Validate = null;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T EnableValidate<T>(this T toolSettings) where T : STEAMCMD
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Validate = true;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T DisableValidate<T>(this T toolSettings) where T : STEAMCMD
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Validate = false;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T ToggleValidate<T>(this T toolSettings) where T : STEAMCMD
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Validate = !toolSettings.Validate;
            return toolSettings;
        }

        #endregion
    }
}
