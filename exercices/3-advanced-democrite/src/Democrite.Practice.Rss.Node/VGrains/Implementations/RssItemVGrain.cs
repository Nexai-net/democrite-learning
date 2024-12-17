﻿// Copyright (c) Nexai.
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

    using System.Threading.Tasks;

    internal sealed class RssItemVGrain : VGrainBase<RssItemState, RssItemStateSurrogate, RssItemStateConverter, IRssItemVGrain>, IRssItemVGrain
    {
        #region Fields
        
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RssItemVGrain"/> class.
        /// </summary>
        /// <param name="timeManager">This is the service to privide time information.</param>
        /// <param name="logger">This is the logger service to let log about working</param>
        /// <param name="persistentState">This service ensure the storage of this VGrain state. <see cref="PersistentStateAttribute"> provide some information to correctly resolve the correct IPersistentState</param>
        public RssItemVGrain([PersistentState("Rss")] IPersistentState<RssItemStateSurrogate> persistentState,
                             ILogger<IRssItemVGrain> logger,
                             ITimeManager timeManager) 
            : base(logger, persistentState)
        {
            this._timeManager = timeManager;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<UrlRssItem> UpdateAsync(RssItem item, IExecutionContext<string> executionContext)
        {
            var grainId = this.GetGrainId().Key.ToString();

            // Ensure that RssItem provide follow the logic to be stored in a vgrain with the same key as item id.

            if (grainId != item.Uid)
                throw new InvalidDataException("This rss item doesn't bellong to this VGrain");

            var hasChanged = this.State!.PushNewItemVersion(item, this._timeManager);
            if (hasChanged)
                await PushStateAsync(executionContext.CancellationToken);

            return new UrlRssItem(item.Uid, item.Link);
        }

        #endregion
    }
}