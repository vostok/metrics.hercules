using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Models;

namespace Vostok.Metrics.Hercules
{
    /// <summary>
    /// Converts <see cref="MetricEvent"/>s to <see cref="HerculesEvent"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesEventMetricBuilder
    {
        public static HerculesEvent Build([NotNull] MetricEvent @event)
        {
            var builder = new HerculesEventBuilder();
            Build(@event, builder);
            return builder.BuildEvent();
        }

        public static void Build([NotNull] MetricEvent @event, [NotNull] IHerculesEventBuilder builder)
        {
            builder.SetTimestamp(@event.Timestamp);
            builder.AddValue(TagNames.Value, @event.Value);
            builder.AddValue(TagNames.TagsHash, @event.Tags.GetHashCode());
            builder.AddVectorOfContainers(
                TagNames.Tags,
                @event.Tags,
                (b, tag) =>
                {
                    b.AddValue(TagNames.Key, tag.Key);
                    b.AddValue(TagNames.Value, tag.Value);
                });

            if (!string.IsNullOrEmpty(@event.Unit))
                builder.AddValue(TagNames.Unit, @event.Unit);

            if (!string.IsNullOrEmpty(@event.AggregationType))
                builder.AddValue(TagNames.AggregationType, @event.AggregationType);

            if (@event.AggregationParameters != null)
                builder.AddContainer(
                    TagNames.AggregationParameters,
                    b =>
                    {
                        foreach (var pair in @event.AggregationParameters)
                            b.AddValue(pair.Key, pair.Value);
                    });
        }
    }
}