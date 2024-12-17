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

In this advanced Documentation we will see democrite advanced features:

- Storage - Repository
- StreamQueue
- Dynamic Definition
- Unit Testing
- Artifacts

This advanced Documentation provides insights into key features that are essential for optimizing and managing your data flow. In the practical section, we will leverage these features to enhance the outcomes of our previous practice.

# Storage - Repository

Orleans provide a simple but efficient way to store data easily through **IPersistantState**.
This system allow grain to dump theire memory (state) and restore when needed without knowing where and how the data will be stored.

With **Democrite** we extern the principal to the term of "**repository**".<br/>
A "**repository**" is an advanced storage system that allow push, pull through a defined Id and advanced search through **linq** query.

1- Begin by configuring the repository with the appropriate storage extensions. <br/>
2- Subsequently, request a repository instance, either read-only (IReadOnlyRepository<,>) or read-write, through constructor injection using the configuration key (Call also StorageName)

> [!CAUTION]
>
> The entity stored using a repository must implemente the interface IEntityWithId<ID_TYPE>. <br/>
> This interface allow search and storage using a Uid property. <<br/>


> [!NOTE]
>
> The repository system support projection meaning you don't need to get all the object stored, you can choose the requested parameters.

Example:

**Configuration**

``` csharp
.SetupNodeMemories(s =>
{
    s.UseMongoRepository("RssFeeds")
})
```

For this example, a repository was configured to utilize the most recently specified MongoDB database. This configuration is uniquely identified by the string 'RssFeeds'.

**Usage**

To get a repository access service, the easiest way is request from the constructor the service.

``` csharp
[Repository("CollectionName", "RssFeeds")] IReadOnlyRepository<RssItemMetaData, Guid> metaDataRepository,
```

``` csharp
[Repository("CollectionName", "RssFeeds")] IRepository<RssItemMetaData, Guid> metaDataRepository,
```

> [!NOTE]
>
> One difficulty in state storage management is to list all the state or grain instanciate through time for a specific type.
> For example in the "Basic - Practice" we use a specific grain as registry where all the rss feeds have to register to be able to later one list all existing feeds.
> Using the repository feature you can request a service to search through all the existing state. Attention you need to configure a speicific state storage that enabled repository feature.

# StreamQueue

We cannot speak of distributed system without speacking of queuing and streaming.
Orlean provide an abstraction system used to integrate multiple type of system RabbitMQ, MSMQueue, Azure MQ, ... call "StreamQueue".

In democrite we use a **definition** to identify a StreamQueue full configuration.
This **definition** will be used by the trigger feature to be able to send data through it and react to new incomming message it.

**Definition**
``` csharp
var queue = StreamQueue.CreateFromDefaultStream("sni", "stream-key", "stream-namespace");
```

**Trigger - from stream**
``` csharp
var from = Trigger.Stream(queue, "trigger-when-incomming-msg");
                  
                  // Define the number of concurrent message threat in same time
                  // 42 concurrent what is the culuster size
                  .MaxConcurrentProcess(42)

                  // Define the number of concurrent message threat in same time relative to the cluster SIZE
                  // example: 5 * NB_SILO
                  .MaxConcurrentFactorClusterRelativeProcess(5);
```

**Trigger - to stream**
``` csharp
var target = Trigger.Signal(SIGNAL_ID)
                    .AddTargetStream(queue);
```

> [!CAUTION]
> No infinit loop system exist in democrite if you push back on the same queue the message you received from it, you risk to trigger an infinit loop.

# Dynamic Definition

At this point in the Documentation, you've learned how to define and inject dependencies during node configuration.<br />

> [!NOTE]
>
> If you push a new definition or update most of the definition provider have a **refresh** method to notify the cluster about changes.

Certain scenarios require the dynamic creation of definitions for short-term use.

For instance, when the system identifies a security threat, it might invoke the "Check" sequence to validate the information. As this sequence and its associated processes can be time-consuming, we opt to avoid waiting and instead use a signal to notify us upon completion. This dynamic situation necessitates the creation of a signal definition and potentially a trigger definition specifically for this case.

To facilitate this, we employ the IDynamicDefinitionHandler service. This service stores the definition and notifies other silos of the newly available definition, enabling synchronization. Later, these dynamically created definitions can be removed to maintain a clean environment.

> [!CAUTION]
>
> Dynamically created definitions, by default, have a lifespan tied to the cluster's existence. This means they will be removed if all cluster nodes go offline.

The dynamic definition feature will also be employed in test scenarios, Sass scenarios, and YAML definition compilation. Subsequent Documentations will provide further insights into these use cases.

# Unit Test

A fundamental practice in development is writing unit tests to ensure the consistent behavior of implemented features. The agent paradigm in Orleans is well-suited for this type of testing.

By definition, agents are isolated. All external service dependencies within Orleans must be injected via the constructor as services (interfaces).

A challenge in testing "grains" arises from the RuntimeContext. This service, not injected via the constructor, is obtained from the execution thread context. It's responsible for providing the grain with necessary services without requiring the child class to pass them through the constructor.

To facilitate unit testing of VGrains, Democrite exposes a NuGet package named [Democrite.UnitTests.Toolkit](https://www.nuget.org/packages/Democrite.UnitTests.ToolKit). This library extends AutoFixture to provide methods for creating VGrain instances with specific GrainIds and services.

# Artifacts

Democrite utilizes the concept of artifacts to reference all external resources employed by the engine, such as model files or executable scripts.

This section delves deeper into executable artifacts. Democrite empowers developers to create "VGrains" (Agents) in various programming languages. A  [Pyhton](https://github.com/Nexai-net/democrite/tree/main/samples/PythonVGrains) VGrain example is provided to illustrate equation processing.

> [!CAUTION]
>
> Primarily, this approach enables the use of libraries that aren't available in .NET. However, external VGrains have limitations and can introduce potential performance overhead during deployement and execution.

An external VGrain is essentially a script package, bundled with its dependencies within a ZIP file. It's installed in a designated folder and can be executed in two modes:

- **On-demand execution**: The VGrain is invoked for each request.
- **Daemon mode**: The VGrain is launched once and remains active to handle multiple requests.

> [!NOTE]
>
> To use Democrite in Python, simply install the package using pip install democrite.
>
> To create a VGrain, instantiate a democrite.vgrain(ARGV) object and call either the run or exec method, passing a lambda function as the callback to be executed when a request arrives.


# Practice

In this Documentation series, we'll use Democrite to build a web application that scrapes RSS feeds, analyzes content, and provides a basic search function.

**Part 2**: Consolidation and Search --> [Practice](/docs/3-advanced-practice-democrite.md)

In this second tial part, we'll cover:

- **Project Enhancement** : We will improve the previous practice by integrated advanced feature like StreamQueue, Repository ...
- **Indexation**: Using a simple python library we will try to extract meta information about the article content.
- **Search**: We will intgrate a search API to get back the more related articles.

Subsequent parts will delve deeper into advanced topics, including:

- **Blackboard** : Use a blackboard to analyze deeper the Rss Item (Article) information to improve the indexation