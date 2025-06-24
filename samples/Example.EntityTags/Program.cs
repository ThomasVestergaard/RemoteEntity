using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteEntity;
using RemoteEntity.Redis;
using Example.GettingStarted;


var hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Configuration.AddJsonFile("appsettings.json");
hostBuilder.Configuration.AddEnvironmentVariables();
hostBuilder.AddRemoteEntityWithRedisBackEnd();
hostBuilder.Services.AddHostedService<Consumer>();
hostBuilder.Services.AddHostedService<Producer>();


hostBuilder.Services.BuildServiceProvider();

var host = hostBuilder.Build();
host.StartRemoteEntity();


await host.RunAsync();