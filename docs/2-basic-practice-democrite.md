Democrite - Basic - Practice
___

# Goal

In this tutorial series, we'll use Democrite to build a web application that scrapes RSS feeds, analyzes content, and provides a basic search function.

**Part 1**: Foundation and RSS Feed Integration

In this initial part, we'll cover:

- **Project Structure**: Setting up the core application structure.
- **RSS Feed Loading**: Fetching and storing articles from a specified RSS feed.
- **Auto-Refresh**: Implementing an automatic refresh mechanism to keep the content up-to-date.

## Specifications

- Use Democrite (Attention: you may need to enable the pre-release option)
- Create two binaries: a client and a node
- Allow injecting an RSS feed via the API using its URL (e.g., [invalid URL removed])
- Load the XML RSS file, parse it, and store the RSS items.
- Enable listing all registered feeds.
- Ensure information persistence even after a complete application shutdown.
- Automatically reload feeds and detect changes every 5 minutes.

## Project Architecture

### Client

The client solely exposes a web API, accessible through Swagger UI. It interacts with the Democrite cluster by utilizing the **IDemocriteExecutionHandler** interface.

|Confiuration|Value|
|--|--|
|**Project Type**| Web *(Use method expose on the [fondation orleans practice](/docs/1-fondations-practice-orleans.md#1---solution-setup) to simple create it)*|
|**Name**|Democrite.Practice.Rss.Client|
|**Dotnet Framework**| 8.0|

**NuGet Dependencies:**

- **Democrite.Framework.Client**: Facilitates client-side interactions with the Democrite cluster.
- **Democrite.Framework.Extensions.Mongo**: Enables the use of MongoDB as a data storage and synchronization mechanism.
- **Swashbuckle.AspNetCore**: Implements a Swagger UI, providing a web interface for API testing and documentation.

### Node
The node operates within a cluster to process and handle RSS feeds.

|Confiuration|Value|
|--|--|
|**Project Type**| Console|
|**Name**|Democrite.Practice.Rss.Node|
|**Dotnet Framework**| 8.0|


NuGet Dependencies:

- **Democrite.Framework.Node**: This core library provides the functionality for a node within the Democrite cluster. It automatically references the essential Democrite Framework components.

- **Democrite.Framework.Extensions.Mongo**: This extension unlocks the ability to utilize MongoDB as a central storage and synchronization mechanism for your application.

- **Democrite.Framework.Node.Cron**: This extension empowers you to schedule tasks using cron expressions, allowing for automated RSS feed reloading every 5 minutes.

- **Democrite.Framework.Builders**: This library provides a fluent interface for constructing various definitions within your Democrite application. This simplifies the definition creation process.

- **Democrite.Framework.Bag.DebugTools**: This "bag" contains helpful grains and utilities that streamline the debugging process for your project.

- **Microsoft.Extensions.Http**: This Microsoft library enhances your application's capabilities by adding functionalities for making HTTP calls, which might be necessary for interacting with external APIs during RSS feed processing.

### Data Contract

This library will contain all the interfaces and models shared between the client and the node.

|Confiuration|Value|
|--|--|
|**Project Type**| Class library|
|**Name**|Democrite.Practice.Rss.DataContract|
|**Dotnet Framework**| 8.0|

NuGet Dependencies:

- **Democrite.Framework.Core.Abstractions**: This library is the root of all the democrite framwork and contains the interface, attribute definitions.

this projet MUST be referenced by the client and the node.

## API

The api will be expose only by the client. 
Use mimimal api like in [fondation orleans practice](/docs/1-fondations-practice-orleans.md#2---api) or classic controller if you are more familiar with it.

|Method|Route|Return|Objectif|
|--|--|--|--|
| PUT | inject/rss | Http Code 200 | This endpoint inject a rss url in the system|
| GET | feed/list | a list of rss feed's url with his hash ID on the system| This endpoint list all the subscribe rss feeds|

## Node Architecture

**VGrains**

|Name|Key Type|Role|
|--|--|--|
|**IRssRegistryVGrain**|Singleton| This VGrain store all the rss link |
|**IRssVGrain**| String Url Hash Id| This VGrain represent a rss feed and store all the item ids|
|**IRssItemVGrain**| String Item Guid Hash| This VGrain represent an item in a feed|
|**IRssMonitorVGrain**|Singleton| This shared VGrain will allow the client to list the rss links|

<br/>

**Sequence**

|Name (SNI)|Input|Output|Goal|
|--|--|--|--|
|load-rss-items|RssFeedUrlSource|None|This sequence take a feed url load the items and foreach item store its content|
|import-rss|Uri|None|This sequence take a feed url store it in the **IRssRegistryVGrain** and call sequence **load-rss-items** the load it|
|refresh-all-inject-feeds|None|None|This sequence will be called by the trigger to get all the rss feed urls and call sequence **load-rss-items** the load them|

<br/>

**Signal**

|Name|Carry|Usage|
|--|--|--|
|rss-item-updated|UrlRssItem|This signal is raised when a new item is loaded|

<br/>

**Triggers**

|Name|Type|Configuration|Targets|Usage|
|--|--|--|--|--|
|auto-update-loop|Cron|*/5 * * *|Sequence: **refresh-all-inject-feeds**| This trigger will every 5 min (min % 5 == 0) call the sequence **refresh-all-inject-feeds** to reload all the registered urls|


## Data Models

In this part we will see the model structure reference on the guidance future.

Those following data container are only used in node side.

``` csharp

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class RssItem(string Uid,
                                       string Link,
                                       string Title,
                                       string Description,
                                       string? Content,
                                       string SourceId,
                                       IReadOnlyCollection<string> Creators,
                                       DateTime PublishDate,
                                       IReadOnlyCollection<string> Keywords,
                                       IReadOnlyCollection<string> Categories);

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class RssItemMetaData(string Uid, string Link, DateTime LastUpdate);

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct UrlData(Uri SourceUrl, string Data);

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct UrlRssItem(string Guid, string Link);

```

The Following data container are used by the client and the node, is should be placed in the shared part.

``` csharp

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct RssFeedUrlSource(Uri SourceUri, string HashId);
```

> [!NOTE]
>
>All these data containers are tagged with multiple attributes:
>
>- **Immutable**: An Orleans attribute indicating that the object is immutable, enabling optimizations during data transfer by preventing >unnecessary copying.
>
>- **Serializable**: A standard .NET attribute marking the object as serializable.
>
>- **GenerateSerializer**: An Orleans attribute instructing the framework to automatically generate the required serializers and copiers for the >object. This is essential for objects that may be transferred between silos or clients.
>
>- **ImmutableObject**: Another .NET attribute reinforcing the immutability of the object.
>
>While all four attributes are beneficial, only the GenerateSerializer attribute is strictly mandatory for cross-silo or client-server 
communication.

# Guide

## 1 - Structure and Configuration

### Client

The client configuration will mimic the one explains in previous practice [Fondation Orleans](/docs/1-fondations-practice-orleans.md#1---solution-setup).
Instead of orleans configuration "UseOrleans" we will setup the democrite client.

``` csharp
builder.Host.UseDemocriteClient(b =>
                                {
                                    // Wizard Config allow you to use democrite help for configuration
                                    b.WizardConfig()
                                     
                                     // Define a mongoDB as meeting point for the cluster
                                     .UseMongoCluster(o => o.ConnectionString("127.0.0.1:27017"));
                                });
```

### Node

The node will be in standalone mode:

``` csharp
var node = DemocriteNode.Create(b =>
                                {
                                    // Use democrite wizard configuration
                                    b.WizardConfig()

                                     // Define MongoBD as cluster meeting point
                                     .UseMongoCluster(o => o.ConnectionString("127.0.0.1:27017"))

                                     // Enabled logging through console view
                                     .ConfigureLogging(c => c.AddConsole())

                                     // Enabled HttpClient service to download web content
                                     .ConfigureServices(s =>
                                     {
                                         s.AddHttpClient();
                                     })

                                     // Add tools an grain declaration usefull to debug
                                     .AddDebugTools(LogLevel.Information)

                                     // Activate Cron trigger management
                                     .UseCronTriggers()

                                     // Open to client connection
                                     .ExposeNodeToClient()

                                     // Setup all the need definitions
                                     // .AddPracticeDefinitions()

                                     // Define mongodb as default storage point
                                     .SetupNodeMemories(n => n.UseMongo(StorageTypeEnum.All));
                                });

await using (node)
{
    await node.StartUntilEndAsync();
}
```

## 2 - VGrain creation
**Declaration**
``` csharp

    /// <summary>
    /// Grain used to register all the rss feed stored
    /// </summary>
    [VGrainIdSingleton]
    internal interface IRssRegistryVGrain : IVGrain, IRssMonitorVGrain
    {
        /// <summary>
        /// Register if needing the rss feed
        /// </summary>
        Task<RssFeedUrlSource> RegisterAsync(Uri rssFeed, IExecutionContext ctx);
    }

    /// <summary>
    /// Grain dedicated to a specific source
    /// </summary>
    /// <remarks>
    ///     The GrainId is a hash of the source url
    /// </remarks>
    [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{executionContext.Configuration}")]
    public interface IRssVGrain : IVGrain
    {
        /// <summary>
        /// Parses the RSS to produce <see cref="RssItem"/>
        /// </summary>
        Task<IReadOnlyCollection<RssItem>> LoadAsync(Uri source, IExecutionContext<string> executionContext);
    }

    /// <summary>
    /// VGrain dedicated to each rss item
    /// </summary>
    /// <remarks>
    ///     Use Rss link hash ad identifier
    /// </remarks>
    [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{executionContext.Configuration}")]
    public interface IRssItemVGrain : IVGrain
    {
        /// <summary>
        /// Updates the rss item content
        /// </summary>
        Task<UrlRssItem> UpdateAsync(RssItem item, IExecutionContext<string> executionContext);
    }
```

> [!NOTE]
>
> Usually in dotnet we keep one class/interface/structure by files. <br/>
> It's tolerate to have one files with multiple data structure (DTO)

In the declaration below, you see **IRssRegistryVGrain** marked as **internal** and inheriting from **IRssMonitorVGrain**. A VGrain interface does not need to be public to be used. You can choose the desired protection level. 

In fact, we recommend making the **implementation** internal to ensure only the interface is exposed.

The IRssMonitorVGrain interface must be placed in the shared library to be recognized by the client.

``` csharp
    [VGrainIdSingleton]
    public interface IRssMonitorVGrain : IVGrain
    {
        /// <summary>
        /// Gets all registred feed asynchronous.
        /// </summary>
        [ReadOnly]
        Task<IReadOnlyCollection<RssFeedUrlSource>> GetAllRegistredFeedAsync(IExecutionContext ctx);
    }
```

A VGrain interface or implementation can inherit from multiple other grains. The instance ID is constructed from the implementation name and key information. Multiple declarations can refer to the same implementation and instance if their key information matches.

This flexibility allows for selective method exposure. While the registration details are internal to protect the implementation, public methods like GetAllRegisteredFeedAsync can be accessed by the client.

**State** 

In our scenario, the VGrain will maintain a state, necessitating a state object. Based on Orleans best practices, we've observed potential pitfalls related to constructor design when creating serializable states.

To mitigate these risks, we advocate for a surrogate-based approach in Democrite. While requiring more code, this method offers greater safety. We'll divide the implementation into three parts:

- **State Class**: This class manages the grain's memory, encapsulating business logic for data integrity and validity.
- **Surrogate**: A simple data container designed to streamline saving and restoration from storage.
- **Converter**: A class responsible for converting between the State and Surrogate representations.

See the example of the **IRssRegistryVGrain** state:

``` csharp
internal sealed class RssRegistryState
{
    #region Fields

    private readonly Dictionary<string, RssFeedUrlSource> _rssFeeds;
    private IImmutableList<RssFeedUrlSource> _copyList;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RssRegistryState"/> class.
    /// </summary>
    public RssRegistryState(IEnumerable<RssFeedUrlSource> rssFeeds)
    {
        this._rssFeeds = rssFeeds?.ToDictionary(k => k.HashId) ?? new Dictionary<string, RssFeedUrlSource>();
        this._copyList = this._rssFeeds.Values.ToImmutableList();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the RSS feeds.
    /// </summary>
    public IReadOnlyCollection<RssFeedUrlSource> RssFeeds
    {
        get { return this._copyList; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Pushes the specified source.
    /// </summary>
    public bool Push(in RssFeedUrlSource source)
    {
        if (this._rssFeeds.ContainsKey(source.HashId))
            return false;

        this._rssFeeds[source.HashId] = source;
        this._copyList = this._rssFeeds.Values.ToImmutableList();

        return true;
    }

    #endregion
}

[Immutable]
[Serializable]
[GenerateSerializer]
[ImmutableObject(true)]
/// <summary>
/// Surrogate data container
/// </summary>
public record struct RssRegistryStateSurrogate(IReadOnlyCollection<RssFeedUrlSource> Feeds);

[RegisterConverter]
internal sealed record class RssRegistryStateConverter : IConverter<RssRegistryState, RssRegistryStateSurrogate>
{
    /// <inheritdoc />
    public RssRegistryState ConvertFromSurrogate(in RssRegistryStateSurrogate surrogate)
    {
        return new RssRegistryState(surrogate.Feeds);
    }

    /// <inheritdoc />
    public RssRegistryStateSurrogate ConvertToSurrogate(in RssRegistryState value)
    {
        return new RssRegistryStateSurrogate(value.RssFeeds);
    }
}
```

> [!NOTE]
>
> As you can see only the surrogate is in public. We still choose to expose only controlled information.

<br />

**Implementation**

To implement a VGrain is mostly like implementing a "Grain", but we inherit from the class "VGrainBase" instead:

``` csharp
internal sealed class RssRegistryVGrain : VGrainBase<RssRegistryState, RssRegistryStateSurrogate, RssRegistryStateConverter, IRssRegistryVGrain>, IRssRegistryVGrain
```

With **VGrainBase**, state management is more secure. The converter is invoked whenever the state is loaded or saved, guaranteeing the correct initialization of the **RssRegistryState** object through its constructor.

## 3 - Definitions

Now that you understand how to configure clients and nodes, and implement VGrains, it's time to learn how to orchestrate these components using sequences, triggers, and other definitions.

To Simplify the Setup we will write the definitions in code and store them in the node in memory storage.
To ensure each node have the same definitions we will for each definitions a fix UID.

A good practice is also to create an extensions method that will configure all of them.

``` csharp
// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// keep : Democrite.Framework.Configurations
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Builders;
    using Democrite.Practice.Rss.DataContract;
    using Democrite.Practice.Rss.DataContract.Models;
    using Democrite.Practice.Rss.Node.Models;
    using Democrite.Practice.Rss.Node.VGrains;

    using System;

    public static class DefintionConfigurationExtensions
    {
        public static IDemocriteNodeWizard AddPracticeDefinitions(this IDemocriteNodeWizard builder)
        {
            var rssItemUpdated = Signal.Create("rss-item-updated", fixUid: PracticeConstants.RssItemUpdatedSignalId);

            var loadRssFeedSeq = Sequence.Build("load-rss-items", fixUid: new Guid("52150059-8000-4A19-8416-A3DED9D368AE"))
                                         // Define the input the sequence is expecting
                                         .RequiredInput<RssFeedUrlSource>()

                                         // Call loadasync method on the vgrain IRssVGrain with the key corresponding to source id hash
                                         .Use<IRssVGrain>().ConfigureFromInput(i => i.HashId)
                                                           .Call((g, i, ctx) => g.LoadAsync(i.SourceUri, ctx)).Return

                                         // Loop for each items and apply the following sub-sequence
                                         .Foreach(IType<RssItem>.Default, f =>
                                         {
                                             // For each item update the loaded information calling method UpdateAsync
                                             // on vgrain IRssItemVGrain with the key corresponding the id guid hash
                                             return f.Use<IRssItemVGrain>().ConfigureFromInput(i => i!.Uid)
                                                                           .Call((g, i, ctx) => g.UpdateAsync(i!, ctx)).Return

                                                                           // Fire a signal indicating this item have been updated
                                                                           .FireSignal(PracticeConstants.RssItemUpdatedSignalId).RelayMessage();
                                         })
                                         .Build();

            var importSeq = Sequence.Build("import-rss", fixUid: PracticeConstants.ImportRssSequence)
                                    .RequiredInput<Uri>()
                                    .Use<IRssRegistryVGrain>().Call((g, i, ctx) => g.RegisterAsync(i, ctx)).Return
                                    .CallSequence(loadRssFeedSeq.Uid).ReturnNoData
                                    .Build();

            var refreshAllFeedsSeq = Sequence.Build("refresh-all-inject-feeds", fixUid: new Guid("250EF9E4-6278-4115-97DE-C33D92DC223F"))
                                             .NoInput()
                                             .Use<IRssMonitorVGrain>().Call((g, ctx) => g.GetAllRegistredFeedAsync(ctx)).Return
                                             .Foreach(IType<RssFeedUrlSource>.Default, f =>
                                             {
                                                 return f.CallSequence(loadRssFeedSeq.Uid).ReturnNoData;
                                             })
                                             .Build();

            var autoUpdateTrigger = Trigger.Cron("*/2 * * * *", "auto-update-loop", fixUid: new Guid("7A832833-FFDA-4E08-8E17-764F3E307DAF"))
                                           .AddTargetSequence(refreshAllFeedsSeq.Uid)
                                           .Build();

#if DEBUG
            // Automatically display in the console when a signal flagged is fire
            builder.ShowSignals(rssItemUpdated);
#endif

            builder.AddInMemoryDefinitionProvider(p =>
            {
                p.SetupSignals(rssItemUpdated);
                p.SetupTriggers(autoUpdateTrigger);
                p.SetupSequences(importSeq, loadRssFeedSeq, refreshAllFeedsSeq);
            });

            return builder;
        }
    }
}
```

## 4 - Execution Handler

The final step involves enabling the client to invoke sequences or VGrains within the cluster. This can be achieved by utilizing the IDemocriteExecutionHandler service.

``` csharp

// API
app.MapPut("inject/rss", async ([FromBody] Uri rssUrl, [FromServices] IDemocriteExecutionHandler execHandler, CancellationToken token) =>
{
    var rssInjected = await execHandler.Sequence<Uri>(PracticeConstants.ImportRssSequence)
                                       .SetInput(rssUrl)
                                       .RunAsync(token);

    rssInjected.SafeGetResult();
});

app.MapGet("feed/list", async ([FromServices] IDemocriteExecutionHandler execHandler, CancellationToken token) =>
{
    var listExecResult = await execHandler.VGrain<IRssMonitorVGrain>()
                                          .Call((g, ctx) => g.GetAllRegistredFeedAsync(ctx))
                                          .RunAsync(token);

    var list = listExecResult.SafeGetResult();

    return list;
});

```

# Test

## URL

https://news.mit.edu/rss/topic/artificial-intelligence2 <br />
https://deepmind.google/blog/rss.xml <br />
 <br />
https://www.wired.com/feed/tag/ai/latest/rss <br />
https://rss.nytimes.com/services/xml/rss/nyt/EnergyEnvironment.xml <br />
https://machinelearningmastery.com/blog/feed/ <br />