using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Nuke
{
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class AppCenter : ToolSettings
    {
        public AppCenter()
        {
        }

        public override Action<OutputType, string> ProcessCustomLogger => DotNetTasks.DotNetLogger;

        public virtual bool? Version { get; internal set; }

        protected override Arguments ConfigureProcessArguments(Arguments arguments)
        {
            if (Version == true)
            {
                arguments
                    .Add("--version");
            }
            return base.ConfigureProcessArguments(arguments);
        }


    }
}
