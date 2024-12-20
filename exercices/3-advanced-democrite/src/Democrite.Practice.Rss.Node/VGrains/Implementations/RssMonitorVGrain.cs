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
        
        private readonly IReadOnlyRepository<RssItemStateSurrogate, string> _rssItemRepository;
        private readonly IReadOnlyRepository<RssStateSurrogate, string> _rssRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        public RssMonitorVGrain(ILogger<IRssMonitorVGrain> logger,

                                // Ask a repository on the grain state storage configured by the key "LookupStorageConfig"
                                [Repository("Rss", "LookupStorageConfig")] IReadOnlyRepository<RssStateSurrogate, string> rssRepository,
                                [Repository("Rss", "LookupStorageConfig")] IReadOnlyRepository<RssItemStateSurrogate, string> rssItemRepository) 
            : base(logger)
        {
            this._rssRepository = rssRepository;
            this._rssItemRepository = rssItemRepository;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<UrlSource>> GetAllRegistredFeedAsync(IExecutionContext ctx)
        {
            var allRssStates = await this._rssRepository.GetAllAsync(ctx.CancellationToken);
            return allRssStates.Select(s => new UrlSource(new Uri(s.SourceUrl), s.Uid)).ToArray();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<RssUrlSource>> SearchAsync(string pattern, IExecutionContext ctx)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(pattern);

            var words = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var  datas = await this._rssItemRepository.GetValuesAsync(r => words.Any(w => r.Current.Content.Contains(w) || 
                                                                                          r.Current.Creators.Contains(w) ||
                                                                                          r.Current.Title.Contains(w) ||
                                                                                          r.Current.Description.Contains(w) ||
                                                                                          r.Current.Categories.Contains(w)), ctx.CancellationToken);

            var scoredDatas = datas.Select(r =>
            {
                var score = words.Where(w => r.Current.Content.Contains(w) ||
                                             r.Current.Creators.Contains(w) ||
                                             r.Current.Title.Contains(w) ||
                                             r.Current.Description.Contains(w) ||
                                             r.Current.Categories.Contains(w))
                                 .Count();

                var scoreIteration = words.Sum(w => (r.Current.Content.Split(w).Length - 1) +
                                                    r.Current.Creators.Where(s => string.Equals(s, w, StringComparison.OrdinalIgnoreCase)).Count() +
                                                    (r.Current.Title.Split(w).Length - 1) +
                                                    (r.Current.Description.Split(w).Length - 1) +
                                                    r.Current.Categories.Where(s => string.Equals(s, w, StringComparison.OrdinalIgnoreCase)).Count());
                return (score: score + scoreIteration, data: r);
            })
            .OrderByDescending(s => s.score)
            //.Select(s => s.r)
            .Take(15)
            .ToArray();

            return scoredDatas.Select(s => s.data)
                              .Select(d => new RssUrlSource(new Uri(d.Current!.Link), d.Uid, d.Current.SourceId, d.Current.Title)).ToArray(); 
        }

        #endregion
    }
}
