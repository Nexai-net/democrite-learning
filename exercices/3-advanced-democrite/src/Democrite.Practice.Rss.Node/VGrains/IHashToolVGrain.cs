// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Practice.Rss.DataContract.Models;

    /// <summary>
    /// Define a reusage vgrain responsible to produce hash of requested data
    /// </summary>
    [VGrainStatelessWorker]
    public interface IHashToolVGrain : IVGrain
    {
        /// <summary>
        /// Produce a hash id from a <see cref="Uri"/>
        /// </summary>
        Task<UrlSource> HashAsync(Uri source, IExecutionContext context);
    }
}
