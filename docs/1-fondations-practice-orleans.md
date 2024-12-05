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

**Choose a folder and create a dotnet solution (SLN) called "MSOOlrean.Practice.Chat".**

You can do it using following command line and open the SLN generated with visual studio:
``` powershell
dotnet create solution -n MSOOlrean.Practice.Chat
```

For this practice we will create only one projet called "**Silo**" using dotnet (8.0 min).<br/>
To expose our API through swagger we will need to projet "Web" type. <br/>
This kind of projet use dotnet core base with by default all the ASPNet core library.

In visual studio you found a lot of template to create a web application with a lot of option. <br/>
If you are not familiar with those we advice follow those simple steps.

- Create a basic console application type
- Open the projet file (csproj). You can right click on the projet -> Edit Project File
- Change root xml tag from 
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
|**Microsoft.Orleans.Server**| 8.2.0| This package refer to all dependency needed to create a MS Orleans silo.|
|**Orleans.Providers.MongoDB**|8.2.0| This package is an extension for MS Orleans that provide mongoDB storage capacity.|
|**OrleansDashboard**|8.2.0| This package is an extension for MS Orleans that provide a web dashboard usefull to see what happen in the cluster.|
|**Swashbuckle.AspNetCore**|7.1.0| This Package allow to create a swagger API with web application to use and test the API.|

<br/>

**Configure the basic of the application**

We will build application using classic service builder, dependency injection and application hosting.
To do so go to the program entry point (by default the file program.cs)

we will create 3 parts:
- Builder creation
- Configuration
- Application

First we start to create an application builder used to create a context where all the services are configured before run.
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

To explose the API and have an easy interface to test it we will use swagger through **Swashbuckle** library.

### Configuration

To configure swagger to simply have to insert in the "Service Configurations" section the following code:

``` csharp
/*
 * Services configurations
 */

// Enabled swagger API file generation
builder.Services.AddSwaggerGen(options =>
{
    // Define doc infomations
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Chat Orlean demo",
        Version = "v1",
    });

    // Mapping MUST also look for minimal API
}).AddEndpointsApiExplorer();
```

[!caution]
> By default, Visual Studio have generated a file "Properties/launchSettings.json" <br/>
> In this file you will found launch configuration used by visual studio. <br />
> To be able to manualy launch locally multiple instances we need to override those configuration in code to prevent using the same port for each instances<br/>
> Resulting in binding issues.<br/>
><br/>
> To do so we need to configure the "UseUrls" with the folling code: <br/>
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

[!tips]
> To acceelarate the debug phase we advise to add te follow code after "app.UseSwaggerUI();". <br/>
> When you launch the debugger visual studio will open by default the configured URL in explorer. <br/>
> But to go to your swagger API interface you need to add /swagger at the end.<br/>
> The following code directly redirect to swagger interface directly when root page is open.
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
>

*At this point when you open launch, it compile and launch on a swagger interface empty.*

### Endpoints

We will now add our api endpoints using minimal API to simplify the process but you can use classic controllers is your are more familliare with.

## 3 - Orleans Configuration
## 4 - User Grain
## 5 - Chat Room Grain
## 6 - Testing

# Conclusion

|Pros|Cons|
|--|--|
|az|aze|

A "correction" of the practice is available [/exercices/1-fondation-2-orleans/](/exercices/1-fondation-2-orleans/). We advise you to do by yourself. <br/>
To run this solution you need to to have a mongodb instance usable by the connection "mongodb://127.0.0.1:27017", we advice to use a docker container.