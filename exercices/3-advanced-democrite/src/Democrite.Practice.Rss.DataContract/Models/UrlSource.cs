// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Practice.Rss.DataContract.Models
{
    using System;
    using System.ComponentModel;

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct UrlSource(Uri SourceUri, string HashId);

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct RssUrlSource(Uri Link, string Uid, string SourceId, string Title);
}
