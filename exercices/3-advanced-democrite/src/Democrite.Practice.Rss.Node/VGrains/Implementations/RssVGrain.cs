// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.VGrains.Implementations
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Practice.Rss.Node.Models;
    using Democrite.Practice.Rss.Node.States;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using MongoDB.Driver.Linq;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using System.Web;
    using System.Xml.Linq;

    internal sealed class RssVGrain : VGrainBase<RssState, RssStateSurrogate, RssStateConverter, IRssVGrain>, IRssVGrain
    {
        #region Fields

        private readonly IHttpClientFactory _clientFactory;
        private readonly ITimeManager _timeManager;
        private readonly IHashService _hashService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RssVGrain"/> class.
        /// </summary>
        /// <param name="hashService">This is the common service to produce Hash base on content (by default SHA256 + Unicity values).</param>
        /// <param name="timeManager">This is the service to privide time information.</param>
        /// <param name="logger">This is the logger service to let log about working</param>
        /// <param name="httpClientFactory">This is the factory service used to produce a safe reusable <see cref="IHttpClient"/> to download the rss xml feed with a web URL</param>
        /// <param name="persistentState">This service ensure the storage of this VGrain state. <see cref="PersistentStateAttribute"> provide some information to correctly resolve the correct IPersistentState</param>
        public RssVGrain([PersistentState("Rss")] IPersistentState<RssStateSurrogate> persistentState,
                         ILogger<IRssVGrain> logger,
                         IHttpClientFactory httpClientFactory,
                         ITimeManager timeManager,
                         IHashService hashService)
            : base(logger, persistentState)
        {
            this._timeManager = timeManager;
            this._hashService = hashService;
            this._clientFactory = httpClientFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<RssItem>> LoadAsync(Uri source, IExecutionContext<string> executionContext)
        {
            ArgumentNullException.ThrowIfNull(source);

            // TODO : assert if source is not http

            if (string.IsNullOrEmpty(this.State!.SourceUrl))
            {
                // This scope ensure at the first call that source URL is setup

                // Cloning using surrogate to let the storage responsability to RssState 
                var copy = this.State.ToSurrogate();
                var newState = new RssState(source.ToString(), copy.Items, this._timeManager.UtcNow);

                await base.PushStateAsync(newState, executionContext.CancellationToken);
            }

            // Get HttpClient from factory to ensure correct re-usability and connexion sharing
            var client = this._clientFactory.CreateClient();

            var response = await client.GetAsync(source.ToString(), executionContext.CancellationToken);

            // This method correctly raise dedicated exception is server return http code different than 200
            response.EnsureSuccessStatusCode();

            // We assume the target will only return a xml string content
            var contentStr = await response.Content.ReadAsStringAsync(executionContext.CancellationToken);

            var items = await ParseRssDataAsync(contentStr, executionContext.CancellationToken);

            executionContext.CancellationToken.ThrowIfCancellationRequested();

            this.State.PushItems(items.Select(i => new RssItemMetaData(i.Uid, i.Link, this._timeManager.UtcNow)));
            await PushStateAsync(executionContext.CancellationToken);

            return items;
        }

        #region Tools

        /// <summary>
        /// Parses the RSS data.
        /// </summary>
        private async ValueTask<IReadOnlyCollection<RssItem>> ParseRssDataAsync(string contentStr, CancellationToken token)
        {
            var grainId = this.GetGrainId().Key.ToString();

            ArgumentNullException.ThrowIfNullOrEmpty(contentStr);

            var root = XDocument.Parse(contentStr);

            var rssRoot = root.FirstNode as XElement;
            if (rssRoot == null || string.Equals(rssRoot.Name.LocalName, "rss", StringComparison.OrdinalIgnoreCase) == false)
                throw new InvalidDataException("Expect RSS XML format");

            var channels = rssRoot.Elements("channel");

            var results = new List<RssItem>();

            foreach (var item in channels.Elements("item"))
            {
                token.ThrowIfCancellationRequested();

                var title = item.Elements("title").FirstOrDefault();
                var link = item.Elements("link").FirstOrDefault();
                var guid = item.Elements("guid").FirstOrDefault();

                if (guid is null || title is null || link is null)
                {
                    this.Logger.OptiLog(LogLevel.Warning, "Rss item with missing mandatory information Guid, Link or Title : {item}", item);
                    continue;
                }

                string? description = null;
                var descriptionElem = item.Elements("description").FirstOrDefault();
                if (descriptionElem is not null && string.IsNullOrEmpty(descriptionElem.Value) == false)
                    description = HttpUtility.HtmlDecode(descriptionElem.Value);

                string? content = null;
                var encodedContent = item.Elements(XName.Get("encoded", "content")).FirstOrDefault();
                if (encodedContent is not null && string.IsNullOrEmpty(encodedContent.Value) == false)
                    content = HttpUtility.HtmlDecode(encodedContent.Value);

                var pudDate = this._timeManager.UtcNow;
                var pubdateElem = item.Elements("pubdate").FirstOrDefault();
                if (pubdateElem is not null && string.IsNullOrEmpty(pubdateElem.Value) == false && DateTime.TryParse(pubdateElem.Value, out var parsedPubDate))
                    pudDate = parsedPubDate;

                var categories = item.Elements("category").ToArray();
                var creators = item.Elements(XName.Get("creator", "dc")).ToArray();

                var keywords = item.Elements(XName.Get("keywords", "media")).SelectMany(k => k.Value?.Split(",", StringSplitOptions.TrimEntries) ?? EnumerableHelper<string>.ReadOnlyArray)
                                                              .ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;

                var subject = item.Elements(XName.Get("subject", "dc")).SelectMany(k => k.Value?.Split(",", StringSplitOptions.TrimEntries) ?? EnumerableHelper<string>.ReadOnlyArray)
                                                                       .ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;

                // Concat grainId + guid.Value to have a unique key combining source information and item itself
                // Prevent conflict between items with same id but with different source
                var guidHash = await this._hashService.GetHash(grainId + "##" + guid.Value);
                var rssItem = new RssItem(guidHash,
                                          link.Value,
                                          title.Value,
                                          description ?? "",
                                          content,
                                          grainId!,
                                          creators?.Select(c => c.Value)
                                                   .Where(c => string.IsNullOrEmpty(c) == false)
                                                   .ToArray() ?? EnumerableHelper<string>.ReadOnlyArray,
                                          pudDate,
                                          keywords.Concat(subject).Distinct().ToArray(),
                                          categories?.Select(c => c.Value)
                                                     .Where(c => string.IsNullOrEmpty(c) == false)
                                                     .ToArray() ?? EnumerableHelper<string>.ReadOnlyArray);

                results.Add(rssItem);
            }

            return results;
        }

        #endregion
        #endregion
    }
}
