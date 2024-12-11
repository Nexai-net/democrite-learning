// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Practice.Rss.Node.Models;

    using System.Threading.Tasks;

    /// <summary>
    /// VGrain dedicated to each rss item
    /// </summary>
    /// <remarks>
    ///     Use Rss link hash ad identifier
    /// </remarks>
    [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{executionContext.Configuration}")]
    public interface IRssItemVGrain : IVGrain
    {
        /// <summary>
        /// Updates the rss item content
        /// </summary>
        Task<UrlRssItem> UpdateAsync(RssItem item, IExecutionContext<string> executionContext);
    }
}
