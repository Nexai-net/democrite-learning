// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.DataContract
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Practice.Rss.DataContract.Models;

    using Orleans.Concurrency;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    [VGrainIdSingleton]
    public interface IRssMonitorVGrain : IVGrain
    {
        /// <summary>
        /// Gets all registred feed asynchronous.
        /// </summary>
        [ReadOnly]
        Task<IReadOnlyCollection<RssFeedUrlSource>> GetAllRegistredFeedAsync(IExecutionContext ctx);
    }
}
