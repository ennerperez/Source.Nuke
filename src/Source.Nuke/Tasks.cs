using System;
using System.Collections.Generic;
using System.IO;
using Force.Crc32;
using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Source.Interfaces;
using System.Linq;

namespace Nuke.Common.Tools.Source
{
	[PublicAPI]
	public static partial class Tasks
	{
		public static IReadOnlyCollection<Output> Source(Configure<Tools> configurator)
		{
			return Source(configurator(new Tools()));
		}

		public static IReadOnlyCollection<Output> Source(Tools toolsSettings = null)
		{
			toolsSettings = toolsSettings ?? throw new NullReferenceException("ToolPath is not defined");
			if (!toolsSettings.Skip)
			{
				if (toolsSettings is IDownloadable)
				{
					var result = (toolsSettings as IDownloadable).Download();
					if (!result) throw new FileNotFoundException($"{toolsSettings.Executable} was not found");
				}
				if ((toolsSettings is ISlammin) && (toolsSettings as ISlammin).UsingSlammin.HasValue)
				{
					var usingSlammin = (toolsSettings as ISlammin).UsingSlammin;
					if (usingSlammin != null && usingSlammin.Value)
					{
						var crc32 = new Crc32Algorithm();
						var hash = string.Empty;
						var fs = File.ReadAllBytes(toolsSettings.ProcessToolPath);
						foreach (var b in crc32.ComputeHash(fs)) hash += b.ToString("x2").ToUpper();

						if (ISlammin.HashTable[toolsSettings.Executable] != hash)
							ISlammin.Download(toolsSettings);
					}
				}

				using var process = ProcessTasks.StartProcess(toolsSettings);
                try
                {
                    process.AssertZeroExitCode();
                }
                catch (Exception e)
                {
                    throw new Exception(process.Output.LastOrDefault().Text, e);
                }
                if (!string.IsNullOrWhiteSpace(toolsSettings.StdOutput))
					File.WriteAllText(toolsSettings.StdOutput, process.Output.StdToText());
				toolsSettings.Callback?.Invoke();
				return process.Output;
			}
			return null;
		}
	}
}
