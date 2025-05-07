using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RemoteEntity;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddRemoteEntityCore(this IHostApplicationBuilder hostBuilder)
    {
        hostBuilder.Services.AddSingleton<IEntityHive, EntityHive>();
        return hostBuilder;
    }
}