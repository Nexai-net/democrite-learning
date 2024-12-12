Multi-Agent Distributed System - Democrite - Advanced
___

<table border="10px !important" align="center">
    <tr border="none">
        <td border="none">
            <image src="https://raw.githubusercontent.com/Nexai-net/democrite/refs/heads/main/docs/logo.png">
        </td>
        <td border="0">
            <h1 valign='center'>Democrite</h1>
        </td>
    </tr>
</table>

<table border="0" align="center">
    <tr border="0">
        <td border="0">
            <h3 valign='center'>Sponsored by</h3>
        </td>
        <td border="0">
            <a href="https://www.amexiogroup.com"><img src="https://www.amexio.fr/wp-content/themes/Amexio/img/Amexio-Horizontal.svg" height="64px" /></a>
        </td>
    </tr>
</table>

Before reading this part we advice you to first attend previous parts :
- [Fondation](/docs/1-fondations.md)
- [Democrite - Basic](/docs/2-basic.md)

In this advanced tutorial we will see democrite advance features:

- Storage - Repository
- StreamQueue
- Dynamic Definition
- Artifacts
- Redirection

# Storage - Repository

Orleans provide a simple but efficient way to store data easily through **IPersistantState**.
This system allow grain to dump theire memory (state) and restore when needed without knowing where and how the data will be stored.

With **Democrite** we extern the principal to the term of "**repository**".
A "**repository**" is an advanced storage system that allow push, pull through a defined Id and advanced search through **linq** query.

First you need to configure the repository we the specific storage extensions and then request by construtor injection a read only (IReadOnlyRepository<,>) or not repository.

Example:

**configuration**

``` csharp
.SetupNodeMemories(s =>
{
    s.UseMongoRepository("RssFeeds")
})
```

In this example, we configured a repository that will use the last configured mongoDB databases. Each configuration is identify by a string, in our case "RssFeeds".

**usage**

To get a repository access service, the easiest way is request from the constructor the service.

``` csharp
[Repository("RssFeeds")] IReadOnlyRepository<RssItemMetaData> metaDataRepository,
```

``` csharp
[Repository("RssFeeds")] IRepository<RssItemMetaData> metaDataRepository,
```

> [!NOTE]
>
> One difficulty in state storage management is to list all the state or grain instanciate through time for a specific type.
> For example in the "Basic - Practice" we use a specific grain as registry where all the rss feeds have to register to be able to later one list all existing feeds.
> Using the repository feature you can request a service to search through all the existing state. Attention you need to configure a speicific state storage that enabled repository feature.

# StreamQueue

We cannot speak of distributed system without speacking of queuing and streaming.
Orlean provide a abstraction system used to integrate multiple type of system RabbitMQ, MSMQueue, Azure MQ, ... call "StreamQueue".

In democrite we use a definition to identify all the stream queue configuration.
This definition will be used by the trigger feature to be able to send data through it and react to it.

**definition**
``` csharp
var queue = StreamQueue.CreateFromDefaultStream("sni", "stream-key", "stream-namespace");
```

**trigger - from stream**
``` csharp
var from = Trigger.Stream(queue, "trigger-when-incomming-msg");
                  
                  // Define the number of concurrent message threat in same time
                  // 42 concurrent what is the culuster size
                  .MaxConcurrentProcess(42)

                  // Define the number of concurrent message threat in same time relative to the cluser SIZE
                  // example: 5 * NB_SILO
                  .MaxConcurrentFactorClusterRelativeProcess(5);
```

**trigger - to stream**
``` csharp
var target = Trigger.Signal(SIGNAL_ID)
                    .AddTargetStream(queue);
```

# Dynamic Definition
# Artifacts
# Redirection
# Practice
