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
    public record struct UrlData(Uri SourceUrl, string Data);

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct UrlRssItem(string Guid, string Link);
}
