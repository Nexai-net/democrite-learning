// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.States
{
    using Democrite.Practice.Rss.Node.Models;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    internal sealed class RssState
    {
        #region Fields

        private readonly Dictionary<string, RssItemMetaData> _items;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RssState"/> class.
        /// </summary>
        public RssState(string sourceUrl, IEnumerable<RssItemMetaData> items, DateTime lastUpdate)
        {
            this.SourceUrl = sourceUrl;
            this._items = items?.GroupBy(i => i.Uid)
                                .ToDictionary(k => k.Key, v => v.OrderByDescending(d => d.LastUpdate).First()) ?? new Dictionary<string, RssItemMetaData>();

            this.LastUpdate = lastUpdate;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the last update.
        /// </summary>
        public DateTime LastUpdate { get; }

        /// <summary>
        /// Gets the source URL.
        /// </summary>
        public string SourceUrl { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        public RssStateSurrogate ToSurrogate()
        {
            return new RssStateSurrogate(this.SourceUrl, this._items.Values.ToArray(), this.LastUpdate);
        }

        #region Tools

        /// <summary>
        /// Pushes the items.
        /// </summary>
        internal void PushItems(IEnumerable<RssItemMetaData> items)
        {
            foreach (var item in items)
                this._items[item.Uid] = item;
        }

        #endregion
        #endregion
    }

    /// <summary>
    /// Simple surrogate used to easily store (serialize) all the <see cref="RssState"/> information
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct RssStateSurrogate(string SourceUrl, 
                                           IReadOnlyCollection<RssItemMetaData> Items, 
                                           DateTime LastUpdate);

    [RegisterConverter]
    internal sealed class RssStateConverter : IConverter<RssState, RssStateSurrogate>
    {
        /// <inheritdoc />
        public RssState ConvertFromSurrogate(in RssStateSurrogate surrogate)
        {
            return new RssState(surrogate.SourceUrl, surrogate.Items, surrogate.LastUpdate);
        }

        /// <inheritdoc />
        public RssStateSurrogate ConvertToSurrogate(in RssState value)
        {
            return value.ToSurrogate();
        }
    }
}
