using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tochka.JsonRpc.Client.Tests
{
    public class TestEnvironment
    {
        public readonly IServiceProvider ServiceProvider;

        public TestEnvironment(Action<ServiceCollection> setup = null)
        {
            ServiceProvider = BuildServices(setup);
        }

        internal static ServiceProvider BuildServices(Action<ServiceCollection> setup)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole(_ => { });
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            setup?.Invoke(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }
    }
}
