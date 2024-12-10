// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Practice.Rss.Node.Models;

    using System;
    using System.Threading.Tasks;

    [VGrainStatelessWorker]
    public interface IToolVGrain : IVGrain
    {
        /// <summary>
        /// Creates the URL source asynchronous.
        /// </summary>
        Task<UrlSource> CreateUrlSourceAsync(Uri source, IExecutionContext ctx);
    }
}
