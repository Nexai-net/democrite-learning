// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains.Implementations
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Practice.Rss.Node.Models;

    using Elvex.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ToolVGrain : VGrainBase<IToolVGrain>, IToolVGrain
    {
        #region Fields
        
        private readonly IHashService _hashService;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolVGrain"/> class.
        /// </summary>
        public ToolVGrain(ILogger<IToolVGrain> logger, 
                          IHashService hashService) 
            : base(logger)
        {
            this._hashService = hashService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<UrlSource> CreateUrlSourceAsync(Uri source, IExecutionContext ctx)
        {
            var hash = await this._hashService.GetHash(source.ToString());
            return new UrlSource(source, hash);
        }

        #endregion
    }
}
