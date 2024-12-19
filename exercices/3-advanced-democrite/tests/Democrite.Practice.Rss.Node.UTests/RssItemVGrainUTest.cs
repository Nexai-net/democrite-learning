// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.UTests
{
    using AutoFixture;
    using AutoFixture.Kernel;

    using Democrite.Framework.Core.Abstractions;
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
    using NSubstitute.ReceivedExtensions;

    using Orleans.Runtime;

    public class RssItemVGrainUTest
    {
        #region Fields

        private static readonly ISignalService s_defaultSignalServiceMock;
        private static readonly ITimeManager s_defaultTimeManagerService;
        private static readonly Fixture s_defaultTestAutoFixture;

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize the class <see cref="RssItemVGrainUTest"/>
        /// </summary>
        static RssItemVGrainUTest()
        {
            s_defaultSignalServiceMock = Substitute.For<ISignalService>();
            s_defaultTimeManagerService = new TimeManager();

            // Setup a referent "Fixture" object with default needing services
            // This referent fixture will be used has fallback strategy if local one doesn't have valid solver
            s_defaultTestAutoFixture = new Fixture();
            s_defaultTestAutoFixture.Register<ILogger<IRssItemVGrain>>(() => InMemoryLogger.Create<IRssItemVGrain>());
            s_defaultTestAutoFixture.Register<ITimeManager>(() => s_defaultTimeManagerService);
            s_defaultTestAutoFixture.Register<ISignalService>(() => s_defaultSignalServiceMock);

            var persistantState = Substitute.For<IPersistentState<RssItemStateSurrogate>>();
            s_defaultTestAutoFixture.Register<IPersistentState<RssItemStateSurrogate>>(() => persistantState);

        }

        #endregion

        /// <summary>
        /// Check that update method correctly react to inserting a <see cref="RssItem"/> with a different Uid
        /// </summary>
        [Fact]
        public async Task RssItem_Update_Validity_Check()
        {
            var localFixture = new Fixture(new DefaultEngineParts(s_defaultTestAutoFixture.AsEnumerable()));

            
            var validGrainId = new GrainId(GrainType.Create("RssItemVGrain"), IdSpan.Create("valid"));

            var vgrain = await localFixture.CreateAndInitVGrain<RssItemVGrain>(forcedGrainId: validGrainId)!;

            var validRssItem = new RssItem("valid", null, null, null, null, null, null, DateTime.UtcNow, null, null);

            var ctxMock = Substitute.For<IExecutionContext<string>>();
            ctxMock.Configuration.Returns("valid");

            var updated = await vgrain.UpdateAsync(validRssItem, ctxMock);

            Check.That(updated.Guid).IsNotNull().And.IsEqualTo("valid");

            var invvalidRssItem = new RssItem("invalid", null, null, null, null, null, null, DateTime.UtcNow, null, null);

            Check.ThatCode(() => vgrain.UpdateAsync(invvalidRssItem, ctxMock)).Throws<InvalidDataException>()
                                                                              .WithMessage("This rss item doesn't bellong to this VGrain");
        }

        /// <summary>
        /// Check that after an update that induce a change the state is well saved and signal is weel raised
        /// </summary>
        [Fact]
        public async Task RssItem_Update_Save_And_Signal()
        {
            var localFixture = new Fixture(new DefaultEngineParts(s_defaultTestAutoFixture.AsEnumerable()));

            var persistantState = Substitute.For<IPersistentState<RssItemStateSurrogate>>();
            localFixture.Register<IPersistentState<RssItemStateSurrogate>>(() => persistantState);

            var localSignalServiceMock = Substitute.For<ISignalService>();
            localFixture.Register<ISignalService>(() => localSignalServiceMock);

            var validRssItem = localFixture.Create<RssItem>();

            var validGrainId = new GrainId(GrainType.Create("RssItemVGrain"), IdSpan.Create(validRssItem.Uid));
            var vgrain = await localFixture.CreateAndInitVGrain<RssItemVGrain>(forcedGrainId: validGrainId)!;

            var ctxMock = Substitute.For<IExecutionContext<string>>();
            ctxMock.Configuration.Returns(validRssItem.Uid);

            // Check that no call have been made to save the state
            await persistantState.Received(0).WriteStateAsync();
            await localSignalServiceMock.Received(0).Fire<UrlRssItem>(Arg.Any<SignalId>(), Arg.Any<UrlRssItem>(), Arg.Any<CancellationToken>(), vgrain);

            var updated = await vgrain.UpdateAsync(validRssItem, ctxMock);

            Check.That(updated.Guid).IsNotNull().And.IsEqualTo(validRssItem.Uid);

            // Check that 1 call have been made to save the state
            await persistantState.Received(1).WriteStateAsync();
            await localSignalServiceMock.Received(1).Fire<UrlRssItem>(Arg.Any<SignalId>(), Arg.Any<UrlRssItem>(), Arg.Any<CancellationToken>(), vgrain);

            // Flush records
            persistantState.ClearReceivedCalls();
            localSignalServiceMock.ClearReceivedCalls();

            // Try to add identically item
            var identicalValidRssItem = new RssItem(validRssItem.Uid,
                                                    validRssItem.Link,
                                                    validRssItem.Title,
                                                    validRssItem.Description,
                                                    validRssItem.Content,
                                                    validRssItem.SourceId,
                                                    validRssItem.Creators.ToArray(),
                                                    validRssItem.PublishDate,
                                                    validRssItem.Keywords.ToArray(),
                                                    validRssItem.Categories.ToArray());

            // Ensure equality
            Check.That(identicalValidRssItem).IsEqualTo(validRssItem);

            updated = await vgrain.UpdateAsync(identicalValidRssItem, ctxMock);
            Check.That(updated.Guid).IsNotNull().And.IsEqualTo(validRssItem.Uid);

            // Check that no call have been made to save the state, saving must only occured when change occured
            await persistantState.Received(0).WriteStateAsync();
            await localSignalServiceMock.Received(0).Fire<UrlRssItem>(Arg.Any<SignalId>(), Arg.Any<UrlRssItem>(), Arg.Any<CancellationToken>(), vgrain);


        }
    }
}