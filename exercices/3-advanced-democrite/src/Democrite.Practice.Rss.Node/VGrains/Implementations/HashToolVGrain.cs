// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains.Implementations
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Practice.Rss.DataContract.Models;

    using Elvex.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    internal sealed class HashToolVGrain : VGrainBase<IHashToolVGrain>, IHashToolVGrain
    {
        #region Fields
        
        private readonly IHashService _hashService;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initilized a new instance of the class <see cref="HashToolVGrain"/>
        /// </summary>
        public HashToolVGrain(ILogger<IHashToolVGrain> logger,
                              IHashService hashService) 
            : base(logger)
        {
            this._hashService = hashService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<UrlSource> HashAsync(Uri source, IExecutionContext context)
        {
            var hash = await this._hashService.GetHash(source.LocalPath);
            return new UrlSource(source, hash);
        }

        #endregion

    }
}
