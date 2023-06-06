// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Source.Interfaces;

namespace Nuke.Common.Tools.Source.Tooling
{
    /// <summary>
    /// https://developer.valvesoftware.com/wiki/Studiomdl
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class QCC : Tools
    {
        public QCC() : base("studiomdl.exe")
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected override Arguments ConfigureProcessArguments(Arguments arguments)
        {
            arguments
                // General
                .Add("-game {value}", Game)
                .Add("-quiet", Verbose)
                .Add("-x360", X360)
                .Add("-nox360", NoX360)
                .Add("-nowarnings", NoWarnings)
                .Add("-maxwarnings {value}", MaxWarnings)
                // Animation
                .Add("-definebones", DefineBones)
                .Add("-printbones", PrintBones)
                .Add("-printgraph", PrintGraph)
                .Add("-overridedefinebones", OverrideDefineBones)
                .Add("-checklengths", CheckLengths)
                // Performance
                .Add("-fastbuild", FastBuild)
                .Add("-preview", Preview)
                .Add("-fullcollide", FullCollide)
                .Add("-striplods", StripLods)
                .Add("-minlod {value}", MinLod)
                .Add("-mdlreport", MdlReport)
                .Add("-perf", Perf)
                .Add("-mdlreportspreadsheet", MdlReportSpreadSheet)
                // Debug
                .Add("-d", Dumps)
                .Add("-h", HitBoxes)
                .Add("-n", BadNormals)
                .Add("-dumpmaterials", DumpMaterials)
                .Add("-i", IgnoreWarning)
                .Add("-t", ReplaceAllMaterials)
                .Add("-parsecompletion", ParseCompletion)
                .Add("-collapsereport", CollapseReport)
                // Other
                .Add("-nop4", NoP4)
                .Add("-verify", Verify)
                .Add("-a {value}", NormalBlendAngle)
                .Add("-vsi {value}", VSI)
                .Add("-stripmodel {value}", StripModel)
                .Add("-stripvhv {value}", StripVHV)
                .Add("-makefile", MakeFile)
                .Add("-basedir {value}", BaseDir)
                .Add("-tempcontent {value}", TempContent)
                // NonFunctional
                .Add("-t", TagReversed)
                .Add("-ihvtest", IHVTest)
                //
                .Add("{value}", Input);
            return base.ConfigureProcessArguments(arguments);
        }

        #region General

        /// <summary>
        /// Suppresses some console output, such as spewing the searchpaths.
        /// </summary>
        public virtual bool? Quiet => !Verbose;

        /// <summary>
        /// Enable Xbox 360 output, overriding the Gameinfo.txt
        /// </summary>
        public virtual bool? X360 { get; internal set; }

        /// <summary>
        /// Disable Xbox 360 output, overriding the Gameinfo.txt
        /// </summary>
        public virtual bool? NoX360 { get; internal set; }

        /// <summary>
        /// Disable warnings.
        /// </summary>
        public virtual bool? NoWarnings { get; internal set; }

        /// <summary>
        /// Print no more than the specified number of warnings.
        /// </summary>
        public virtual int? MaxWarnings { get; internal set; }

        #endregion

        #region Animation

        /// <summary>
        /// See $definebone.
        /// </summary>
        public virtual bool? DefineBones { get; internal set; }

        /// <summary>
        /// Writes extra bone info to the console.
        /// </summary>
        public virtual bool? PrintBones { get; internal set; }

        /// <summary>
        /// Todo: Appears to dump xnode data for each node?
        /// </summary>
        public virtual bool? PrintGraph { get; internal set; }

        /// <summary>
        /// Equivalent to specifying $unlockdefinebones in QC.
        /// </summary>
        public virtual bool? OverrideDefineBones { get; internal set; }

        /// <summary>
        /// Prints engine-ready keyframe data for each animation.
        /// </summary>
        public virtual bool? CheckLengths { get; internal set; }

        #endregion

        #region Performance

        /// <summary>
        /// Skip processing DX7, DX8, X360, and software VTX variants (use .dx90.vtx only). This speeds up compiling.
        /// </summary>
        public virtual bool? FastBuild { get; internal set; }

        /// <summary>
        /// Skip splitting quads into tris. This changes the rendering flags for the model, most likely resulting in slower performance or buggier rendering in-engine.
        /// </summary>
        public virtual bool? Preview { get; internal set; }

        /// <summary>
        /// Don't truncate really big collision meshes (in all games since 07, use $maxconvexpieces).
        /// </summary>
        public virtual bool? FullCollide { get; internal set; }

        /// <summary>
        /// Ignore all $lod commands.
        /// </summary>
        public virtual bool? StripLods { get; internal set; }

        /// <summary>
        /// Throw away data from LODs above the given one (see $minlod).
        /// </summary>
        public virtual int? MinLod { get; internal set; }

        /// <summary>
        /// Report performance info for an already-compiled model. A QC file is not needed when using this command.
        /// </summary>
        public virtual string MdlReport { get; internal set; }

        /// <summary>
        /// Same as -mdlreport.
        /// </summary>
        public virtual string Perf { get; internal set; }

        /// <summary>
        /// Report performance info, per-LOD, as a comma-delimited spreadsheet. It will appear in the form:
        /// </summary>
        public virtual bool? MdlReportSpreadSheet { get; internal set; }

        #endregion

        #region Debug

        /// <summary>
        /// Dumps various glview files (10 per LOD per VTX file),
        /// </summary>
        public virtual bool? Dumps { get; internal set; }

        /// <summary>
        /// Dump hitboxes to console.
        /// </summary>
        public virtual bool? HitBoxes { get; internal set; }

        /// <summary>
        /// Tag bad normals.
        /// </summary>
        public virtual bool? BadNormals { get; internal set; }

        /// <summary>
        /// Dump names of used materials to the console.
        /// </summary>
        public virtual bool? DumpMaterials { get; internal set; }

        /// <summary>
        /// Ignore warnings.
        /// </summary>
        public virtual bool? IgnoreWarning { get; internal set; }

        /// <summary>
        /// Replaces all materials with the default pink check pattern
        /// </summary>
        public virtual bool? ReplaceAllMaterials { get; internal set; }

        /// <summary>
        /// Prints an easily parseable message indicating whether the compile was successful or a failure.
        /// </summary>
        public virtual bool? ParseCompletion { get; internal set; }

        /// <summary>
        /// Prints info on which bones are being retained and which bones are being collapsed.
        /// </summary>
        public virtual bool? CollapseReport { get; internal set; }

        #endregion

        #region Other

        /// <summary>
        /// Disables Valve's Perforce integration. Unless you actually have Perforce set up for your game/mod (a highly unlikely scenario), you should use this.
        /// </summary>
        public virtual bool? NoP4 { get; internal set; }

        /// <summary>
        /// Compile the model, but don't actually write the results to disk.
        /// </summary>
        public virtual bool? Verify { get; internal set; }

        ///Auto-smooth faces equal to or below specified angle. Will override normal data for all meshes.
        public virtual decimal? NormalBlendAngle { get; internal set; }

        /// <summary>
        /// Generates a VSI file from a QC or MDL.
        /// </summary>
        public virtual string VSI { get; internal set; }

        /// <summary>
        /// Strips down a model, removing its LOD info.
        /// </summary>
        public virtual string StripModel { get; internal set; }

        /// <summary>
        /// Strips down hardware verts (VHV) of their LOD info.
        /// </summary>
        public virtual string StripVHV { get; internal set; }

        /// <summary>
        /// Generates a simple makefile for later compiling. It also parses the QC for any errors, and runs in -quiet mode.
        /// </summary>
        public virtual bool? MakeFile { get; internal set; }

        /// <summary>
        /// Runs studiomdl in the context of the provided path.
        /// </summary>
        public virtual string BaseDir { get; internal set; }

        /// <summary>
        /// Adds the provided path as a content search path.
        /// </summary>
        public virtual string TempContent { get; internal set; }

        #endregion

        #region NonFunctional

        /// <summary>
        /// "Tag reversed".
        /// </summary>
        public virtual bool? TagReversed { get; internal set; }

        /// <summary>
        /// Probably meant to test the model collision for any physics errors. Skips whatever argument is provided after it.
        /// </summary>
        public virtual bool? IHVTest { get; internal set; }

        #endregion
    }

    public static partial class Extensions
    {
        #region NoP4

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <param name="nop4"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T SetNoP4<T>(this T toolSettings, bool? nop4) where T : QCC
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.NoP4 = nop4;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T ResetNoP4<T>(this T toolSettings) where T : QCC
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.NoP4 = null;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T EnableNoP4<T>(this T toolSettings) where T : QCC
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.NoP4 = true;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T DisableNoP4<T>(this T toolSettings) where T : QCC
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.NoP4 = false;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T ToggleNoP4<T>(this T toolSettings) where T : QCC
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.NoP4 = !toolSettings.NoP4;
            return toolSettings;
        }

        #endregion
    }
}
