// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.UTests
{
    using AutoFixture;

    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Helpers;
    using Democrite.Framework.Core.Models;
    using Democrite.Practice.Rss.Node.Models;
    using Democrite.Practice.Rss.Node.States;
    using Democrite.Practice.Rss.Node.VGrains;
    using Democrite.Practice.Rss.Node.VGrains.Implementations;
    using Democrite.UnitTests.ToolKit.Extensions;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Loggers;
    using Elvex.Toolbox.Services;

    using Microsoft.Extensions.Logging;

    using NFluent;

    using NSubstitute;

    using Orleans.Runtime;

    public class RssItemVGrainUTest
    {
        [Fact]
        public async Task RssItem_Update()
        {
            var autoFixture = new Fixture();
            autoFixture.Register<ILogger<IRssItemVGrain>>(() => InMemoryLogger.Create<IRssItemVGrain>());

            var persistantState = Substitute.For<IPersistentState<RssItemStateSurrogate>>();
            var signalService = Substitute.For<ISignalService>();

            autoFixture.Register<IPersistentState<RssItemStateSurrogate>>(() => persistantState);
            autoFixture.Register<ITimeManager>(() => new TimeManager());
            autoFixture.Register<ISignalService>(() => signalService);

            var validGrainId = new GrainId(GrainType.Create("RssItemVGrain"), IdSpan.Create("valid"));

            var vgrain = autoFixture.CreateVGrain<RssItemVGrain>(forcedGrainId: validGrainId)!;
            await autoFixture.InitVGrain(vgrain);

            var validRssItem = new RssItem("valid", null, null, null, null, null, null, DateTime.UtcNow, null, null);

            var ctx = ExecutionContextHelper.CreateNew("valid");

            var updated = await vgrain.UpdateAsync(validRssItem, ctx);

            Check.That(updated.Guid).IsNotNull().And.IsEqualTo("valid");
        }
    }
}