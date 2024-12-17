Multi-Agent Distributed System - Democrite - Basic
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

**Democrite is an open-source framework for building robust, scalable and distributed 'multi-agent like' system based on [Microsoft Orleans](https://github.com/dotnet/orleans).**

This explanation assumes familiarity with:

- **Multi-Agent Paradigm**: A system consisting of multiple, autonomous entities that collaborate to achieve a common goal.
- **Distributed Systems**: Systems where components are located on multiple computers and communicate with each other.
- **Microsoft Orleans**: An open-source framework for building distributed systems using the virtual actor model.

If you are not we advice you to refer to cours call [fondation](/docs/1-fondations.md)

**Packages**

All necessary packages are available on nuget.org with the prefix  [Democrite Framework](https://www.nuget.org/packages?q=democrite+framework&includeComputedFrameworks=true&prerel=true&sortby=relevance)

**Key Terminology:**
- **Node** : Equivalent to an MS Orleans Silo. Democrite uses "**Node**" for clarity regarding its role in a cluster.
- **Agent** : Corresponds to an MS Orleans Grain. Democrite emphasizes "Agent" to reflect the framework's focus on autonomous entities.
- **VGrain (Virtual Grain)**: Democrite uses this term due to its specific ID construction process (explained in the VGrains section). [cf. VGrains Section](#vgrains---agent-type)
- **Bag** : A container that groups agents, models, or blackboard controllers to work together. Bags are designed for reuse across projects.

# Configuration

Similar to MS Orleans, Democrite can be integrated into a host builder using extension methods. However, it can also be configured and run independently as a standalone application.

Attach to an existing host.
``` csharp

IHostBuilder builder;
builder.Host.UseDemocriteNode(b =>
{
    b.WizardConfig()
     ...
});

```

or Standalone
``` csharp
var node = DemocriteNode.Create(b =>
{
    b.WizardConfig()
     ...
});

```
When integrated into an existing host, the framework will be configured as part of the host's startup process and will begin operating when the host is launched. In the standalone scenario, the framework will create and manage its own host environment, requiring you to initiate the startup process.

> [!NOTE]
>
> While the configuration process is similar for both Node and Client modes, there are some key differences. In **Node mode**, you'll primarily interact with the **DemocriteNode** configuration, whereas in **Client mode**, you'll use the **DemocriteClient** configuration.

## Node vs Client

Like *Microsoft Orleans*, **Democrite** operates in two primary modes: **Node** and **Client**.

- **Node**: A Node is a direct participant in the execution cluster. When launched, it joins the cluster and actively participates in processing tasks.
- **Client**: A Client, as the name suggests, is external to the cluster. It can connect to the cluster to issue orders or requests and subsequently disconnect. A single Client can connect to multiple clusters and/or Nodes.

## Why Node and Client?

This division offers several advantages:

- **Security**: By isolating the cluster on a secure infrastructure, you can limit access to sensitive data and operations. Clients can then connect from less secure environments to interact with the cluster.

- **Scalability**: This separation allows for independent scaling of the execution layer (Nodes) and the interface layer (Clients). For instance, you can have a few Clients (e.g., API servers) handling user requests, while a large cluster of Nodes processes those requests.

## Best Practices:

It's recommended to design your system with separate Client and Node components from the outset. This proactive approach simplifies future scaling and security enhancements.

# Definition

A Democrite definition is a **serializable** data structure that describes a single element within the framework, such as a sequence, trigger, or signal. Each definition contains configuration details and metadata specific to its element type. Definitions are uniquely identified by a GUID (Globally Unique Identifier), but you can also access them using a more human-readable alias called a [Reference Id](/docs/4-expert.m#refrence-id). This alias system provides a convenient way to target specific definitions within your project.

The primary purpose of definitions is to be stored in a persistent location like a database, YAML file, or similar storage solution. The Democrite cluster can then load and execute these definitions to manage workflows, triggers, and other functionalities within your system.

To facilitate definition creation, we leverage builder objects. These tools assist in providing essential and optional details, while guaranteeing adherence to validity rules at compile time. For instance, in sequence definitions, they enforce type consistency across all transformation steps.

To maintain separation between definition creation and execution, Democrite utilizes the [Democrite.Framework.Builders](https://www.nuget.org/packages/Democrite.Framework.Builders) package. This package provides tools and functionalities dedicated solely to building various types of definitions, ensuring a clean separation between definition logic and runtime execution.

Example: Building a Sequence

``` csharp
var seqDefinition = Sequence.Build("simple-name-identifier",
                                   fixUid: new Guid("20337617-FFF9-4DA9-A200-2BAD8EBA619E"),
                                   metadataBuilder: m =>
                                   {
                                       m.Namespace("test-demo")
                                        .Description("This sequence is for help purpose");
                                   })
                            .RequiredInput<string>()
          
                            ...
          
                            .Build();
```

> [!CAUTION]
>
> When creating a definition, you have the option to either assign a unique ID yourself or let the system generate one automatically. While this flexibility works well for single-node clusters, it can lead to issues in multi-node environments.
>
> If multiple nodes attempt to trigger a sequence with the same configuration but different automatically generated IDs, the system may not recognize them as identical.
>
> To ensure consistent behavior across multiple nodes and enhance security, we strongly recommend using fixed IDs for each definition. This approach guarantees that definitions with the same configuration will be recognized as identical, regardless of the node that triggers them.

Democrite leverages the **strategy** pattern to provide a flexible and adaptable approach to accessing **definitions**.This pattern involves a single entry point, the *IDefinitionService*, which can dynamically select the most appropriate strategy based on specific conditions. In our case, this means that while we have a unified interface for accessing definitions, the underlying implementation can vary depending on the source provider. This allows for diverse data sources, such as databases, configuration files, or remote services, to be seamlessly integrated into the framework.

By example:

``` csharp
var node = DemocriteNode.Create(b =>
{
    b.WizardConfig()
     ...


     // Add a sequence definition in the node memory
     .AddInMemoryDefinitionProvider(b =>{
        b.SetupSequences(seqDefinition)
     })

    // Define mongo db as definition source to add all the definition stored in collection "Definitions"
    // Extension bll: Democrite.Framework.Extensions.Mongo
    .AddMongoDefinitionProvider(o => o.ConnectionString("127.0.0.1:27017"))
});
```

In this example with have 2 sources of definitions:
- **In Memory** : This source is setup by default.
- **MongoDB** : This source is provide by the package [Democrite.Framework.Extensions.Mongo](https://www.nuget.org/packages/Democrite.Framework.Extensions.Mongo/)

<br/>
At start all the definitions will be loaded and start when needed.

# VGrains - Agent Type

See in [Fondation](/docs/1-fondations.md#grain), Miscrosoft orleans use the actor-model type thought an element call **grain**.

In accordance with Orleans terminology, a grain is a **virtual** actor that can appear in any compatible silo, with its state being restored if necessary.

In Orleans, to invoke a grain, one must request a proxy instance from the **IGrainFactory**. This proxy seamlessly manages the communication between the caller and the called. This is why a grain consists of an interface and an implementation, allowing the proxy to inherit the interface.

With Democrite, there is **no need to explicitly call the grain yourself**, it will do it for you based on the configuration.

This is the reason we refer to them as Virtual Grains (VGrain), to denote a behavior that prevent direct call consumption.

Democrite has refined the agent pattern, shifting the responsibility for determining usage scenarios from the user to the agent itself. This approach mitigates issues arising from misapplied calling rules, such as unnecessary grain creation. By empowering agents to self-declare their intended use cases, the system becomes more efficient and responsive.

## VGrain Id Configuration & Rules

A **VGrain** can establish rules for its automatic ID construction, which enables seamless integration and automated usage within sequences and other framework components.

``` csharp

// This attribute define the format of the expected grain key
[VGrainIdFormat(IdFormatTypeEnum.Guid, FirstParameterTemplate = "{input.Key}",
                                       FirstParameterFallback = "1DD93765-06A7-4368-81D9-828EECBE2767",
                                       SecondParameterTemplate = "{executionContext.Configuration}",
                                       SecondParameterFallback = "global")]

// This attribute flag the grain as singleton, meaning only one instance is allow in the cluster
[VGrainIdSingleton()]

// This attribute provide meta data describing the VGrain
[VGrainMetaData("A3DB6159-E75D-4EBC-B8F8-D9B6B148AB53", 
                "example-sni-grain",
                namespaceIdentifier: "test-bag",
                description: "This VGRain demonstrate the attributes behavior")]

public interface IExampleVGrain : IVGrain
{

}

```

> [!NOTE]
>
> In comparaison to classic orlean **Grain** where you need to inherite "IGrainWith..." interface here you just need to inherite from *IVGrain*

The provided example demonstrates an automatic VGrain configuration pattern. In simpler terms, the grain ID is constructed using a primary key (a GUID) extracted from the method call's input parameter named "**Key**". If this parameter is missing or invalid, a fallback value ("**FirstParameterFallback**") is used. Additionally, a secondary key is derived from the execution context's "**Configuration**" property. If this property is also missing or invalid, another fallback value ("**SecondParameterFallback**") is employed. 

This dynamic ID construction allows for flexible and context-aware VGrain selection based on both input parameters and execution context.

For example if through a sequence we have:
- As input
``` json
{
    "Key": "3FF66EB1-6F8E-40DF-8EA5-DA612C9B5E3A",
    "Message": "Hello, world"
}
```
- As Execution Context
``` json
{
    "Configuration": "r0",
}
```

This we will call the vgrain id "3FF66EB1-6F8E-40DF-8EA5-DA612C9B5E3A-r0".
This creates a dynamic calling system that adapts the target based on usage, providing a flexible and context-aware approach to interaction.

## Methods

While Microsoft Orleans requires manual invocation of grains and their methods, Democrite leverages definitions to specify what to call and with which information. To streamline this calling process further, Democrite imposes a specific rule:

***Exposed methods must have either zero or one custom parameter, but they must always include an IExecutionContext parameter.***

This constraint simplifies method invocation and enhances the framework's ability to dynamically determine the appropriate target based on context.

Example:
``` csharp

// Valid
Task ValidDemocriteCall(string poney, IExecutionContext ctx);

Task OtherValidDemocriteCall(IExecutionContext ctx);

// Invalid
Task InvalidDemocriteCall();

Task InvalidDemocriteCall(string poney);

Task InvalidDemocriteCall(string poney, string other, IExecutionContext ctx);
```

> [!NOTE]
>
> This method could be "Generic"
> ``` csharp
>
> // Valid
> Task ValidDemocriteCall<TInput>(TInput poney, IExecutionContext ctx);
> 
> Task ValidDemocriteCall<TConfig>(string poney, IExecutionContext<TConfig> ctx);
> ```

This rule facilitates the management of automatic calls within sequences, triggers, and other mechanisms. The mandatory **IExecutionContext** parameter provides essential information for monitoring, embedding complex data, and enabling specialized behaviors, thereby enhancing the flexibility and traceability of the system.

## Implementation

To implement a VGrain is kind like MS Orlean instead of inherite of clas "Grain" you have to inherite from class "VGrainBase".

**Grain Id Validation**

In the follow of the ID automatic generation an implementation could add some validator to ensure the id generate follow the rules.

``` csharp
[VGrainIdRegexValidator("^[a-zA-Z{1}-[0-9]+$")]
```

**State**

A key distinction between traditional Orleans Grains and Democrite VGrains lies in their flexibility regarding state management and usage. VGrains offer greater adaptability, allowing for more dynamic and flexible system design.

No State:
``` csharp
public sealed class ImplementationVGrain : VGrain<IImplementationVGrain> : IImplementationVGrain
```

Serializable State:
``` csharp
public sealed class ImplementationVGrain : VGrain<SerializableState, IImplementationVGrain> : IImplementationVGrain
```

State with serializable surrogate:
``` csharp
public sealed class ImplementationVGrain : VGrain<SerializableState, SerializableStateSurrogate, SerializableStateConverter, IImplementationVGrain> : IImplementationVGrain
```

> [!NOTE]
>
> In Microsoft Orleans, a Surrogate is a simple, serializable structure that serves as a data container. To utilize surrogates, you must define a  corresponding **IConverter** class responsible for transforming the original object into a savable Surrogate and vice versa. This approach simplifies state management by eliminating the need for direct serialization of the original object, which might have complex or non-serializable components.

# Sequence

A **Sequence** is a predefined set of steps that are executed in a specific order. Each step can take input from the previous step and produce output that can be used as input for the next step. 

To run a Sequence, only its unique identifier (Uid) needs to be provided or it's reference Id.

## Definitions

To configure and test a **sequence** you need to create and register it.

**Build definition**
```csharp
var collectorSequence = Sequence.Build("sequence-sni")
                                // Ask a web URI in input
                                .RequiredInput<Uri>()

                                // Fetch html page and return it
                                .Use<IHtmlCollectorVGrain>().Call((a, url, ctx) => a.FetchPageAsync(url, ctx)).Return

                                // Configure inspector on specific pair inspect and extract current value
                                .Use<IPriceInspectorVGrain>().Configure(currencyPair)
                                                             .Call((a, page, ctx) => a.SearchValueAsync(page, ctx)).Return

                                // Store the value received into a dedicated statefull grain
                                .Use<ICurrencyPairVGrain>().Configure(currencyPair)
                                                           .Call((a, data, ctx) => a.StoreAsync(data, ctx)).Return
                                .Build();
```

In the present example, we observe the process of building a sequence definition.
- **Meta Information**: We begin by providing fundamental metadata, including the Simple Name Identifier (SNI), a fixed Unique Identifier (UID), and a descriptive explanation.
- **Input Specification**: Next, we specify the type of input the sequence anticipates. This input may be optional.
- **Step Definition: Subsequently**, we construct the individual steps of the sequence. In this particular case, three steps are defined, each designed to call a VGrain with a designated target method.
- **Definition Finalization**: Finally, the Build method is employed to finalize the definition, encapsulating all the specified components.

"collectorSequence" is a definition you can store. <br/>
For example in the node memory:

```csharp
.AddInMemoryDefinitionProvider(m =>
{
    // Local in node memory setup
    .SetupSequences(collectorSequence);
})
```

A Sequence can perform the following actions:

- **Data Transformation**: Modify or manipulate data without relying on a VGrain.
- **Signal Sending**: Trigger events or notifications to other systems.
- **Sequence Invocation**: Execute another predefined Sequence.
- **Iterative Processing**: Loop through a collection of data, applying a Sequence to each item.

## Execution Context

The context parameter (ctx) in sequence definitions is a crucial element for managing the execution context of a Sequence. It carries the following information:

- **CancellationToken**: This token allows for graceful cancellation of the Sequence if required.
- **FlowUid**: A unique identifier shared by all steps within the same Sequence execution.
- **CurrentExecutionId**: A unique identifier for the specific call within the current Sequence execution.
- **ParentExecutionId**: The unique identifier of the previous step in the Sequence.

Additionally, the context parameter can be used to store configuration information and data. By calling the *.Configure* method on a step, you can add configuration settings to the **Configuration** property of the context. This configuration can be used to target specific VGrains or other resources.  (cf. [VGrain Id](#vgrain-id-configuration--rules)).

To handle multiple pieces of information that cannot be passed as a single parameter, you can use the PushToContext method to store data within the context. This data can then be retrieved later in the Sequence using the appropriate mechanisms.

# Execution Handler

In MS Orleans, you use **IGrainFactory** service to create and call standard grains.
In Democrite we use **IDemocriteExecutionHandler**.

This service is specifically designed for **VGrains**. It offers two main functionalities:
- **Direct Grain Calls**: You can directly call a VGrain using its ID, which is automatically resolved based on the current context.
- **Sequence Execution**: You can initiate a sequence execution, which involves a series of steps that can include VGrain calls.

**VGrain Direct**
```csharp
var result = await _executionHandler.VGrain<IStringTestVGrain>()
                                    .SetInput(input)
                                    .SetConfiguration(cfg)
                                    .Call((g, i, ctx) => g.ConcatResultAsync(i, ctx))
                                    .RunAsync(token);
```

**Sequence**

```csharp
var execResult = await this._democriteExecutionHandler.Sequence<string>(simpleSeq.Uid)
                                                      .SetInput(inputTest)
                                                      .RunAsync<string>(token);
```

Each call return a "IExecutionResult" that contains the response value or exception information.

to get the result or throw the exception if occured you have the following method:
```csharp
var result = execResult.SafeGetResult();
```

> [!NOTE]
>
> The **IExecutionHandler** interface is primarily designed for client-side usage. It enables clients to interact with the **Democrite** framework, including executing Sequences and calling VGrains.

# Trigger / Signal

Previously we see how to call a sequence or a VGrain manually but a complex system requiered more automatic way to to it.

**Democrite** offers several built-in mechanisms to support event-driven architectures:

- **Triggers**: Triggers are event-driven components that can be activated by various events, such as timers, signals, or messages from message queues. When activated, a trigger can initiate a sequence of actions, send signals, or perform other tasks.

- **Signals**: Signals are lightweight, asynchronous notifications that can carry small amounts of data. They are often used to trigger actions in other parts of the system, such as logging events or initiating workflows.

- **Doors**: Doors are conditional triggers that listen to one or more signals or doors. When the specified conditions are met, the door emits its own signal, triggering further actions.

## Signal

To fire a signal you need to create it first with a definition and then use the "ISignalService".
> [!NOTE]
> 
> Democrite offers a signal hierarchy feature that enables you to organize signals into a hierarchical structure. A parent signal can have one or > more child signals. When a child signal is raised, its parent signals are also raised, propagating the event up the hierarchy. This mechanism is > particularly useful for:
> 
> - **Centralized Monitoring**: You can monitor high-level parent signals to get an overview of system behavior.
> - **Specific Event Handling**: Child signals allow you to react to more specific events without being overwhelmed by a flood of generic events.

## Trigger

A trigger must be defined before it can be used. Once defined, the trigger will become active as soon as at least one node in the cluster is ready to process events.

**Cron**

A cron trigger is one who will fire following a formated period of time ([Norme](https://en.wikipedia.org/wiki/Cron)):

```csharp
// Every minutes between 9h and 18h UTC between monday and friday
var triggerDefinition = Trigger.Cron("* 9-18 * * mon-fri")

// Every second
var triggerDefinition = Trigger.Cron("* * * * *")
```

To enable the "Cron" trigger you need to add the extension library "Democrite.Framework.Node.Cron" and call the method "UseCronTriggers" during the configuration of the engine.
Example:

```csharp

builder.Host.UseDemocriteNode(b =>
{
    b.WizardConfig()
     .NoCluster()
     .ConfigureLogging(c => c.AddConsole())
     
     // Enabled cron triggers and activate the "second" support
     .UseCronTriggers(supportSecondCron: true);
});

```

> [!CAUTION]
>
> A Cron jobs typically operate with a minimum interval of 1 minute and use 5 parameters to define the schedule. To achieve higher precision, you can enable second-level scheduling by adding a sixth parameter during the configuration process. This allows you to define Cron jobs that execute at specific seconds within a minute.

**Signal**

Built-in triggers can react to incoming signals.

```csharp
// Trigger when the signal corresponding to SIGNAL_ID have been fire
var triggerDefinition = Trigger.Signal(SIGNAL_ID)
```

> [!NOTE]
> 
> The framework uses internal signals that do not require explicit activation.

## Carried Data

Triggers and signals can be used to transmit data. When a trigger or signal is activated, it can carry specific information. For instance, the "NewBlock" signal can include the ID of the newly created block to provide context to the receiving components.

> [!NOTE]
>
> To ensure traceability and prevent unintended infinite loops, signals in Democrite maintain a history. When a signal triggers another, the newly created signal carry also the ID and content of the previous signal. This history chain can be useful for debugging and tracing the flow of events. However, it's essential to design signal flows carefully to avoid excessive signal propagation, which can lead to performance bottlenecks and make debugging difficult. By limiting the depth of signal chains and using appropriate filtering mechanisms, you can effectively manage signal propagation and maintain system performance.

Triggers in Democrite are capable of sending data. They can relay incoming messages from signals or stream queues. However, Cron triggers, which operate on a predefined schedule, don't have an inherent source of data. To address this, you can configure a DataSource within the trigger definition. This DataSource can provide the necessary information to be sent by the trigger, ensuring that the trigger can deliver relevant data even without an immediate external event.

Exemple:

```csharp
Trigger.Cron("* * * * *")
       
       // Set the default output to static collection with cicling mode
       .SetOutput(d => d.StaticCollection<Uri>(new[]
       {
           new Uri("https://github.com/Nexai-net/democrite"),
           new Uri("https://www.amexio-group.com/"),
           new Uri("https://en.wikipedia.org/wiki/Cron"),
       }).PullMode(PullModeEnum.Circling))

       .AddTargetSequence(SeqId)
       .Build();
```

In this example, the sequence with ID "SeqId" will be triggered every second. Each trigger will receive a URL from a static collection, cycling through the URLs in a round-robin fashion.

> [!NOTE]
>
>A trigger can be configured to target multiple destinations, such as sequences, streams, or signals. For each target, you can optionally specify a data source. This data source will provide the input data for the target.
>
>If no specific data source is defined for a target, the trigger's default data source will be used. If the trigger itself doesn't have a default data source, the input data associated with the trigger event will be used, if available.

## Others

In addition to built-in triggers, Demoiselle allows you to create custom triggers. This flexibility enables you to tailor triggers to specific use cases. For instance, StreamQueue triggers are designed to monitor data streams and fire events whenever new messages are received. To learn how to create custom triggers, refer to the "Expert" documentation.

# Practice

In this tutorial series, we'll use Democrite to build a web application that scrapes RSS feeds, analyzes content, and provides a basic search function.

**Part 1**: Foundation and RSS Feed Integration --> [Practice](/docs/2-basic-practice-democrite.md)

In this initial part, we'll cover:

- **Project Structure**: Setting up the core application structure.
- **RSS Feed Loading**: Fetching and storing articles from a specified RSS feed.
- **Auto-Refresh**: Implementing an automatic refresh mechanism to keep the content up-to-date.

Subsequent parts will delve deeper into advanced topics, including:

- **Content Analysis**: Processing and understanding the content of articles.
- **Search Engine Implementation**: Building a robust search functionality.

<h2>Stay tuned for the advanced tutorial series!</h2>