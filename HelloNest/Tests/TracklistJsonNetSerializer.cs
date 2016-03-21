using System;
using System.Collections.Generic;
using Nest;
using Newtonsoft.Json;

namespace HelloNest.Tests
{
    // cf.: https://github.com/elastic/elasticsearch-net/blob/master/docs/2.0-breaking-changes/nest-breaking-changes.md#serialization-settings
    internal class TracklistJsonNetSerializer : JsonNetSerializer
    {
        public TracklistJsonNetSerializer(IConnectionSettingsValues settings) : base (settings)
        {
        }

        protected override IList<Func<Type, JsonConverter>> ContractConverters =>
            new List<Func<Type, JsonConverter>>
            {
                type => type == typeof(TrackList) ? new JsonCreationConverter<TrackList>() : null
            };
    }
}