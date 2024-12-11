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
        public static IDemocriteNodeWizard AddPracticeDefinitions(this IDemocriteNodeWizard builder)
        {
            var rssItemUpdated = Signal.Create("rss-item-updated", fixUid: PracticeConstants.RssItemUpdatedSignalId);

            var loadRssFeedSeq = Sequence.Build("load-rss-items", fixUid: new Guid("52150059-8000-4A19-8416-A3DED9D368AE"))
                                         .RequiredInput<RssFeedUrlSource>()
                                         .Use<IRssVGrain>().ConfigureFromInput(i => i.HashId)
                                                           .Call((g, i, ctx) => g.LoadAsync(i.SourceUri, ctx)).Return

                                         .Foreach(IType<RssItem>.Default, f =>
                                         {
                                             return f.Use<IRssItemVGrain>().ConfigureFromInput(i => i!.Uid)
                                                                           .Call((g, i, ctx) => g.UpdateAsync(i!, ctx)).Return
                                                                           .FireSignal(PracticeConstants.RssItemUpdatedSignalId).RelayMessage();
                                         })
                                         .Build();

            var importSeq = Sequence.Build("import-rss", fixUid: PracticeConstants.ImportRssSequence)
                                    .RequiredInput<Uri>()
                                    .Use<IRssRegistryVGrain>().Call((g, i, ctx) => g.RegisterAsync(i, ctx)).Return
                                    .CallSequence(loadRssFeedSeq.Uid).ReturnNoData
                                    .Build();

            var refreshAllFeedsSeq = Sequence.Build("refresh-all-inject-feeds", fixUid: new Guid("250EF9E4-6278-4115-97DE-C33D92DC223F"))
                                             .NoInput()
                                             .Use<IRssMonitorVGrain>().Call((g, ctx) => g.GetAllRegistredFeedAsync(ctx)).Return
                                             .Foreach(IType<RssFeedUrlSource>.Default, f =>
                                             {
                                                 return f.CallSequence(loadRssFeedSeq.Uid).ReturnNoData;
                                             })
                                             .Build();

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
