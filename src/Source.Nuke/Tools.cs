using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using System.Collections.Generic;

namespace Nuke.Common.Tools.Source
{

	[PublicAPI]
	[Serializable]
	public class Tools : ToolSettings
	{

		public Tools()
		{
		}

		public Tools(string executable)
		{
			Executable = executable;
		}

		public string Executable { get; private set; }

        /// <summary>
        ///
        /// </summary>
        [ExcludeFromCodeCoverage]
        public override string ProcessToolPath => Path.Combine(InstallDir, Executable);

		/// <summary>
		///
		/// </summary>
        [ExcludeFromCodeCoverage]
        public Action<OutputType, string> ProcessCustomLogger => DotNetTasks.DotNetLogger;

		/// <summary>
		/// Turn on verbose output (also shows more command-line options). Use without any other parameters.
		/// </summary>
		public virtual bool? Verbose { get; internal set; }

		/// <summary>
		/// Specify the folder of the gameinfo.txt file.
		/// </summary>
		public virtual string Game { get; internal set; }


        /// <summary>
        /// Specify the Install folder.
        /// </summary>
        public virtual string InstallDir { get; internal set; }

		public virtual long AppId { get; internal set; }

		/// <summary>
		///
		/// </summary>
		public virtual string Input { get; internal set; }

		/// <summary>
		/// Contains the output of the process execution.
		/// </summary>
		public virtual string StdOutput { get; internal set; }

        /// <summary>
        ///
        /// </summary>
        public virtual Action<IReadOnlyCollection<Output>> Callback { get; internal set; }

		/// <summary>
		/// Run as an idle-priority process.
		/// </summary>
		public virtual bool? Low { get; internal set; }

		/// <summary>
		/// Control the number of threads used. Defaults to the # of processors (times 2 for Hyperthreading/SMT CPU's) on your machine. Maximum is 16 threads. With a patched version you can reach 32 threads.
		/// </summary>
		public virtual ushort? Threads { get; internal set; }

		/// <summary>
		/// Override the VPROJECT environment variable.
		/// </summary>
		public virtual string VProject { get; internal set; }

		public virtual bool? Fast { get; internal set; }
		public virtual bool Skip { get; internal set; }

	}
}
