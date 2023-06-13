using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

// Set the orderer
[assembly: TestCollectionOrderer(nameof(TestCaseOrderAttribute), "Build.Test")]
// Need to turn off test parallelization so we can validate the run order
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Build.Test.Fixtures
{
    public class TestFixture : TestBedFixture
    {
        protected override void AddServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        protected override ValueTask DisposeAsyncCore()
        {
            return new ValueTask();
        }

        protected override IEnumerable<TestAppSettings> GetTestAppSettings()
        {
            yield return new() {Filename = "appsettings.json", IsOptional = true};
        }
    }
}
