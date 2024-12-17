// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// keep : Democrite.Framework.Configurations
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Builders;
    using Democrite.Practice.Rss.DataContract;
    using Democrite.Practice.Rss.DataContract.Models;
    using Democrite.Practice.Rss.Node.Models;
    using Democrite.Practice.Rss.Node.VGrains;

    using System;

    public static class DefintionConfigurationExtensions
    {
        /// <summary>
        /// Generate needed definition and store it in the node memory
        /// </summary>
        public static IDemocriteNodeWizard AddPracticeDefinitions(this IDemocriteNodeWizard builder)
        {
            var rssItemUpdated = Signal.Create("rss-item-updated", fixUid: PracticeConstants.RssItemUpdatedSignalId);

            var loadRssFeedSeq = Sequence.Build("load-rss-items", fixUid: new Guid("52150059-8000-4A19-8416-A3DED9D368AE"))

                                         // Define the input expected type
                                         .RequiredInput<RssFeedUrlSource>()

                                         // Use the vgrain IRssVGrain with the key matching "HashId" property on the input objet
                                         .Use<IRssVGrain>().ConfigureFromInput(i => i.HashId)

                                                           // Call in the VGrain IRssVGrain the method LoadAsync to load and parse the RSS XML target by the URL in property "SourceUri"
                                                           .Call((g, i, ctx) => g.LoadAsync(i.SourceUri, ctx)).Return

                                         // LoadAsync Methods will returned all the items parsed in objects RssItem
                                         .Foreach(IType<RssItem>.Default, f =>
                                         {
                                             // Foreach RssItem we use the IRssItemVGrain VGrain with the key matching RssItem UID (Hash combine of feed Uid and Item Guid (XML property)
                                             return f.Use<IRssItemVGrain>().ConfigureFromInput(i => i!.Uid)
                                                                           
                                                                           // Call in the VGrain IRssItemVGrain the method UpdateAsync to store or update the current RssItem Content
                                                                           .Call((g, i, ctx) => g.UpdateAsync(i!, ctx)).Return

                                                                           // Fire a signal with RssItem id information for future threatment
                                                                           .FireSignal(PracticeConstants.RssItemUpdatedSignalId).RelayMessage();
                                         })
                                         .Build();

            var importSeq = Sequence.Build("import-rss", fixUid: PracticeConstants.ImportRssSequence)

                                     // Define the input expected URI
                                    .RequiredInput<Uri>()

                                    // Use the vgrain IRssRegistryVGrain without specific key (singleton) and call the method RegisterAsync in it to register the URL and produce a normalize Hash Id from the URL
                                    .Use<IRssRegistryVGrain>().Call((g, i, ctx) => g.RegisterAsync(i, ctx)).Return

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

                                             // GetAllRegistredFeedAsync return a collection of RssFeedUrlSource containing the URL and its HashId associate
                                             .Foreach(IType<RssFeedUrlSource>.Default, f =>
                                             {
                                                 // For each feed URL call the sequence load-rss-items to reload the feed information
                                                 return f.CallSequence(loadRssFeedSeq.Uid).ReturnNoData;
                                             })
                                             .Build();

            // Define a trigger that will fire in internal of 2 minutes (Minutes % 2 == 0) and call the sequence refresh-all-inject-feeds
            var autoUpdateTrigger = Trigger.Cron("*/2 * * * *", "auto-update-loop", fixUid: new Guid("7A832833-FFDA-4E08-8E17-764F3E307DAF"))
                                           .AddTargetSequence(refreshAllFeedsSeq.Uid)
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
            });

            return builder;
        }
    }
}
