using Build.Test.Fixtures;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;
namespace Source.Nuke.Test.Abstracts
{
    public abstract class Test : TestBed<TestFixture>
    {
        protected Test([NotNull] ITestOutputHelper testOutputHelper, [NotNull] TestFixture fixture) : base(testOutputHelper, fixture)
        {
            var _ = fixture.GetService<IConfiguration>(testOutputHelper);
        }
    }
}
