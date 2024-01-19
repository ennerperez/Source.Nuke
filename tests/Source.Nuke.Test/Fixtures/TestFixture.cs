using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

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
