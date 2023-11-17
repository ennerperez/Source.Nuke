using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Force.Crc32;
using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Source.Interfaces;

namespace Nuke.Source.Abstractions
{
	[PublicAPI]
	[ExcludeFromCodeCoverage]
	public static partial class Tasks
	{
		public static IReadOnlyCollection<Output> Source(Configure<Tooling> configurator)
		{
			return Source(configurator(new Tooling()));
		}

		public static IReadOnlyCollection<Output> Source(Tooling tooling = null)
		{
			tooling = tooling ?? throw new NullReferenceException("ToolPath is not defined");
			if (!tooling.Skip)
			{
				if (tooling is IDownloadable)
				{
					var result = (tooling as IDownloadable).Download();
					if (!result) throw new FileNotFoundException($"{tooling.Executable} was not found");
				}
				if ((tooling is ISlammin) && (tooling as ISlammin).UsingSlammin.HasValue)
				{
					var usingSlammin = (tooling as ISlammin).UsingSlammin;
					if (usingSlammin != null && usingSlammin.Value)
					{
						var crc32 = new Crc32Algorithm();
						var hash = string.Empty;
						var fs = File.ReadAllBytes(tooling.ProcessToolPath);
						foreach (var b in crc32.ComputeHash(fs)) hash += b.ToString("x2").ToUpper();

						if (ISlammin.HashTable[tooling.Executable] != hash)
							ISlammin.Download(tooling);
					}
				}

				using var process = ProcessTasks.StartProcess(tooling);
				process.AssertZeroExitCode();
				if (!string.IsNullOrWhiteSpace(tooling.StdOutput))
					File.WriteAllText(tooling.StdOutput, process.Output.StdToText());
				tooling.Callback?.Invoke();
				return process.Output;
			}
			return null;
		}
	}
}
