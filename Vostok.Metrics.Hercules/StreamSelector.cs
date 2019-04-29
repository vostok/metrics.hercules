namespace Vostok.Metrics.Hercules
{
    internal class StreamSelector
    {
        private readonly HerculesMetricSenderSettings settings;

        public StreamSelector(HerculesMetricSenderSettings settings)
        {
            this.settings = settings;
        }

        public string SelectStream(string aggregationType)
            => SelectStreamInternal(aggregationType) ?? settings.FallbackStream;

        private string SelectStreamInternal(string aggregationType)
        {
            switch (aggregationType)
            {
                case null:
                    return settings.FinalStream;

                case WellKnownAggregationTypes.Counter:
                    return settings.CountersStream;

                case WellKnownAggregationTypes.Timer:
                    return settings.TimersStream;

                case WellKnownAggregationTypes.Histogram:
                    return settings.HistogramsStream;

                default:
                    return null;
            }
        }
    }
}
