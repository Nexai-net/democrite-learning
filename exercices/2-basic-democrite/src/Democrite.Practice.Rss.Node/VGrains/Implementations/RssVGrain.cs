// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains.Implementations
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Practice.Rss.Node.Models;
    using Democrite.Practice.Rss.Node.States;

    using Elvex.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class RssVGrain : VGrainBase<RssState, RssStateSurrogate, RssStateConverter, IRssVGrain>, IRssVGrain
    {
        #region Fields

        private readonly IHttpClientFactory _clientFactory;
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RssVGrain"/> class.
        /// </summary>
        public RssVGrain([PersistentState("Rss")] IPersistentState<RssStateSurrogate> persistentState,
                         ILogger<IRssVGrain> logger,
                         IHttpClientFactory httpClientFactory,
                         ITimeManager timeManager)
            : base(logger, persistentState)
        {
            this._timeManager = timeManager;
            this._clientFactory = httpClientFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<RssItem>> LoadAsync(Uri source, IExecutionContext<string> executionContext)
        {
            ArgumentNullException.ThrowIfNull(source);

            // TODO : assert if source is not http

            if (string.IsNullOrEmpty(this.State!.SourceUrl))
            {
                var copy = this.State.ToSurrogate();
                var newState = new RssState(source.ToString(), copy.Items, this._timeManager.UtcNow);
                await base.PushStateAsync(newState, executionContext.CancellationToken);
            }

            var client = this._clientFactory.CreateClient();

            var response = await client.GetAsync(source.ToString());

            response.EnsureSuccessStatusCode();

            var contentStr = await response.Content.ReadAsStringAsync(executionContext.CancellationToken);

            var items = ParseRssData(contentStr);

            this.State.PushItems(items.Select(i => new RssItemMetaData(i.HashId, i.Link, this._timeManager.UtcNow)));
            await PushStateAsync(executionContext.CancellationToken);

            return items;
        }

        #region Tools

        /// <summary>
        /// Parses the RSS data.
        /// </summary>
        private IReadOnlyCollection<RssItem> ParseRssData(string contentStr)
        {
            throw new NotImplementedException();
        }

        #endregion
        #endregion
    }
}
