// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using Orleans.Providers.MongoDB.Configuration;

using Silo.Grains;

using System.Diagnostics;
using System.Text.RegularExpressions;

const string DB_NAME = "PracticeChat";

var builder = WebApplication.CreateBuilder();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Chat Orlean demo",
        Version = "v1",
    });

}).AddEndpointsApiExplorer();

var siloPortRandom = Random.Shared.Next(5000, 65530);

var apiRandomPort = Random.Shared.Next(5000, 65530);
builder.WebHost.UseUrls("http://localhost:" + apiRandomPort);

Console.WriteLine("API : http://localhost:" + apiRandomPort);

builder.Services.AddOrleans(s =>
{
    s.UseMongoDBClient("mongodb://localhost")
     .UseMongoDBClustering(options =>
     {
         options.DatabaseName = DB_NAME;
         options.Strategy = MongoDBMembershipStrategy.SingleDocument;
     })
     .AddMongoDBGrainStorageAsDefault((MongoDBGrainStorageOptions o) =>
     {
         o.DatabaseName = DB_NAME;
         o.CollectionPrefix = "ChatDemo";
     })
     .ConfigureEndpoints(siloPortRandom, 0)
     .UseDashboard();
});

var app = builder.Build();

app.UseSwagger();

var validNameReg = new Regex("^[a-zA-Z0-9]+$");

#if DEBUG

app.MapGet("/", request =>
{
    request.Response.Redirect("swagger");
    return Task.CompletedTask;
});

#endif

Console.Title = Process.GetCurrentProcess().Id.ToString();

var userGrp = app.MapGroup("/user");

userGrp.MapPost("login", async (string username, [FromServices] IGrainFactory factory, CancellationToken token) =>
{
    EnsureNameIsValid(username, validNameReg);

    var userGrain = factory.GetGrain<IUserGrain>(username);
    return await userGrain.LoginAsync();
});

userGrp.MapGet("room/list/{username}", async ([FromRoute] string username, [FromServices] IGrainFactory factory, CancellationToken token) =>
{
    EnsureNameIsValid(username, validNameReg);

    var userGrain = factory.GetGrain<IUserGrain>(username);
    return await userGrain.GetChatRoomList();
});

userGrp.MapPost("disconnect", async (string username, [FromServices] IGrainFactory factory, CancellationToken token) =>
{
    EnsureNameIsValid(username, validNameReg);

    var userGrain = factory.GetGrain<IUserGrain>(username);
    await userGrain.Logout();
});

var roomGrp = app.MapGroup("/chat/room/{roomIdentifiant}");

roomGrp.MapGet("etag", async ([FromRoute] string roomIdentifiant, [FromServices] IGrainFactory factory, CancellationToken token) =>
{
    EnsureNameIsValid(roomIdentifiant, validNameReg);

    var room = factory.GetGrain<IChatRoomGrain>(roomIdentifiant);
    return await room.GetRoomEtag();
});

roomGrp.MapGet("participants", async ([FromRoute] string roomIdentifiant, [FromServices] IGrainFactory factory, CancellationToken token) =>
{
    EnsureNameIsValid(roomIdentifiant, validNameReg);

    var room = factory.GetGrain<IChatRoomGrain>(roomIdentifiant);
    return await room.GetParticipants();
});

roomGrp.MapPost("join", async ([FromQuery] string username, [FromServices] IGrainFactory factory, [FromRoute] string roomIdentifiant, CancellationToken token) =>
{
    EnsureNameIsValid(username, validNameReg);
    EnsureNameIsValid(roomIdentifiant, validNameReg);

    var userGrain = factory.GetGrain<IUserGrain>(username);
    await userGrain.JoinRoom(roomIdentifiant);
});

roomGrp.MapPost("leave", async ([FromQuery] string username, [FromServices] IGrainFactory factory, [FromRoute] string roomIdentifiant, CancellationToken token) =>
{
    EnsureNameIsValid(username, validNameReg);
    EnsureNameIsValid(roomIdentifiant, validNameReg);

    var userGrain = factory.GetGrain<IUserGrain>(username);
    await userGrain.LeaveRoom(roomIdentifiant);
});

roomGrp.MapGet("messages", async ([FromRoute] string roomIdentifiant, [FromServices] IGrainFactory factory, [FromQuery]Guid? lastReceivedMessageId = null, CancellationToken token = default) =>
{
    EnsureNameIsValid(roomIdentifiant, validNameReg);

    var room = factory.GetGrain<IChatRoomGrain>(roomIdentifiant);
    return await room.GetMissingMessage(lastReceivedMessageId);
});

roomGrp.MapPost("send/message", async ([FromRoute] string roomIdentifiant, [FromServices] IGrainFactory factory, [FromQuery] string username, string message, CancellationToken token) =>
{
    EnsureNameIsValid(username, validNameReg);
    EnsureNameIsValid(roomIdentifiant, validNameReg);

    var room = factory.GetGrain<IChatRoomGrain>(roomIdentifiant);
    return await room.SendMessage(username, message);
});

app.UseSwaggerUI();
await app.RunAsync();

static void EnsureNameIsValid(string name, Regex validNameReg)
{
    ArgumentNullException.ThrowIfNullOrEmpty(name);

    if (!validNameReg.IsMatch(name))
        throw new InvalidDataException("Invalid name must be composed only of alpha numerical values");
}