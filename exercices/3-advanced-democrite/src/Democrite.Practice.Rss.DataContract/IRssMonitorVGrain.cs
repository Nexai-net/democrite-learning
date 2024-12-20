// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.DataContract
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Practice.Rss.DataContract.Models;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    //[VGrainIdSingleton]

    // A singleton was creating a bottle neck where all the request was going
    // A state repository will all us to explorer the data base without a single registry point
    // We pass to stateless worker to let democrite and orleans spawn as many instance as needed
    [VGrainStatelessWorker]
    public interface IRssMonitorVGrain : IVGrain
    {
        /// <summary>
        /// Gets all registred feed asynchronous.
        /// </summary>
        [Orleans.Concurrency.ReadOnly]
        Task<IReadOnlyCollection<UrlSource>> GetAllRegistredFeedAsync(IExecutionContext ctx);

        /// <summary>
        /// 
        /// </summary>
        [Orleans.Concurrency.ReadOnly]
        Task<IReadOnlyCollection<RssUrlSource>> SearchAsync(string Pattern, IExecutionContext ctx);
    }
}
