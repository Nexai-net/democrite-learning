// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.States
{
    using Democrite.Practice.Rss.Node.Models;

    using Elvex.Toolbox.Abstractions.Services;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class RssItemState
    {
        #region Fields

        private readonly List<RssItem> _history;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RssItemState"/> class.
        /// </summary>
        public RssItemState(IEnumerable<RssItem>? history,
                            RssItem? current,
                            DateTime? lastUpdate)
        {
            this._history = history?.ToList() ?? new List<RssItem>();
            this.Current = current;
            this.LastUpdateTime = lastUpdate ?? DateTime.UtcNow;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current.
        /// </summary>
        public RssItem? Current { get; private set; }

        /// <summary>
        /// Gets the last update time.
        /// </summary>
        public DateTime LastUpdateTime { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        public RssItemStateSurrogate ToSurrogate()
        {
            return new RssItemStateSurrogate(this.Current, this._history, this.LastUpdateTime);
        }

        /// <summary>
        /// Pushes the new item version.
        /// </summary>
        internal bool PushNewItemVersion(RssItem item, ITimeManager timeManager)
        {
            if (this.Current?.Equals(item) == true)
                return false;

            this.Current = item;
            this.LastUpdateTime = timeManager.UtcNow;

            if (this.Current is not null)
                this._history.Add(this.Current);

            return true;
        }

        /// <inheritdoc />
        internal bool SetCurrentContent(string content)
        {
            if (this.Current is null)
                return false;

            if (string.Equals(this.Current.Content, content))
                return false;

            this.Current = this.Current.WithContent(content);
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Simple surrogate used to easily store (serialize) all the <see cref="RssItemState"/> information
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct RssItemStateSurrogate(RssItem? Current, IReadOnlyCollection<RssItem> History, DateTime? LastUpdate);

    [RegisterConverter]
    internal sealed class RssItemStateConverter : IConverter<RssItemState, RssItemStateSurrogate>
    {
        /// <inheritdoc />
        public RssItemState ConvertFromSurrogate(in RssItemStateSurrogate surrogate)
        {
            return new RssItemState(surrogate.History,
                                    surrogate.Current,
                                    surrogate.LastUpdate);
        }

        /// <inheritdoc />
        public RssItemStateSurrogate ConvertToSurrogate(in RssItemState value)
        {
            return value.ToSurrogate();
        }
    }
}
