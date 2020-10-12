using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Metrics.Models;

namespace Vostok.Metrics.Hercules
{
    /// <summary>
    /// Represents configuration of <see cref="HerculesMetricSender"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesMetricSenderSettings
    {
        public HerculesMetricSenderSettings([NotNull] IHerculesSink sink)
            => Sink = sink ?? throw new ArgumentNullException(nameof(sink));

        /// <summary>
        /// Hercules sink used to emit events.
        /// </summary>
        [NotNull]
        public IHerculesSink Sink { get; }

        /// <summary>
        /// Name of the Hercules stream to use as a fallback for when there's no configured stream for metric's aggregation type.
        /// </summary>
        [NotNull]
        public string FallbackStream { get; set; } = "metrics_any";

        /// <summary>
        /// <para>Name of the Hercules stream to use for <see cref="MetricEvent"/>s with <c>null</c> aggregation type.</para>
        /// <para>If set to <c>null</c>, <see cref="FallbackStream"/> will be used instead.</para>
        /// </summary>
        [CanBeNull]
        public string FinalStream { get; set; } = "metrics_final";

        /// <summary>
        /// <para>Name of the Hercules stream to use for <see cref="MetricEvent"/>s with <see cref="WellKnownAggregationTypes.Counter"/> aggregation type.</para>
        /// <para>If set to <c>null</c>, <see cref="FallbackStream"/> will be used instead.</para>
        /// </summary>
        [CanBeNull]
        public string CountersStream { get; set; } = "metrics_counters";

        /// <summary>
        /// <para>Name of the Hercules stream to use for <see cref="MetricEvent"/>s with <see cref="WellKnownAggregationTypes.Timer"/> aggregation type.</para>
        /// <para>If set to <c>null</c>, <see cref="FallbackStream"/> will be used instead.</para>
        /// </summary>
        [CanBeNull]
        public string TimersStream { get; set; } = "metrics_timers";

        /// <summary>
        /// <para>Name of the Hercules stream to use for <see cref="MetricEvent"/>s with <see cref="WellKnownAggregationTypes.Histogram"/> aggregation type.</para>
        /// <para>If set to <c>null</c>, <see cref="FallbackStream"/> will be used instead.</para>
        /// </summary>
        [CanBeNull]
        public string HistogramsStream { get; set; } = "metrics_histograms";

        /// <summary>
        /// Name of the Hercules stream to use for <see cref="AnnotationEvent"/>s.
        /// </summary>
        [NotNull]
        public string AnnotationsStream { get; set; } = "annotations";

        /// <summary>
        /// Whether or not to send metric events to Hercules.
        /// </summary>
        [CanBeNull]
        public Func<bool> Enabled { get; set; }
    }
}