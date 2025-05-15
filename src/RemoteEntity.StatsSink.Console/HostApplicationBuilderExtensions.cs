using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteEntity.Stats;

namespace RemoteEntity.StatsSink.Console;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddRemoteEntityConsoleStatsSink(this IHostApplicationBuilder hostBuilder, TimeSpan flushToConsoleInterval )
    {
        var options = new ConsoleStatsSinkOptions
        {
            FlushToConsoleInterval = flushToConsoleInterval
        };
        hostBuilder.Services.AddSingleton(options);
        hostBuilder.Services.AddTransient<IRemoteEntityStatsSink, ConsoleStatsSink>();
        return hostBuilder;
    }
}