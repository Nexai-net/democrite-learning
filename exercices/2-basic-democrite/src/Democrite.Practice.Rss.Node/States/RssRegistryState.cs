// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.States
{
    using Democrite.Practice.Rss.DataContract.Models;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;

    internal sealed class RssRegistryState
    {
        #region Fields

        private readonly Dictionary<string, RssFeedUrlSource> _rssFeeds;
        private IImmutableList<RssFeedUrlSource> _copyList;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RssRegistryState"/> class.
        /// </summary>
        public RssRegistryState(IEnumerable<RssFeedUrlSource> rssFeeds)
        {
            this._rssFeeds = rssFeeds?.ToDictionary(k => k.HashId) ?? new Dictionary<string, RssFeedUrlSource>();
            this._copyList = this._rssFeeds.Values.ToImmutableList();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the RSS feeds.
        /// </summary>
        public IReadOnlyCollection<RssFeedUrlSource> RssFeeds
        {
            get { return this._copyList; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Pushes the specified source.
        /// </summary>
        public bool Push(in RssFeedUrlSource source)
        {
            if (this._rssFeeds.ContainsKey(source.HashId))
                return false;

            this._rssFeeds[source.HashId] = source;
            this._copyList = this._rssFeeds.Values.ToImmutableList();

            return true;
        }

        #endregion
    }

    /// <summary>
    /// Simple surrogate used to easily store (serialize) all the <see cref="RssRegistryState"/> information
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct RssRegistryStateSurrogate(IReadOnlyCollection<RssFeedUrlSource> Feeds);

    [RegisterConverter]
    internal sealed record class RssRegistryStateConverter : IConverter<RssRegistryState, RssRegistryStateSurrogate>
    {
        /// <inheritdoc />
        public RssRegistryState ConvertFromSurrogate(in RssRegistryStateSurrogate surrogate)
        {
            return new RssRegistryState(surrogate.Feeds);
        }

        /// <inheritdoc />
        public RssRegistryStateSurrogate ConvertToSurrogate(in RssRegistryState value)
        {
            return new RssRegistryStateSurrogate(value.RssFeeds);
        }
    }
}
