// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains.Implementations
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Practice.Rss.DataContract;
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
        private readonly ISignalService _signalService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RssItemVGrain"/> class.
        /// </summary>
        /// <param name="timeManager">This is the service to privide time information.</param>
        /// <param name="logger">This is the logger service to let log about working</param>
        /// <param name="persistentState">This service ensure the storage of this VGrain state. <see cref="PersistentStateAttribute"> provide some information to correctly resolve the correct IPersistentState</param>
        /// <paramref name="signalService"/>This service allow us to fire a signal.
        public RssItemVGrain([PersistentState("Rss", "LookupStorageConfig")] IPersistentState<RssItemStateSurrogate> persistentState,
                             ILogger<IRssItemVGrain> logger,
                             ITimeManager timeManager,
                             ISignalService signalService)
            : base(logger, persistentState)
        {
            this._timeManager = timeManager;
            this._signalService = signalService;
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

            var itemResult = new UrlRssItem(item.Uid, item.Link);

            // The content have changed then signal is fired to trigger different processing
            // Raised also when the content is empty
            if (hasChanged || string.IsNullOrEmpty(this.State.Current?.Content))
                await this._signalService.Fire(PracticeConstants.RssItemUpdatedSignalId, itemResult, executionContext.CancellationToken, this);

            return itemResult;
        }

        /// <inheritdoc />
        public async Task StoreArticleContentAsync(string content, IExecutionContext<string> executionContext)
        {
            var added = this.State!.SetCurrentContent(content);

            if (added)
                await PushStateAsync(executionContext.CancellationToken);
        }

        #endregion
    }
}
