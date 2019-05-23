using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Models;

namespace Vostok.Metrics.Hercules
{
    /// <summary>
    /// An implementation of <see cref="IMetricEventSender"/> that saves incoming events as Hercules events using an instance of <see cref="IHerculesSink"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesMetricSender : IMetricEventSender
    {
        private readonly HerculesMetricSenderSettings settings;
        private readonly StreamSelector streamSelector;

        public HerculesMetricSender([NotNull] HerculesMetricSenderSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            streamSelector = new StreamSelector(settings);
        }

        public void Send(MetricEvent @event)
            => settings.Sink.Put(streamSelector.SelectStream(@event.AggregationType), builder => BuildEvent(builder, @event));

        private static void BuildEvent(IHerculesEventBuilder builder, MetricEvent @event)
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
