using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Model;

// ReSharper disable PossibleNullReferenceException

namespace Vostok.Metrics.Hercules
{
    /// <summary>
    /// Converts <see cref="HerculesEvent"/>s written by <see cref="HerculesMetricSender"/> back to <see cref="MetricEvent"/>s.
    /// </summary>
    [PublicAPI]
    public static class HerculesMetricEventFactory
    {
        [NotNull]
        public static MetricEvent CreateFrom([NotNull] HerculesEvent @event)
        {
            var timestamp = @event.Timestamp;
            var tags = ConvertTags(@event.Tags[TagNames.Tags].AsVector.AsContainerList);
            var value = @event.Tags[TagNames.Value].AsDouble;
            var unit = @event.Tags[TagNames.Unit]?.AsString;
            var aggregationType = @event.Tags[TagNames.AggregationType]?.AsString;
            var aggregationParameters = ConvertAggregationParameters(@event.Tags[TagNames.AggregationParameters]?.AsContainer);

            return new MetricEvent(value, tags, timestamp, unit, aggregationType, aggregationParameters);
        }

        private static MetricTags ConvertTags([NotNull] IReadOnlyList<HerculesTags> tags)
        {
            var result = MetricTags.Empty;

            foreach (var tag in tags)
                result = result.Append(tag[TagNames.Key].AsString, tag[TagNames.Value].AsString);

            return result;
        }

        private static IReadOnlyDictionary<string, string> ConvertAggregationParameters([CanBeNull] HerculesTags tags)
        {
            if (tags == null || tags.Count == 0)
                return null;

            var result = new Dictionary<string, string>(tags.Count);

            foreach (var tag in tags)
                result[tag.Key] = tag.Value.AsString;
            
            return result;
        }
    }
}