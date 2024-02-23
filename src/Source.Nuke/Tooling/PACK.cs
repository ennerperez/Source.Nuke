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
	public class PACK : Tools
	{

		public PACK() : base("bspzip.exe")
		{

		}

		public string FileList { get; internal set; }
		public bool RenameNav { get; internal set; }
		public bool GenParticleManifest { get; internal set; }

		/// <summary>
		///
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		protected override Arguments ConfigureProcessArguments(Arguments arguments)
		{
			if (string.IsNullOrWhiteSpace(FileList))
			{
				arguments
					//.Add("-verbose", Verbose)
					.Add("-dir {value}", Input);
			}
			else
			{
				arguments
					.Add("-addlist {value}", Input)
					.Add("{value}", FileList)
					.Add("{value}", Path.ChangeExtension(Input, "bzp"));
			}
			return base.ConfigureProcessArguments(arguments);
		}



	}

	public static partial class Extensions
	{
		#region FileList

		/// <summary>
		///
		/// </summary>
		/// <param name="toolSettings"></param>
		/// <param name="fileList"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[Pure]
		public static T SetFileList<T>(this T toolSettings, string fileList) where T : PACK
		{
			toolSettings = toolSettings.NewInstance();
			toolSettings.FileList = fileList;
			return toolSettings;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="toolSettings"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[Pure]
		public static T ResetFileList<T>(this T toolSettings) where T : PACK
		{
			toolSettings = toolSettings.NewInstance();
			toolSettings.FileList = null;
			return toolSettings;
		}

		#endregion

        #region RenameNav

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <param name="verbose"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T SetRenameNav<T>(this T toolSettings, bool renameNav) where T : PACK
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.RenameNav = renameNav;
            return toolSettings;
        }


		/// <summary>
		///
		/// </summary>
		/// <param name="toolSettings"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[Pure]
		public static T ResetRenameNav<T>(this T toolSettings) where T : PACK
		{
			toolSettings = toolSettings.NewInstance();
			toolSettings.RenameNav = false;
			return toolSettings;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="toolSettings"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[Pure]
		public static T EnableRenameNav<T>(this T toolSettings) where T : PACK
		{
			toolSettings = toolSettings.NewInstance();
			toolSettings.RenameNav = true;
			return toolSettings;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="toolSettings"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[Pure]
		public static T DisableRenameNav<T>(this T toolSettings) where T : PACK
		{
			toolSettings = toolSettings.NewInstance();
			toolSettings.RenameNav = false;
			return toolSettings;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="toolSettings"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[Pure]
		public static T ToggleRenameNav<T>(this T toolSettings) where T : PACK
		{
			toolSettings = toolSettings.NewInstance();
			toolSettings.RenameNav = !toolSettings.RenameNav;
			return toolSettings;
		}

        #endregion

        #region RenameNav

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <param name="verbose"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T SetGenParticleManifest<T>(this T toolSettings, bool genParticleManifest) where T : PACK
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.GenParticleManifest = genParticleManifest;
            return toolSettings;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T ResetGenParticleManifest<T>(this T toolSettings) where T : PACK
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.GenParticleManifest = false;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T EnableGenParticleManifest<T>(this T toolSettings) where T : PACK
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.GenParticleManifest = true;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T DisableGenParticleManifest<T>(this T toolSettings) where T : PACK
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.GenParticleManifest = false;
            return toolSettings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toolSettings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static T ToggleGenParticleManifest<T>(this T toolSettings) where T : PACK
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.GenParticleManifest = !toolSettings.GenParticleManifest;
            return toolSettings;
        }

        #endregion
	}
}
