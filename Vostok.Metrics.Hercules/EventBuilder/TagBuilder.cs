using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Models;

// ReSharper disable ParameterHidesMember

namespace Vostok.Metrics.Hercules.EventBuilder
{
    internal class TagBuilder : DummyHerculesTagsBuilder, IHerculesTagsBuilder
    {
        private string key;
        private string value;

        public MetricTag Build()
        {
            return new MetricTag(key, value);
        }

        IHerculesTagsBuilder IHerculesTagsBuilder.AddValue(string key, string value)
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