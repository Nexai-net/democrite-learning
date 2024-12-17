// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains.Implementations
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Practice.Rss.DataContract;
    using Democrite.Practice.Rss.DataContract.Models;
    using Democrite.Practice.Rss.Node.States;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class RssMonitorVGrain : VGrainBase<IRssMonitorVGrain>, IRssMonitorVGrain
    {
        #region Fields
        
        private readonly IReadOnlyRepository<RssStateSurrogate, string> _rssRepository;
        
        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        public RssMonitorVGrain(ILogger<IRssMonitorVGrain> logger,

                                // Ask a repository on the grain state storage configured by the key "LookupStorageConfig"
                                [Repository("Rss", "LookupStorageConfig")] IReadOnlyRepository<RssStateSurrogate, string> rssRepository) 
            : base(logger)
        {
            this._rssRepository = rssRepository;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<UrlSource>> GetAllRegistredFeedAsync(IExecutionContext ctx)
        {
            var allRssStates = await this._rssRepository.GetAllAsync(ctx.CancellationToken);
            return allRssStates.Select(s => new UrlSource(new Uri(s.SourceUrl), s.Uid)).ToArray();
        }
        
        #endregion
    }
}
