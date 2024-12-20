using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions;
using Democrite.Practice.Rss.DataContract;
using Democrite.Practice.Rss.DataContract.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using System.Diagnostics;

#if DEBUG

// Delay the client to pop to ensure node have been running first
if (Debugger.IsAttached)
    await Task.Delay(3000);

#endif

var builder = WebApplication.CreateBuilder();

builder.Services.AddSwaggerGen(a =>
{
    a.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Democrite RSS Practice",
        Version = "v1",
    });
}).AddEndpointsApiExplorer();

builder.Host.UseDemocriteClient((h, cfg) => cfg.AddJsonFile("AppSettings.json"),
                                b =>
                                {
                                    b.WizardConfig()
                                     .UseMongoCluster(o => o.ConnectionString("127.0.0.1:27017"));
                                });

var app = builder.Build();

app.UseSwagger();

#if DEBUG

app.MapGet("/", request =>
{
    request.Response.Redirect("swagger");
    return Task.CompletedTask;
});

#endif

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

app.MapGet("search", async ([FromServices] IDemocriteExecutionHandler execHandler, string search, CancellationToken token) =>
{
    if (string.IsNullOrEmpty(search))
        return EnumerableHelper<RssUrlSource>.ReadOnly;

    var listExecResult = await execHandler.VGrain<IRssMonitorVGrain>()
                                          .SetInput(search)
                                          .Call((g, i, ctx) => g.SearchAsync(i, ctx))
                                          .RunAsync(token);

    var list = listExecResult.SafeGetResult();

    return list;
});

app.UseSwaggerUI();

app.Run();