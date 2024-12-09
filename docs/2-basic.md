Multi-Agent Distributed System - Democrite - Basic
___

<table border="10px !important" align="center">
    <tr border="none">
        <td border="none">
            <image src="docs/logo.png">
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
> TWhile the configuration process is similar for both Node and Client modes, there are some key differences. In **Node mode**, you'll primarily interact with the **DemocriteNode** configuration, whereas in **Client mode**, you'll use the **DemocriteClient** configuration.

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

A Democrite definition is a serializable data structure that describes a single element within the framework, such as a sequence, trigger, or signal. Each definition contains configuration details and metadata specific to its element type. Definitions are uniquely identified by a GUID (Globally Unique Identifier), but you can also access them using a more human-readable alias called a [Reference Id](/docs/4-expert.m#refrence-id). This alias system provides a convenient way to target specific definitions within your project.

The primary purpose of definitions is to be stored in a persistent location like a database, YAML file, or similar storage solution. The Democrite cluster can then load and execute these definitions to manage workflows, triggers, and other functionalities within your system.

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

    [VGrainIdRegexValidator("^[a-zA-Z{1}-[0-9]+$")]

# Sequence

## Execution Context

# Execution Handler
# Trigger / Signal
