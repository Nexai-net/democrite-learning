// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// keep : Democrite.Framework.Configurations
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Bag.DebugTools;
    using Democrite.Framework.Builders;
    using Democrite.Framework.Builders.Artifacts;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Practice.Rss.DataContract;
    using Democrite.Practice.Rss.DataContract.Models;
    using Democrite.Practice.Rss.Node.Models;
    using Democrite.Practice.Rss.Node.VGrains;

    using System;
    using System.Diagnostics;
    using YamlDotNet.Core;

    public static class DefinitionConfigurationExtensions
    {
        /// <summary>
        /// Generate needed definition and store it in the node memory
        /// </summary>
        public static IDemocriteNodeWizard AddPracticeDefinitions(this IDemocriteNodeWizard builder)
        {
            var rssItemUpdated = Signal.Create(PracticeConstants.RssItemUpdatedSignalId);

            var loadRssFeedSeq = Sequence.Build("load-rss-items", fixUid: new Guid("52150059-8000-4A19-8416-A3DED9D368AE"))

                                         // Define the input expected type
                                         .RequiredInput<UrlSource>()

                                         // Use the vgrain IRssVGrain with the key matching "HashId" property on the input objet
                                         .Use<IRssVGrain>().ConfigureFromInput(i => i.HashId)

                                                           // Call in the VGrain IRssVGrain the method LoadAsync to load and parse the RSS XML target by the URL in property "SourceUri"
                                                           .Call((g, i, ctx) => g.LoadAsync(i.SourceUri, ctx)).Return

                                         // LoadAsync Methods will returned all the items parsed in objects RssItem
                                         .Foreach(IType<RssItem>.Default, f =>
                                         {
                                             // 'f' is a sub-sequence builder auto configured based on collection type IType<RssItem>.Default

                                             // Foreach RssItem we use the IRssItemVGrain VGrain with the key matching RssItem UID (Hash combine of feed Uid and Item Guid (XML property)
                                             return f.Use<IRssItemVGrain>().ConfigureFromInput(i => i!.Uid)

                                                                           // Call in the VGrain IRssItemVGrain the method UpdateAsync to store or update the current RssItem Content
                                                                           .Call((g, i, ctx) => g.UpdateAsync(i!, ctx)).Return;

                                                   // Fire a signal with RssItem id information for future threatment
                                                   // The fire is now done the IRssItemVGrain only when the data have changed
                                                   //.FireSignal(PracticeConstants.RssItemUpdatedSignalId).RelayMessage();
                                         })
                                         .Build();

            var importSeq = Sequence.Build("import-rss", fixUid: PracticeConstants.ImportRssSequence)

                                     // Define the input expected URI
                                    .RequiredInput<Uri>()

                                    // Use the vgrain IRssRegistryVGrain without specific key (singleton) and call the method RegisterAsync in it to register the URL and produce a normalize Hash Id from the URL
                                    //.Use<IRssRegistryVGrain>().Call((g, i, ctx) => g.RegisterAsync(i, ctx)).Return

                                    // No more need to register the url due to repository system
                                    // We are using instead a generic vgrain that could easily be re-use
                                    .Use<IHashToolVGrain>().Call((g, i, ctx) => g.HashAsync(i, ctx)).Return

                                    // Call the sequence load-rss-items to load all the rss feed items
                                    .CallSequence(loadRssFeedSeq.Uid).ReturnNoData

                                    .Build();

            var refreshAllFeedsSeq = Sequence.Build("refresh-all-inject-feeds", fixUid: new Guid("250EF9E4-6278-4115-97DE-C33D92DC223F"))
            
                                              // Define that no input are expected
                                             .NoInput()

                                             // Use the vgrain IRssMonitorVGrain to get all the registred url

                                             /*
                                              * The implementation of the vgrain IRssMonitorVGrain and IRssRegistryVGrain are the same.
                                              * But IRssRegistryVGrain is internal, meaning only the current library can see and use it but IRssMonitorVGrain is public.
                                              * This allow the client to directly calls the IRssMonitorVGrain is needed without the option to add feed.
                                              */
                                             .Use<IRssMonitorVGrain>().Call((g, ctx) => g.GetAllRegistredFeedAsync(ctx)).Return

                                             // GetAllRegistredFeedAsync return a collection of UrlSource containing the URL and its HashId associate
                                             .Foreach(IType<UrlSource>.Default, f =>
                                             {
                                                 // For each feed URL call the sequence load-rss-items to reload the feed information
                                                 return f.CallSequence(loadRssFeedSeq.Uid).ReturnNoData;
                                             })
                                             .Build();

            // Define a trigger that will fire in internal of 2 minutes (Minutes % 2 == 0) and call the sequence refresh-all-inject-feeds
            var autoUpdateTrigger = Trigger.Cron("* * * * *", "auto-update-loop", fixUid: new Guid("7A832833-FFDA-4E08-8E17-764F3E307DAF"))
                                           .AddTargetSequence(refreshAllFeedsSeq.Uid)
                                           .Build();

            // Define a stream queue based on default configuration
            // Without any orlean configuration the stream will be persisted in the Cluster memory
            // Meaning if all nodes cluster are down the message in the stream queue will be lost
            var rssItemIndexationStream = StreamQueue.CreateFromDefaultStream("indexation-queue", "indexation-stream-key", fixUid: new Guid("E5881B0B-3749-4B18-8B0C-3B734F6373AF"));

            // Define a trigger that will listen a signal a relay the signal's content to a stream queue for future treatments
            var triggerToInjectInStreamQueue = Trigger.Signal(rssItemUpdated.SignalId, "from-signal-to-queue", fixUid: new Guid("D25CF6FF-C1D5-473D-B261-000F4A748B8E"))
                                                      .AddTargetStreams(rssItemIndexationStream)
                                                      .Build();

            //
            // Create external VGrain using a python script to be executed
            //
            // Need python 3.9+ installed
            // Need python library democrite 'pip install democrite'
            // Need python library lxml 'pip install lxml'
            //
            var extractorArtifact = Artifact.VGrain("html-extrator", uid: new Guid("E7C685D4-DB74-4E7A-A95A-F54E48ECC8BB"))
                                            //.ExecuteBy("'C:/Users/Mickaelthumerel/AppData/Local/Programs/Python/Python312/python.exe'", null, ':')
                                            .Python()
                                            .Directory("Resources")
                                            .ExecuteFile("HtmlExtractor.py")

                                            .ExecEnvironment(e =>
                                            {
                                                e.Docker()
                                                 .Image("python")
                                                 .InstallDemocritePython()
                                                 .AddExtraBuildInstruction("RUN pip install lxml");
                                            })
#if DEBUG
                                            .Verbose(ArtifactExecVerboseEnum.Full)
#else
                                            .Verbose(ArtifactExecVerboseEnum.Minimal)
#endif
                                            .Persistent()
                                            .CompileAsync().GetAwaiter().GetResult();

            var rssItemIndexationSequence = Sequence.Build("rss-item-indexation", fixUid: new Guid("469E8121-CCFA-49DB-9582-1AF66C5C78AB"))

                                                    // Define that incomming input MUST be of type UrlRssItem
                                                    .RequiredInput<UrlRssItem>()
                                                    
                                                    // VGrain from bag "debug", this vgrain will simply display on the log the input and/or the execution context informations 
                                                    .Use<IDisplayInfoVGrain>().Call((g, i, ctx) => g.DisplayCallInfoAsync(i, ctx)).Return

                                                    // Store in execution context the full UrlRssItem that contains the rss item id information
                                                    .PushToContext(d => d)

                                                    // Use external artifact to extract HTML article content
                                                    .Use<IGenericArtifactExecutableVGrain>().Configure(extractorArtifact.Uid)
                                                                                            .Call((g, i, ctx) => g.RunAsync<HtmlPageContent, string>(i.Link, ctx)).Return

                                                    // Store in IRssItemVGrain the article content
                                                    .Use<IRssItemVGrain>().ConfigureFromContext<UrlRssItem, string>(r => r.Guid)
                                                                          .Call((g, i, ctx) => g.StoreArticleContentAsync(i.Content!, ctx)).Return
                                                    
                                                    .Build();

            var triggerFromStreamQueue = Trigger.Stream(rssItemIndexationStream, "from-queue-to-indexation", fixUid: new Guid("DB443A95-6185-4F70-8682-302E56CAB15A"))
#if DEBUG
                                                // Allow only 1 message to be process (3 if debugger is not attached) at the same time
                                                .MaxConcurrentProcess(Debugger.IsAttached ? (uint)1 : (uint)3)
#else
                                                // Allow maximum 3* messages to be process at same time
                                                //  * This value will increase by the cluster size. Example 4 nodes in the cluster then concurrent message could be 3 * 4 = 12 at same time
                                                .MaxConcurrentFactorClusterRelativeProcess(3)
#endif
                                                .AddTargetSequence(rssItemIndexationSequence)
                                                .Build();



#if DEBUG
            // Automatically display in the console when a signal flagged is fire
            builder.ShowSignals(rssItemUpdated);
#endif

            builder.AddInMemoryDefinitionProvider(p =>
            {
                p.SetupSignals(rssItemUpdated);
                p.SetupTriggers(autoUpdateTrigger);
                p.SetupSequences(importSeq, loadRssFeedSeq, refreshAllFeedsSeq);

                p.SetupArtifacts(extractorArtifact);
                p.SetupStreamQueues(rssItemIndexationStream);
                p.SetupSequences(rssItemIndexationSequence);
                p.SetupTriggers(triggerToInjectInStreamQueue, triggerFromStreamQueue);
            });

            return builder;
        }
    }
}
