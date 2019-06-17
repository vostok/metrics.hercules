using System.Collections.Generic;
using Vostok.Commons.Binary;
using Vostok.Commons.Collections;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Models;

namespace Vostok.Metrics.Hercules.EventBuilder
{
    internal static class MetricEventBuilderCache
    {
        public static readonly CachingTransform<IBinaryBuffer, Dictionary<ByteArrayKey, MetricTags>> TagsCache 
            = new CachingTransform<IBinaryBuffer, Dictionary<ByteArrayKey, MetricTags>>(
                _ => new Dictionary<ByteArrayKey, MetricTags>());

        public static readonly CachingTransform<IBinaryBuffer, Dictionary<ByteArrayKey, Dictionary<string, string>>> AggregationParametersCache
            = new CachingTransform<IBinaryBuffer, Dictionary<ByteArrayKey, Dictionary<string, string>>>(
                _ => new Dictionary<ByteArrayKey, Dictionary<string, string>>());
    }
}