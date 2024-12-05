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

[!caution]
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

[!TIP]
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

[!TIP]
> To Identify the silo you can add the following line
> ``` csharp
> Console.Title = Process.GetCurrentProcess().Id.ToString() + " port :" + apiRandomPort;
> ```

[!TIP]
> You can double-click on the generated binary multiple times to start multiple silos. <br/>
> Each silo will connect to the local MongoDB instance to discover and form a cluster.
>
> If you navigate to http://localhost:8080, you'll see a dashboard displaying all the silos in the cluster.
>
> **Note**: All silos will attempt to bind to port 8080. Only one will succeed. If you close this silo, the dashboard will become inaccessible.
>
> You can easily identify the application owner by looking at the console logs, where you'll see a refresh tick.


[!CAUTION]
> When a silo fails, the remaining silos will attempt to reconnect multiple times.  <br />
> This may result in normal warning or error logs being generated.

## 4 - User Grain
## 5 - Chat Room Grain
## 6 - Testing

# Conclusion

|Pros|Cons|
|--|--|
|az|aze|

A corrected version of this practice is available at [/exercices/1-fondation-2-orleans/](/exercices/1-fondation-2-orleans/). <br/>
We encourage you to try it on your own.

To run this solution, you'll need a MongoDB instance accessible via the connection string "mongodb://127.0.0.1:27017". <br/>
We recommend using a Docker container for this.