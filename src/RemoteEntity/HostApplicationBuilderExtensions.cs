using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteEntity.Stats;

namespace RemoteEntity;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddRemoteEntityCore(this IHostApplicationBuilder hostBuilder)
    {
        hostBuilder.Services.AddSingleton<IEntityHive, EntityHive>();
        hostBuilder.Services.AddScoped<IStatsSinkManager, StatsSinkManager>();
        return hostBuilder;
    }
    
    public static IHost StartRemoteEntity(this IHost host)
    {
        var hive = host.Services.GetRequiredService<IEntityHive>();
        hive.Start();
        return host;
    }
}