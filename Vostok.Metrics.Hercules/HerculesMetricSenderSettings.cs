using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Metrics.Model;

namespace Vostok.Metrics.Hercules
{
    /// <summary>
    /// Represents configuration of <see cref="HerculesMetricSender"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesMetricSenderSettings
    {
        public HerculesMetricSenderSettings([NotNull] IHerculesSink sink, [NotNull] string defaultStream)
        {
            Sink = sink ?? throw new ArgumentNullException(nameof(sink));
            DefaultStream = defaultStream ?? throw new ArgumentNullException(nameof(defaultStream));
        }

        /// <summary>
        /// Hercules sink used to emit events.
        /// </summary>
        [NotNull]
        public IHerculesSink Sink { get; }

        /// <summary>
        /// Name of the Hercules stream to use by default.
        /// </summary>
        [NotNull]
        public string DefaultStream { get; }

        /// <summary>
        /// <para>Name of the Hercules stream to use for <see cref="MetricEvent"/>s with <c>null</c> aggregation type.</para>
        /// <para>If left <c>null</c>, <see cref="DefaultStream"/> will be used instead.</para>
        /// </summary>
        [CanBeNull]
        public string TerminalStream { get; set; }

        /// <summary>
        /// <para>Name of the Hercules stream to use for <see cref="MetricEvent"/>s with <see cref="WellKnownAggregationTypes.Counter"/> aggregation type.</para>
        /// <para>If left <c>null</c>, <see cref="DefaultStream"/> will be used instead.</para>
        /// </summary>
        [CanBeNull]
        public string CountersStream { get; set; }

        /// <summary>
        /// <para>Name of the Hercules stream to use for <see cref="MetricEvent"/>s with <see cref="WellKnownAggregationTypes.Timer"/> aggregation type.</para>
        /// <para>If left <c>null</c>, <see cref="DefaultStream"/> will be used instead.</para>
        /// </summary>
        [CanBeNull]
        public string TimersStream { get; set; }

        /// <summary>
        /// <para>Name of the Hercules stream to use for <see cref="MetricEvent"/>s with <see cref="WellKnownAggregationTypes.Histogram"/> aggregation type.</para>
        /// <para>If left <c>null</c>, <see cref="DefaultStream"/> will be used instead.</para>
        /// </summary>
        [CanBeNull]
        public string HistogramsStream { get; set; }
    }
}