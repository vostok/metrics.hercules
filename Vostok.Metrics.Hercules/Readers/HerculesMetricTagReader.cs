using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Models;

// ReSharper disable ParameterHidesMember

namespace Vostok.Metrics.Hercules.Readers
{
    internal class HerculesMetricTagReader : DummyHerculesTagsBuilder, IHerculesTagsBuilder
    {
        private string key;
        private string value;

        public MetricTag Build()
        {
            return new MetricTag(key, value);
        }

        public new IHerculesTagsBuilder AddValue(string key, string value)
        {
            switch (key)
            {
                case TagNames.Key:
                    this.key = value;
                    break;
                case TagNames.Value:
                    this.value = value;
                    break;
            }

            return this;
        }
    }
}