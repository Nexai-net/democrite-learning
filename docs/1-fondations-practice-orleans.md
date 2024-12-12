Multi-Agent Distributed System - Fondation - Microsoft Orleans
___

# Goal

This tutorial will guide you through the process of creating a silo (Cluster node) using the Microsoft Orleans framework. We will develop a basic multi-user, multi-room chat API application to illustrate the core concepts.

**For this practice you will need:**
- An IDE that support .net devellopment (Advice [Visual Studio 2022+](https://visualstudio.microsoft.com/))
- Docker installed in local or a mongo database instance available

The following instruction will assume a devellopment context based on Visual Studio and Docker. <br/>
The practice was build using dotnet 8.0, newer version could exist and due to retrocompatibility policy you should not have any problems.<br/>
Otherwise we advice you to follow the framework and package version that will be indicated.

# Specifications

## Requirements

- One binary that can be launched multiple times on the same machine.
- All instances of this binary connect together to form a cluster.
- Each instance exposes an API (Swagger) to manipulate the data.
- Until all the silos fail, the service must be available through an API.
- Even after all silos fail, the chat data must remain.
- Sending a message to a room is restricted to logged-in users who have joined the room. Attempts to send messages by unauthorized users should result in an exception error.

## API

Part of the API must offert functionality to manipulate a user.

|Method|Goal|
|--|--|
|**user/login**| A user can log in using the login method with just a username.|
|**user/logout**| A user can log out to end their session. Note that their history must remain, as well as all messages in rooms.|
|**user/room/list**| We can list all the room of a specific user.|

Part of the API must offert functionality to manipulate the chat rooms.

|Method|Goal|
|--|--|
|**chat/room/{roomIdentifiant}/etag**| This method must return the current room ETag. (An ETag is a simple string used to be compared to check for any changes.)|
|**chat/room/{roomIdentifiant}/participants**| This method MUST return all the current participants (online or not) of a chat room.|
|**chat/room/{roomIdentifiant}/join**| This method is used by a user to join a chat room. If the user was already in the room, nothing happens.|
|**chat/room/{roomIdentifiant}/leave**| This method is used by a user to leave a chat room. If the user has already left, nothing happens.|
|**chat/room/{roomIdentifiant}/messages**| This method is used to get all the chat messages. A system must be in place to get only the messages from a specific point.|
|**chat/room/{roomIdentifiant}/send/message**| This method is used to append a new message in the chat room. We need to store who and when the message occur.|

A room is automatically created when at least one user joins it.
A room is never deleted and can be joined without restrictions.

# Step by Step

## 1 - Solution Setup

**Choose a folder and create a dotnet solution (SLN) called "MSOrleans.Practice.Chat".**

You can do it using following command line and open the SLN generated with visual studio:
``` powershell
dotnet create solution -n MSOrleans.Practice.Chat
```

For this practice, we'll create a single project named "**Silo**" using .NET 8.0. <br/>
To expose our API through Swagger, we'll need a "**Web**" project type.<br/>
<br/>
This type of project is based on .NET Core and automatically includes all necessary ASP.NET Core libraries.
Visual Studio offers numerous templates to create web applications with various options.
<br/>
If you're unfamiliar with these templates, follow these simple steps instead:

- Create a basic console application.
- Open the project file (csproj). You can right-click on the project and select "Edit Project File."
- Change the root XML tag from...
``` xml
<Project Sdk="Microsoft.NET.Sdk">
```
TO
``` xml
<Project Sdk="Microsoft.NET.Sdk.Web">
```

Then reload the project as proposed.

**Reference all the needed libray**

You will need for librairies for this practice.

|Package|Minimal Version|Descriptions|
|--|--|--|
|**Microsoft.Orleans.Server**| 8.2.0| This package refers to all dependencies needed to create a MS Orleans silo.|
|**Orleans.Providers.MongoDB**|8.2.0| This package is an extension for MS Orleans that provides mongoDB storage capacity.|
|**OrleansDashboard**|8.2.0| This package is an extension for MS Orleans that provides a web dashboard usefull to seeing what's happening in the cluster.|
|**Swashbuckle.AspNetCore**|7.1.0| This Package allows to create a swagger API with web application to use and test the API.|

<br/>

**Configure the basic of the application**

We'll build our application using the classic service builder, dependency injection, and application hosting patterns.<br/> 
To do this, let's navigate to the program entry point, typically the **Program.cs** file.

We'll structure our code into three main parts:

- **Builder Preparation**: We'll create an application builder, which provides a context to configure services before the application starts.
- **Configurations**: We'll configure various settings, such as database connections, API endpoints, and logging.
- **Application**: We'll define the core logic of our to run application.

``` csharp
/*
 * Builder Preparation 
 */

// Create the application builder
var builder = WebApplication.CreateBuilder();

/*
 * Services configurations
 */

/*
 * Application
 */

var app = builder.Build();

await app.RunAsync();
```

## 2 - API

To expose the API and have an easy interface to test it, we will use swagger through **Swashbuckle** library.

### Configuration

To configure swagger we simply have to insert in the section "Service Configurations" the following code:

``` csharp
/*
 * Services configurations
 */

// Enabled swagger API file generation
builder.Services.AddSwaggerGen(options =>
{
    // Define doc infomation
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Chat Orlean demo",
        Version = "v1",
    });

    // Mapping MUST also look for minimal API
}).AddEndpointsApiExplorer();
```

> [!CAUTION]
> By default, Visual Studio generates a launchSettings.json file containing launch configurations. <br/>
> To manually launch multiple instances locally, we need to override these configurations in code to prevent port conflicts.<br/>
>
> We can achieve this by configuring the UseUrls property as follows:<br/>
> ``` csharp
> var apiRandomPort = Random.Shared.Next(5000, 65530); <br/>
> builder.WebHost.UseUrls("http://localhost:" + apiRandomPort); <br/>
> Console.WriteLine("API : http://localhost:" + apiRandomPort);
> ```

We then need to activate the swagger interface:
``` csharp
/*
 * Application
 */

var app = builder.Build();

// Activate swagger 
app.UseSwagger();

// Activate swagger UI
app.UseSwaggerUI();

await app.RunAsync();
```

> [!TIP]
> To expedite the debugging process, we recommend adding the following code after "app.UseSwagger()" :
>
> ``` csharp
>#if DEBUG
> 
> app.MapGet("/", request =>
> {
>     request.Response.Redirect("swagger");
>    return Task.CompletedTask;
> });
>
> #endif
> ```
>
> When you start the debugger, Visual Studio will automatically open the configured URL in your browser. <br/>
> However, to access the Swagger API interface, you'll need to manually append /swagger to the URL. <br/>
> The code above directly redirects to the Swagger interface when the root page is opened, saving you a step.

*At this point you have a code that compile and launch on a swagger interface, currently empty.*

### Endpoints

We'll now define our API endpoints using Minimal APIs for simplicity. However, you can use traditional controllers if you're more comfortable with that approach.<br/>
Minimal API endpoints should be placed directly after "app.UseSwagger()" within the application section. <br />

Based on the API specification, here's an example structure:

``` csharp

// Setup a group of endpoints dedicated to user
var userGrp = app.MapGroup("/user");

userGrp.MapPost("login", async (string username, CancellationToken token) =>
{
});

userGrp.MapGet("room/list/{username}", async ([FromRoute] string username, CancellationToken token) =>
{
});

userGrp.MapPost("logout", async (string username, CancellationToken token) =>
{
});

// Setup a group of endpoints dedicated to chat room
// Note that we will take the "roomIdentifiant" from the route
var roomGrp = app.MapGroup("/chat/room/{roomIdentifiant}");

roomGrp.MapGet("etag", async ([FromRoute] string roomIdentifiant, CancellationToken token) =>
{
});

roomGrp.MapGet("participants", async ([FromRoute] string roomIdentifiant, CancellationToken token) =>
{
});

roomGrp.MapPost("join", async ([FromQuery] string username, [FromRoute] string roomIdentifiant, CancellationToken token) =>
{
});

roomGrp.MapPost("leave", async ([FromQuery] string username, [FromRoute] string roomIdentifiant, CancellationToken token) =>
{
});

roomGrp.MapGet("messages", async ([FromRoute] string roomIdentifiant, [FromQuery]Guid? lastReceivedMessageId = null, CancellationToken token = default) =>
{
});

roomGrp.MapPost("send/message", async ([FromRoute] string roomIdentifiant, [FromQuery] string username, string message, CancellationToken token) =>
{
});

```

*At this point you must have a compiling application that launched by openning the swagger interface with all the endpoints*

## 3 - Orleans Configuration

Now that our API endpoints and Swagger interface are set up, we need to configure Orleans and its extensions. <br/>
Orleans will handle the [Requirements](#requirements) we've chosen for this practice.

We'll configure the application to be part of the local "default" cluster, using MongoDB as the meeting point and grain state storage.

Add the following code to the configuration section:

``` csharp
var siloPortRandom = Random.Shared.Next(5000, 65530);

builder.Services.AddOrleans(s =>
{
      // Define the address of the mongoDB to use for the diferrent services
    s.UseMongoDBClient("mongodb://localhost")

      // Define mongoDB as the source of information about the other members of the cluster
     .UseMongoDBClustering(options =>
     {
         options.DatabaseName = DB_NAME;

        /*
         * MongoDBMembershipStrategy.SingleDocument
         * 
         * This option parameterizes the extension to store only one document per cluster in the database. It is the most compatible option.
         * You can change the configuration to store one document per silo, but this option only works with MongoDB versions that support transaction systems.
         */
         options.Strategy = MongoDBMembershipStrategy.SingleDocument;
     })

      // Define that mongoDB will be the default place to save the Grain States
     .AddMongoDBGrainStorageAsDefault((MongoDBGrainStorageOptions o) =>
     {
         o.DatabaseName = DB_NAME;
         o.CollectionPrefix = "ChatDemo";
     })

      /*
       * This defines the port binding that the Orleans silo will use.
       * Without configuration, the Orleans silo binds to port 5000.
       * We need to override this value with a random one to allow multiple silo instances to run on the local machine.
       */
     .ConfigureEndpoints(siloPortRandom, 0)

      // This enable the dashboard to see in a beautifull web application what happen in the cluster
     .UseDashboard();
});
```

*At this step you will have a compiling application, an API exposed and an orleans silo connecting to each other to form a cluster*

> [!TIP]
> To Identify the silo you can add the following line
> ``` csharp
> Console.Title = Process.GetCurrentProcess().Id.ToString() + " port :" + apiRandomPort;
> ```

> [!TIP]
> You can double-click on the generated binary multiple times to start multiple silos. <br/>
> Each silo will connect to the local MongoDB instance to discover and form a cluster.
>
> If you navigate to http://localhost:8080, you'll see a dashboard displaying all the silos in the cluster.
>
> **Note**: All silos will attempt to bind to port 8080. Only one will succeed. If you close this silo, the dashboard will become inaccessible.
>
> You can easily identify the application owner by looking at the console logs, where you'll see a refresh tick.


> [!CAUTION]
> When a silo fails, the remaining silos will attempt to reconnect multiple times.  <br />
> This may result in normal warning or error logs being generated.

## 4 - User Grain

Now, let's create our first Grain, a fundamental building block in Orleans. This Grain will handle all user-related data and actions.

Orleans offers several Grain types, but we'll use a Stateful Grain, where the instance key will be the user's username.

A Stateful Grain is identified by a key and have a state object that acts as its memory. This memory is restored when the Grain is activated and persisted to storage when deactivated.

Using **a Grain instance per user** offers several advantages:

- **Quick Data Access**: Active Grains have their memory directly in the silo's RAM.
- **Built-in Concurrency Protection**: Grains are inherently single-threaded, eliminating the need for manual concurrency control.
- **Object-Oriented Paradigm**: A one-to-one mapping between Grains and logical units.

To create a Grain, follow these steps in order:

- **Define the Grain Interface**: This interface outlines the public methods that can be invoked on the Grain.
- **Define the Grain State (Optional)**: If needed, create a class to store the Grain's persistent state.
- **Implement the Grain Class**: This class implements the Grain interface and manages the Grain's state.

### Declaration

A Grain must have a dedicated interface. 

This interface enables remote access across multiple servers. 
To access a Grain, you always interact with its interface get with the **IGrainFactory** service. 

This factory manages the routing of calls, processing, and response handling.

Example:

If we have the following interface
``` csharp
public interface IStorageGrain : IGrainWithStringKey
{
    Task StoreAsync(string key, string Data);
}
```

you will use it like that:

``` csharp
IGrainFactory factory;
var instanceProxy = factory.GetGrain<IStorageGrain>("storage_name");
await instanceProxy.StoreAsync("key", "42");
```

In this scenario, you define a storage Grain that can be instantiated in any silo within the cluster. When you call factory.GetGrain, you won't receive a direct instance of your object, but rather a proxy implementation generated during the build process.

When you invoke the StoreAsync method, the underlying implementation determines the location of the concrete Grain instance. If no instance exists, one is spawned. The parameters are then serialized or cloned and sent (potentially across the network) for execution. The concrete Grain instance receives the data, processes it, and returns a result. This result then follows the reverse path to be returned to the original caller.

This mechanism highlights the importance of defining a Grain interface. The interface serves as a contract for remote interaction, ensuring seamless communication between different silos.

In our practice we will declare the follow grain interface.

``` csharp
/// <summary>
/// Grain dedicated to match one chat user
/// </summary>
public interface IUserGrain : IGrainWithStringKey
{
    /// <summary>
    /// Log a userand return all the room list it participate
    /// </summary>
    Task<IReadOnlyCollection<string>> LoginAsync();
    /// <summary>
    /// Determines whether this user is logged.
    /// </summary>
    Task<bool> IsLogged();
    /// <summary>
    /// Joins a room.
    /// </summary>
    Task JoinRoom(string roomName);
    /// <summary>
    /// Leave a room.
    /// </summary>
    Task LeaveRoom(string roomName);
    /// <summary>
    /// Gets the chat room list.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetChatRoomList();
    /// <summary>
    /// Logouts this user.
    /// </summary>
    Task Logout();
}
```

This interface can only define methods. Our user Grain (Agent) inherits from the **IGrainWithStringKey** interface, informing Orleans that the Grain's key type will be a string.

Orleans supports three key types:
- Guid
- Long
- String

Guid and Long keys can have an optional sub-key of type string.

In our case, we'll use the "username" as the key for our Grain.

### State

We'll need to store some information about the user. The main constraint is that the state object must be serializable and restorable.

> [!TIP]
> A possible unit test, you can populate a state object with values, serialize it to JSON, deserialize it, and compare the results.

Orleans uses a binary serialization based on [Protobuf](https://protobuf.dev/) for efficient network data transfer. A generator creates the necessary data models during the project build process.

To mark objects and properties for serialization, use the [GenerateSerializerAttribute] and [IdAttribute] attributes, respectively. You can apply these attributes to both public and private fields.

> [!CAUTION] 
> For performance reasons, binary mapping might bypass constructors. To ensure proper initialization, override the Activate method of the Grain and call the constructor manually if necessary.

In our case the user state will be like this:

``` csharp
[GenerateSerializer]
public sealed class UserState
{
    #region Fields

    private readonly List<string> _rooms;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="UserState"/> class.
    /// </summary>
    public UserState(IReadOnlyCollection<string>? rooms, string username, bool isLogged)
    {
        this._rooms = rooms?.ToList() ?? new List<string>();
        this.Rooms = this._rooms;
        this.UserName = username;
        this.IsLogged = isLogged;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets all the rooms where the users is currently participating.
    /// </summary>
    [Id(0)]
    public IReadOnlyCollection<string> Rooms { get; }

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    [Id(1)]
    public string UserName { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is logged.
    /// </summary>
    [Id(2)]
    public bool IsLogged { get; set; }
    
    #endregion

    #region Methods
    
    /// <summary>
    /// Joins a room.
    /// </summary>
    internal bool JoinRoom(string roomName)
    {
        if (this._rooms.Contains(roomName))
            return false;
        this._rooms.Add(roomName);
        return true;
    }

    /// <summary>
    /// Leave a room.
    /// </summary>
    internal bool LeaveRoom(string roomName)
    {
        return this._rooms.Remove(roomName);
    }

    #endregion
}
```

### Implementation

To perform a grain implementation you need to inherite frrom you declaration interface and from a base grain class provide by MS Orleans:

``` csharp
/// <summary>
/// Grain implementation of <see cref="IUserGrain"/>
/// </summary>
/// <seealso cref="Silo.Grains.IUserGrain" />
internal sealed class UserGrain : Grain<UserState>, IUserGrain
```

MS Orleans generates various elements during the compilation process, including metadata about Grain implementations. 
A Grain interface can have multiple implementations, each identified by an alias. 

- Your implementation doesn't have to be public to be used, which is useful for managing security access.
- To create a Grain, you must inherit from the Grain or Grain<TState> class. 
- You must also inherit from a Grain interface.

For state management, you need to provide a Storage service in your Grain's constructor.

``` csharp
/// <summary>
/// Initializes a new instance of the <see cref="UserGrain"/> class.
/// </summary>
public UserGrain([PersistentState("Users")] IPersistentState<UserState> storage)
    : base(storage)
{
}
```

An IPersistentState is an interface with read and write methods that abstracts various storage mechanisms, such as databases, files, or memory.
This service is marked with the PersistentStateAttribute to provide specific configuration details. The first parameter defines the state's category (often the collection or table name), and the second parameter specifies the storage configuration name used during setup.

You can access the state through the State property. 
Additionally, methods like WriteStateAsync and ReadStateAsync are available for direct save and restore operations. 
By default, a Grain's state is saved to storage before deactivation. 
However, to protect against unexpected shutdowns (e.g., power outages), it's recommended to save the state whenever changes occur.

As mentioned earlier, state objects might be created without invoking their constructors, especially during the initial activation of a Grain. 
To avoid potential issues, it's recommended to override the OnActivateAsync method and manually create a new state object if necessary.

``` csharp
/// <inheritdoc />
public override async Task OnActivateAsync(CancellationToken cancellationToken)
{
    if (string.IsNullOrEmpty(this.State.UserName))
    {
        var grainId = this.GetGrainId();
        this.State = new UserState(this.State?.Rooms, grainId.Key!.ToString()!, this.State.IsLogged);
        await WriteStateAsync();
    }

    await base.OnActivateAsync(cancellationToken);
}
```

*Except for the specific details explained above, you can implement the Grain declaration following standard naming conventions and requirements.*

## 5 - Chat Room Grain

Following the example of the "UserGrain" try to implement the chat room without more information.

> [!TIP]
> Since .net 8.0 a kind of "new" type of data structure have been introduce "record" <br/>
> A record is an easy way to create D.T.O (Data Transfert Object)
>
> Example:
> ``` csharp
>    [GenerateSerializer]
>    public sealed record class ChatMessage(Guid MessageId, string SenderUserName, string Message, DateTime UTCCreationTime);
> ```
>
> MS Orleans managed the "record" as serializable object.

> [!TIP]
>
> Each MS Orleans instance run in a the application host where dependency injection of service is available. <br />
> If you need any service you just have to ask if in the construction parameters the system will provide it to you
>
> Example for IGrainFactory or logger
>
> ``` csharp
>/// <summary>
>/// Initializes a new instance of the <see cref="ChatRoomGrain"/> class.
>/// </summary>
>public ChatRoomGrain([PersistentState("ChatRooms")] IPersistentState<ChatRoomState> storage,
>                      IGrainFactory grainFactory,
>                      ILogger<IChatRoomGrain> logger)
> ```

## 6 - Testing

To evaluate if you have correclty done the practice test the following scenario.
You can keep the databases between each test.

[ ] Test 1: Simple one user, one chat room, one message

|Step|Action|Description|Excepted Result|If Fails|
|--|--|--|--|--|
|1|Start One Silo|Launch from visual studio or double click on the binary|Orleans starting log. When all is initialized you should see on the console the following line:<br/> *Application started. Press Ctrl+C to shut down.*| Ensure the mongoDB is well started. Clean the database if you prefer|
|2|Open Swagger| If you have copy the console.Title setup you should see on the console title the API port. Go to a navigator an open the localhost:PORT|Swagger interface|Check Http vs https.<br/> Add /swagger to the url if missing|
|3|Login|Call the /user/login with the following name 'demo'|Response 200||
|4|Check Room list|Call the /user/room/list/{name} with the following name 'demo'|Response 200 and content []||
|5|Join Room|Call the /chat/room/{roomIdentifiant}/join with the following username = 'demo' and roomIdentifiant = 'r0' |Response 200||
|6|Check Room list|Call the /user/room/list/{name} with the following name 'demo'|Response 200 and content [ 'r0' ]||
|7|Check Room Messages|Call the /chat/room/{roomIdentifiant}/messages with the following roomIdentifiant = 'r0'|Response 200 and content []||
|8|Send Room Message|Call the /chat/room/{roomIdentifiant}/send/message with the following roomIdentifiant = 'r0', username = 'demo' and message "test 1 validate"|Response 200 and content [ ChatMessage with information ]||
|9|Check Room Messages|Call the /chat/room/{roomIdentifiant}/messages with the following roomIdentifiant = 'r0'|Response 200 and content [ 1: information about message with content = test 1 validate ]||
|10|Close silo|Go to the console and do CTRL+C|The silo server MUST shutdowns||
|11|Restart silo|Restart operation 1 & 2||
|12|Check Room list|Call the /user/room/list/{name} with the following name 'demo'|Response 200 and content [ 'r0' ]||
|13|Check Room Messages|Call the /chat/room/{roomIdentifiant}/messages with the following roomIdentifiant = 'r0'|Response 200 and content [ 1: information about message with content = test 1 validate ]||

This test ensure data are persisted and validate all the simple mechanism to add a message by a user in the chat room

[ ] Test 2: Use test 1 steps with 2 new user 'mika' and 'edouardo' in chat room "r1" and each user let one message<br/>
[ ] Test 3: Use test 2 steps with 2 new user 'remi' and 'manu' in chat room "r2" and each user let one message but this time start 3 server instance to see grain repartition in dashboard (localhost:8080)<br/>
[ ] Test 4: Use test 3 steps with 3 server but try to shutdown a silo during the test<br/>


# Conclusion

A corrected version of this practice is available at [/exercices/1-fondation-2-orleans/](/exercices/1-fondation-2-orleans/). <br/>
We encourage you to try it on your own.

> [!TIP]
> To run the corrected version, you'll need a MongoDB instance accessible via the connection string "mongodb://127.0.0.1:27017". <br/>
> We recommend using a Docker container for this.

This practice demonstrates the ease of using MS Orleans to create a resilient and performant backend. 
While Stateful Grains require a different mindset compared to traditional backend API servers, they offer simplicity in terms of code, comprehension, and development.

This code can be easily packaged into a Docker image and deployed in multiple containers. 
As long as the containers are on the same network and can access the same database, you'll have a cluster that can scale up or down based on your needs.

|Pros|Cons|
|--|--|
|Grain are simple to use ||
|Statefull system allow easy creation performant with a realisation as simple as create a monolith app ||
|Single thread for the grain remote the concurrency/transactionnal problems||
||Statefull system induce a new way of thinking the data managment and process that could be difficult|
||Missing grain id managment to prevent any flood of invalid creations|
||Orleans configuration could by difficult|
