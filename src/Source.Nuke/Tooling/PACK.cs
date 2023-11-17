using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Nuke.Common.Tooling;

// ReSharper disable InconsistentNaming

namespace Nuke.Source.Tooling
{
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class PACK : Nuke.Source.Abstractions.Tooling
    {

        public override string ProcessToolPath => string.Empty;

        // /// <summary>
        // ///
        // /// </summary>
        // public virtual string Input { get; internal set; }

        /// <summary>
        /// Keep (don't delete) input files
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected override Arguments ConfigureProcessArguments(Arguments arguments)
        {
            arguments
                .Add("--verbose", Verbose)
                //
                //
                .Add("{value}", Input);
            return base.ConfigureProcessArguments(arguments);
        }

    }
}
