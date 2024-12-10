// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Practice.Rss.Node.Models;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Grain dedicated to a specific source
    /// </summary>
    /// <remarks>
    ///     The GrainId is a hash of the source url
    /// </remarks>
    [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{executionContext.Configuration}")]
    public interface IRssVGrain : IVGrain
    {
        /// <summary>
        /// Parses the RSS to produce <see cref="RssItem"/>
        /// </summary>
        Task<IReadOnlyCollection<RssItem>> LoadAsync(Uri source, IExecutionContext<string> executionContext);
    }
}
