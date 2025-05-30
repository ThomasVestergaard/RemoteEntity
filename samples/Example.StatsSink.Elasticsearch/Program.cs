using Elastic.Clients.Elasticsearch;
using Example.StatsSink.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteEntity;
using RemoteEntity.Redis;
using RemoteEntity.StatsSink.ElasticSearch;

var hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Configuration.AddJsonFile("appsettings.json");
hostBuilder.Configuration.AddEnvironmentVariables();
hostBuilder
    .AddRemoteEntityWithRedisBackEnd()
    .AddRemoteEntityElasticSearchStatsSink(new ElasticsearchStatsSinkOptions
    {
        FlushInterval = TimeSpan.FromSeconds(5) 
    });
hostBuilder.Services.AddHostedService<Consumer>();
hostBuilder.Services.AddHostedService<Producer>();
hostBuilder.Services.AddSingleton(new ElasticsearchClient(new Uri("http://localhost:9200")));


var host = hostBuilder.Build();
host.StartRemoteEntity();
Console.WriteLine("Example is running. Wait for console sink to flush stats to elastic.");
await host.RunAsync();