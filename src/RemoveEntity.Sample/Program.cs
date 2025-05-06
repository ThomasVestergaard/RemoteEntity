using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RemoteEntity;
using RemoteEntity.Redis;


var hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Services.Configure<RedisConnectionOptions>(hostBuilder.Configuration.GetSection("RemoteEntity"));

var host = hostBuilder.Build(); 

var config = host.Services.GetService<IOptions<RedisConnectionOptions>>();

int halt = 0;
/*public static class Program
{
    public static void Main(string[] args)
    {
        Task.Factory.StartNew(Consumer.Execute, TaskCreationOptions.LongRunning);
        Thread.Sleep(500);

        Producer.Execute();
        
        Console.WriteLine("Done. Hit any key to quit");
        Console.ReadKey();
    }
}*/