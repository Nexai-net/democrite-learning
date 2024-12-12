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

this projet MUST be referenced by the client and the node.

## API

The api will be expose only by the client. 
Use mimimal api like in [fondation orleans practice](/docs/1-fondations-practice-orleans.md#2---api) or classic controller if you are more familiar with it.

|Method|Route|Return|Objectif|
|--|--|--|--|
| PUT | inject/rss | Http Code 200 | This endpoint inject a rss url in the system|
| GET | feed/list | a list of rss feed's url with his hash ID on the system| This endpoint list all the subscribe rss feeds|

## Node Architecture

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
## 2 - VGrain creation
## 3 - 

# Test

## URL

https://news.mit.edu/rss/topic/artificial-intelligence2 <br />
https://deepmind.google/blog/rss.xml <br />
 <br />
https://www.wired.com/feed/tag/ai/latest/rss <br />
https://rss.nytimes.com/services/xml/rss/nyt/EnergyEnvironment.xml <br />
https://machinelearningmastery.com/blog/feed/ <br />