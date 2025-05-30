[![NuGet](https://img.shields.io/nuget/dt/TVestergaard.RemoteEntity.svg)](https://www.nuget.org/packages/TVestergaard.RemoteEntity)

# What is this?
RemoteEntity is a small framework, that helps facilitate microservice communication.
It is based upon a producer/consumer pattern where the producer is the one who 'owns' a specific entity and the consumers are observing this entity over the network and reacting on chages.

The idea is, that the consumer does not need the full change history to get up to the latest version of the entity. The latest version of the entity is always stored and changes can be applied as they come.

RemoteEntity is a fan-out distribution model. Producer is distributing to all who is willing to listen. It does not re-send messages nor does it keep track of which consumers has gotten which message.

# Why do I need it?
If you have a stateful service, you need to restore state when you service restarts or crashes. If your state is dependent on the output from other services, RemoteEntity might help.
Consider the case where you services crashes and while it's down the dependency service produces new data. Once your service is up again your service somehow needs to get that update into it's state in order to resume working.

Eventsourcing might be an answer to this problem but that is also a big thing to introduce into a system. If you don't need to handle every event chronologically, RemoteEntity is for you.


# Dependencies
There are two dependencies:

* A place to store the latest version of the entity.
* A message queue/bus to push changes.

These two dependencies might be the same thing. For example redis offers key/value storage as well as pub/sub functionality.

# Prerequisites
To get started, you need access to a Redis instance. Run one locally in docker using:

    docker run -p 6379:6379 --name some-redis -d redis:latest

# Seamless integration with .NET hosting
Extensionmethods are provided to easy handle configuration and dependency injection.

# Getting started

Install the nuget packages.
```csharp
Install-Package TVestergaard.RemoteEntity.Redis -Version 0.5.0-beta
```

## Define a class that should be shared across services
The assembly this class lives in should be referenced on both the producer and consumer side.

```csharp
    public class MyClass : ICloneable<MyClass> {
        public string SomeValue { get; set; }        
    }
```

## Configure redis connection
### Using appsettings.json
```json
"RemoteEntity": 
{
    "RedisHostName": "localhost"
}
```

### Using environment variables
```bash
RemoteEntity__RedisHostName=localhost
```

## Register RemoteEntity in your application 
Register dependencies in DI and start the redis connection.
```csharp
    var hostBuilder = Host.CreateApplicationBuilder(args);
    hostBuilder.Configuration.AddJsonFile("appsettings.json"); // If you use appsettings
    hostBuilder.Configuration.AddEnvironmentVariables(); // If you use env vars
    hostBuilder.AddRemoteEntityWithRedisBackEnd();    
    
    var host = hostBuilder.Build();
    host.StartRemoteEntityRedisConnection();
```

## Produce some entities
```csharp    
    var entityHive = host.Services.GetRequiredService<IEntityHive>();

    var entity = new MyClass() {
        SomeValue = "Hello there!";
    }

    // Publish
    entityHive.PublishEntity(entity, "Some_id_for_this_entity");
    
    // Change something
    entity.SomeValue = "It's me again!";
    
    // Publish it again
    entityHive.PublishEntity(entity, "Some_id_for_this_entity");
```

## Setup consumer
```csharp
    var observableEntity = entityHive.SubscribeToEntity<SomeValueObject>("Some_id_for_this_entity", entity =>
    {
        // Handle real-time updates here. A thread-safe copy of the updated entity is parsed to this handler
        System.Console.WriteLine($"Update received. New value: {entity.SomeValue}");
    });

    // You can also access the latest version through the Observable object. This is thread safe.
    System.Console.WriteLine($"Current value: {observableEntity.Value.SomeValue}");
    
    // It is also possible to subscribe to updates directly on the entity observer.
    // This is useful if you register the subscribed entities in a dependency injection container.
    observableEntity.OnUpdate += updatedEntity => {
        System.Console.WriteLine($"Update received on entity event handler. New value: {updatedEntity.SomeValue}");
    };
```

## Stop everything
RemoteEntity is utilizing C# Channels and probably some active network connections (depending on which pub/sub method you are using) which needs a graceful shutdown.

When stopping your application, make sure to call Stop() on the EntityHive instance.

```csharp
    await entityHive.Stop();
```

# Tips, tricks and best practises
## Avoid large entities
Entities are meant to be small in nature. Keep them simple and do not publish liarge lists.
This is partticular true if you use redis ans backend. Redis is single threaded and a large publish will block redis until done.

Here are a few implementation tips and tricks based on real-life experience
## Put entity id's on the entity class
To avoid having a entity id's hardcoded all over the place, make a helper method on each entity to generate an apropiate entity id. Often entity id's are dynamic and are generated based on different input variables. This logic should be centralized.
```csharp
    public class MyClass : DeepCloner<MyClass> {
        public static string EntityId(int input1, string input2) => $"MyClass_{input1}_{input2}";

        public string SomeValue { get; set; }        
    }
```

## Have entities in a shared assembly
Entities should be very simple POCO's and should not have any dependencies to business logic. It should be possible to make a seperate class library project that contains only entities. This assembly is then referenced by producers and consumers alike.

## Version control your entities
As entities often are shared in a distributed system, it serves as an integration point where changes might break the integration if not managed properly. It can be helpful to put a version number in the entity class names and use the [Obsolete] attribute to indicate that a migration is ongoing.
```csharp
    [Obsolete]
    public class MyClass_v1 : DeepCloner<MyClass> {
        public static string EntityId(int input1, string input2) => $"MyClass_v1_{input1}_{input2}";
        public string SomeValue { get; set; }        
    }
```
Then put a new version into production parallel with the old one.
```csharp    
    public class MyClass_v2 : DeepCloner<MyClass> {
        public static string EntityId(int input1, string input2) => $"MyClass_v2_{input1}_{input2}";
        public string SomeChangedValue { get; set; }
        public int AnotherValue {get; set;}
    }
```
## Be aware of duplicate publishes
To avoid too much chatter on the network, try to avoid publishing identical enties. RemoteEntity has a built-in duplicate detector to help this. Per default, it allows duplicate publishes, but this can be overridden.
```csharp    
    var entityHive = host.Services.GetRequiredService<IEntityHive>();
    entityHive.HiveOptions.PublishDuplicates = false;
```
The duplicate detector tracks a md5 hash of the latest published entity per entityId. If the current hash equals the latest published hash and PublishDuplicates is set to false, the entity is not published.
Check the full implementation of duplicate detection in DuplicateDetector.cs

# Samples
A simple producer and consumer sample application is included in this repo. I recommend you play around with them to get familiar with the framework behaviour.

Samles needs access to a Redis instance.


# Storage and message queue support
RemoteEntity relies on being able to store and fetch serialized objects and to push updates through a message queue or bus.

The implementation of these two dependencies are put in a seperate nuget package. You can make your own implementations by implementing the IEntityStorage and IEntityPubSub interfaces.

# Roadmap
In no particular order
- [ ] Standalone UI tool for browsing entities
- [x] Helper methods for easy DI integration and hooking into the Microsoft eco system
- [ ] NATS/Jetstream backend using streams and KV store
- [X] Metrics logger
- [ ] Serialization abstraction. Default to System.Json
- [ ] Prototype a standalone backend, ditching dependencies on redis, nats, whatever
- [x] Configurable duplication detection

