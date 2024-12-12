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
                                       DateTime PublishDate,
                                       IReadOnlyCollection<string> Keywords,
                                       IReadOnlyCollection<string> Categories);

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class RssItemMetaData(string Uid, string Link, DateTime LastUpdate);

}
