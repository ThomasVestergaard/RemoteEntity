# What is this?
RemoteEntity is a small framework which helps commication between microservices.
It is based upon a producer/consumer pattern where the producer is the one who 'owns' a specific entity and the consumers are observing this entity over the network and reacting on chages.

The idea is, that the consumer does not need the full change history to get up to the latest version of the entity. The latest version of the entity is always stored and changes can be applied as they come.

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

## Setup producer
```csharp
    var redisClient = new RedisClient("redis://localhost:6379");
    var redisEntityStorage = new RedisEntityStorage(redisClient, NullLogger.Instance);
    var redisEntityPubSub = new RedisEntityPubSub(redisClient, NullLogger.Instance);
    var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger.Instance);

    var entity = new MyClass() {
        SomeValue = "Hello there!";
    }

    entityHive.PublishEntity(entity, "Some_id_for_this_entity");
```

## Setup consumer
```csharp
    var redisClient = new RedisClient("redis://localhost:6379");
    var redisEntityStorage = new RedisEntityStorage(redisClient, NullLogger.Instance);
    var redisEntityPubSub = new RedisEntityPubSub(redisClient, NullLogger.Instance);
    var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger.Instance);

    var observableEntity = hive.SubscribeToEntity<SomeValueObject>("Some_id_for_this_entity", entity =>
    {
        // Handle real-time updates here. A thread-safe copy of the updated entity is parsed to this handler
        System.Console.WriteLine($"Update received. New value: {entity.SomeValue}");
    });

    // You can also access the latest version through the Observable object. This is thread safe.
    System.Console.WriteLine($"Current value: {observableEntity.Value.SomeValue}");
```