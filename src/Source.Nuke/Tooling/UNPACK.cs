// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using Nuke.Common.Tooling;

namespace Nuke.Common.Tools.Source.Tooling
{
    /// <summary>
    /// https://developer.valvesoftware.com/wiki/BSPZIP
    /// </summary>
    [PublicAPI]
    [Serializable]
    public class UNPACK : Tools
    {

        public UNPACK() : base("bspzip.exe")
        {

        }

        public string OutputDir { get; internal set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected override Arguments ConfigureProcessArguments(Arguments arguments)
        {
            arguments
                .Add("-extractfiles {value}", Input)
                .Add("{value}", OutputDir);
            return base.ConfigureProcessArguments(arguments);
        }
    }

    public static partial class Extensions
    {

        #region OutputDir

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <param name="outputDir"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T SetOutputDir<T>(this T toolSettings, string outputDir) where T : UNPACK
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.OutputDir = outputDir;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T ResetOutputDir<T>(this T toolSettings) where T : UNPACK
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.OutputDir = null;
            return toolSettings;
        }

        #endregion
    }
}
