// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Practice.Rss.DataContract;
    using Democrite.Practice.Rss.DataContract.Models;

    /// <summary>
    /// Grain used to register all the rss feed stored
    /// </summary>
    [VGrainIdSingleton]
    internal interface IRssRegistryVGrain : IVGrain, IRssMonitorVGrain
    {
        /// <summary>
        /// Register if needing the rss feed
        /// </summary>
        Task<RssFeedUrlSource> RegisterAsync(Uri rssFeed, IExecutionContext ctx);
    }
}
