// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains.Implementations
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Practice.Rss.DataContract.Models;
    using Democrite.Practice.Rss.Node.Models;
    using Democrite.Practice.Rss.Node.States;

    using Elvex.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class RssRegistryVGrain : VGrainBase<RssRegistryState, RssRegistryStateSurrogate, RssRegistryStateConverter, IRssRegistryVGrain>, IRssRegistryVGrain
    {
        #region Fields

        private readonly IHashService _hashService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RssRegistryVGrain"/> class.
        /// </summary>
        public RssRegistryVGrain([PersistentState("Rss")] IPersistentState<RssRegistryStateSurrogate> persistentState,
                                 ILogger<IRssRegistryVGrain> logger,
                                 IHashService hashService)
            : base(logger, persistentState)
        {
            this._hashService = hashService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<IReadOnlyCollection<RssFeedUrlSource>> GetAllRegistredFeedAsync(IExecutionContext ctx)
        {
            return Task.FromResult(this.State!.RssFeeds);
        }

        /// <inheritdoc />
        public async Task<RssFeedUrlSource> RegisterAsync(Uri rssFeed, IExecutionContext ctx)
        {
            var hash = await this._hashService.GetHash(rssFeed.ToString());

            var source = new RssFeedUrlSource(rssFeed, hash);

            var haveChanged = this.State!.Push(source);

            if (haveChanged)
                await PushStateAsync(ctx.CancellationToken);

            return source;
        }

        #endregion
    }
}
