using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteEntity.Redis;
using RemoveEntity.Sample;


var hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Configuration.AddJsonFile("appsettings.json");
hostBuilder.Configuration.AddEnvironmentVariables();
hostBuilder.AddRemoteEntityWithRedisBackEnd();
hostBuilder.Services
    .AddSingleton<Consumer>()
    .AddSingleton<Producer>();

hostBuilder.Services.BuildServiceProvider();

var host = hostBuilder.Build();
host.StartRemoteEntityRedisConnection();

var consumer = host.Services.GetRequiredService<Consumer>();
await Task.Factory.StartNew(consumer.Execute, TaskCreationOptions.LongRunning);

Thread.Sleep(500);

var producer = host.Services.GetRequiredService<Producer>();
producer.Execute();

Console.WriteLine("Done.");
await host.RunAsync();