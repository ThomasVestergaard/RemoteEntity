using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteEntity.Stats;

namespace RemoteEntity.StatsSink.ElasticSearch;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddRemoteEntityElasticSearchStatsSink(this IHostApplicationBuilder hostBuilder, ElasticsearchStatsSinkOptions options )
    {
        hostBuilder.Services.AddSingleton(options);
        hostBuilder.Services.AddScoped<IRemoteEntityStatsSink, ElasticSearchStatsSink>();
        hostBuilder.Services.AddScoped<IElasticsearchStatsRepository, ElasticsearchStatsRepository>();
        return hostBuilder;
    }
}