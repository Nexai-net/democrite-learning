// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// keep : Democrite.Framework.Configurations
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Builders;
    using Democrite.Practice.Rss.DataContract;
    using Democrite.Practice.Rss.Node.Models;
    using Democrite.Practice.Rss.Node.VGrains;

    using System;

    public static class DefintionConfigurationExtensions
    {
        public static IDemocriteNodeWizard AddPracticeDefinitions(this IDemocriteNodeWizard builder)
        {
            var importSeq = Sequence.Build("import-rss", fixUid: PracticeConstants.ImportRssSequence)
                                    .RequiredInput<Uri>()
                                    .Use<IToolVGrain>().Call((g, i, ctx) => g.CreateUrlSourceAsync(i, ctx)).Return
                                    .PushToContext(i => i)
                                    .Use<IRssVGrain>().ConfigureFromInput(i => i.HashId)
                                                      .Call((g, i, ctx) => g.LoadAsync(i.SourceUri, ctx)).Return

                                    .Foreach(IType<RssItem>.Default, f =>
                                    {
                                        return f.Use<IRssItemVGrain>().ConfigureFromInput(i => i!.HashId)
                                                                      .Call((g, i, ctx) => g.UpdateAsync(i!, ctx)).Return
                                                                      .FireSignal(PracticeConstants.RssItemUpdatedSignalId).RelayMessage();
                                    })
                                    .Build();

            builder.AddInMemoryDefinitionProvider(p =>
            {
                p.SetupSequences(importSeq);
            });

            return builder;
        }
    }
}
