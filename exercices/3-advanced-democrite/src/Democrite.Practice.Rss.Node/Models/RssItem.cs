// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.Node.Models
{
    using System;
    using System.ComponentModel;

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class RssItem(string Uid,
                                       string Link,
                                       string Title,
                                       string Description,
                                       string? Content,
                                       string SourceId,
                                       IReadOnlyCollection<string> Creators,
                                       DateTime? PublishDate,
                                       IReadOnlyCollection<string> Keywords,
                                       IReadOnlyCollection<string> Categories) : IEquatable<RssItem>
    {
        public RssItem WithContent(string content)
        {
            return new RssItem(this.Uid,
                               this.Link,
                               this.Title,
                               this.Description,
                               content,
                               this.SourceId,
                               this.Creators,
                               this.PublishDate,
                               this.Keywords,
                               this.Categories);
        }

        /// <inheritdoc />
        public bool Equals(RssItem? item)
        {
            if (object.ReferenceEquals(this, item)) 
                return true;

            if (item is null)
                return false;

            return string.Equals(this.Uid, item.Uid) &&
                   string.Equals(this.Link, item.Link) &&
                   string.Equals(this.Title, item.Title) &&
                   string.Equals(this.Description, item.Description) &&
                   string.Equals(this.Content, item.Content) &&
                   string.Equals(this.SourceId, item.SourceId) &&
                   (this.PublishDate?.Equals(item.PublishDate) ?? item.PublishDate is null) &&
                   this.Creators.SequenceEqual(item.Creators) &&
                   this.Keywords.SequenceEqual(item.Keywords) &&
                   this.Categories.SequenceEqual(item.Categories);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Uid,
                                    this.Link,
                                    this.Title,
                                    this.Description,
                                    this.Content,
                                    this.SourceId,
                                    this.PublishDate,
                                    HashCode.Combine((this.Creators ?? EnumerableHelper<string>.ReadOnly).OrderBy(s => s).Aggregate(0, (acc, val) => acc ^ (val?.GetHashCode() ?? 0)),
                                                     (this.Keywords ?? EnumerableHelper<string>.ReadOnly).OrderBy(s => s).Aggregate(0, (acc, val) => acc ^ (val?.GetHashCode() ?? 0)),
                                                     (this.Categories ?? EnumerableHelper<string>.ReadOnly).OrderBy(s => s).Aggregate(0, (acc, val) => acc ^ (val?.GetHashCode() ?? 0))));
        }
    }

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class RssItemMetaData(string Uid, string Link, DateTime LastUpdate);

}
