using Autofac;
using HelloFacets.Tests;

namespace HelloFacets
{
    public class AppBootstrapper : AutofacBootstrapper<SearchViewModel>
    {
        protected override void ConfigureIoC(ContainerBuilder builder)
        {
            var client = TestFactory.CreateClient();
            builder.RegisterInstance(client);

            builder.RegisterType<AggregationsViewModel>().AsSelf().InstancePerLifetimeScope();
        }
    }
}