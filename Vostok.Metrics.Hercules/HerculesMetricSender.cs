using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions;
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
            => settings.Sink.Put(
                streamSelector.SelectStream(@event.AggregationType),
                builder => HerculesEventMetricBuilder.Build(@event, builder));
    }
}