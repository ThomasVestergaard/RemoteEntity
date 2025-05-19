using Example.StatsSink.Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteEntity;
using RemoteEntity.Redis;
using RemoteEntity.StatsSink.Console;

// TODO: Before merge to main, remember to change reference to nuget source

var hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Configuration.AddJsonFile("appsettings.json");
hostBuilder.Configuration.AddEnvironmentVariables();
hostBuilder.AddRemoteEntityWithRedisBackEnd()
    .AddRemoteEntityConsoleStatsSink(TimeSpan.FromSeconds(5)); // Set sink to flush every 5 secs
hostBuilder.Services.AddHostedService<Consumer>();
hostBuilder.Services.AddHostedService<Producer>();

var host = hostBuilder.Build();
host.StartRemoteEntity();
Console.WriteLine("Example is running. Wait for console sink to flush stats to the console.");
await host.RunAsync();