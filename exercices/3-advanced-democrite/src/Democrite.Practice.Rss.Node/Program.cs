using Democrite.Framework;
using Democrite.Framework.Configurations;
using Democrite.Framework.Core.Abstractions.Enums;

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

                                     // Activate Cron trigger management
                                     .UseCronTriggers()

                                     // Allow client connection to this node
                                     .ExposeNodeToClient()

                                     // Setup all the need definitions
                                     .AddPracticeDefinitions()

                                     // Define mongodb as default storage point
                                     .SetupNodeMemories(n => n.UseMongo(StorageTypeEnum.All));
                                });

await using (node)
{
    await node.StartUntilEndAsync();
}