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

# Preresiquits
To get started, you need access to a redis instance. Run one locally in docker using:

    docker run -p 6379:6379 --name some-redis -d redis:latest


# Getting started

Get the nuget packages here: TODO

## Define your class that should be shared across services
This class should be available on both the producer and consumer side.

```csharp
    public class MyClass : ICloneable<MyClass> {
        public string SomeValue { get; set; }

        public MyClass Clone() {
            return new MyClass {
                SomeValue = this.SomeValue;
            }
        }
    }
```

## Initiate the entity hive
EntityHive is used in both the producer and the consumer.
```csharp
    var redisClient = new RedisClient("redis://localhost:6379");
    var redisEntityStorage = new RedisEntityStorage(redisClient, NullLogger.Instance);
    var redisEntityPubSub = new RedisEntityPubSub(redisClient, NullLogger.Instance);
    var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger.Instance);
```

## Setup producer
```csharp    
    var entity = new MyClass() {
        SomeValue = "Hello there!";
    }

    entityHive.PublishEntity(entity, "Some_id_for_this_entity");
```

## Setup consumer
```csharp
    var observableEntity = hive.SubscribeToEntity<SomeValueObject>("Some_id_for_this_entity", entity =>
    {
        // Handle real-time updates here. A thread-safe copy of the updated entity is parsed to this handler
        System.Console.WriteLine($"Update received. New value: {entity.SomeValue}");
    });

    // You can also access the latest version through the Observable object. This is thread safe.
    System.Console.WriteLine($"Current value: {observableEntity.Value.SomeValue}");
```

# Samples
A few usage examples are available in this repo.
## Sample case 1
Producer keeps producing when consumers are down. When consumers comes up, they see the latest version of the entity.

## Sample case 2
Producer stop producing. Consumers are still able to get the latest version of the entity. When producer comes back up, consumers resume getting updates.


# Storage and message queue support
RemoteEntity relies on being able to store and fetch serialized objects and to push updates through a message queue or bus.

The implementation of these two dependencies are put in a seperate nuget package. You can make your own implementations by implementing the IEntityStorage and IEntityPubSub interfaces.

