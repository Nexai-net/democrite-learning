﻿using Democrite.Framework;
using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions.Enums;
using Democrite.Framework.Extensions.Docker.Abstractions.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var node = DemocriteNode.Create((h, cfg) => cfg.AddJsonFile("AppSettings.json"),
                                b =>
                                {
                                    b.WizardConfig()
                                     .UseMongoCluster(o => o.ConnectionString("127.0.0.1:27017"))
                                     .ConfigureLogging(c => c.AddConsole())
                                     .ConfigureServices(s =>
                                     {
                                         s.AddHttpClient();
                                     })

                                     // Add tools - grains declaration usefull to debug
                                     .AddDebugTools(LogLevel.Information)

                                     .SetupClusterOptions(o =>
                                     {
                                         o.AddConsoleSiloTitleInfo();
                                     })

                                     // Activate Stream Trigger management and configure the default stream in memory
                                     .UseStreamQueues()

                                     // Activate Cron trigger management
                                     .UseCronTriggers()

                                     .EnableDockerHost()

                                     // Allow client connection to this node
                                     .ExposeNodeToClient()

                                     // Setup all the need definitions
                                     .AddPracticeDefinitions()

                                     // Define mongodb as default storage point
                                     .SetupNodeMemories(n =>
                                     {
                                         n.UseMongo(StorageTypeEnum.All);
                                     
                                         n.UseMongoGrainStorage("LookupStorageConfig", buildRepository: true);
                                     });
                                });

await using (node)
{
    await node.StartUntilEndAsync();
}