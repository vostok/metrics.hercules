using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Commons.Binary;
using Vostok.Commons.Collections;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Models;

// ReSharper disable ParameterHidesMember

namespace Vostok.Metrics.Hercules.Readers
{
    [PublicAPI]
    public class HerculesMetricEventReader : DummyHerculesTagsBuilder, IHerculesEventBuilder<MetricEvent>
    {
        private static readonly DummyHerculesTagsBuilder DummyBuilder = new DummyHerculesTagsBuilder();

        private static readonly CachingTransform<IBinaryBufferReader, Dictionary<ByteArrayKey, MetricTags>> TagsCache
            = new CachingTransform<IBinaryBufferReader, Dictionary<ByteArrayKey, MetricTags>>(
                _ => new Dictionary<ByteArrayKey, MetricTags>());

        private static readonly CachingTransform<IBinaryBufferReader, Dictionary<ByteArrayKey, Dictionary<string, string>>> AggregationParametersCache
            = new CachingTransform<IBinaryBufferReader, Dictionary<ByteArrayKey, Dictionary<string, string>>>(
                _ => new Dictionary<ByteArrayKey, Dictionary<string, string>>());

        private readonly IBinaryBufferReader reader;
        private readonly Dictionary<ByteArrayKey, MetricTags> tagsCache;
        private readonly Dictionary<ByteArrayKey, Dictionary<string, string>> aggregationParametersCache;

        private double? value;
        private MetricTags tags;
        private DateTimeOffset timestamp;
        private string unit;
        private string aggregationType;
        private Dictionary<string, string> aggregationParameters;

        public HerculesMetricEventReader(IBinaryBufferReader reader)
        {
            this.reader = reader;

            // Note(kungurtsev): deleting old cache with byte array buffer.
            tagsCache = TagsCache.Get(reader);
            aggregationParametersCache = AggregationParametersCache.Get(reader);
        }

        public IHerculesEventBuilder<MetricEvent> SetTimestamp(DateTimeOffset timestamp)
        {
            this.timestamp = timestamp;
            return this;
        }

        public MetricEvent BuildEvent()
        {
            return new MetricEvent(
                value ?? throw new ArgumentOutOfRangeException(nameof(value), "Unexpected null value."),
                tags ?? throw new ArgumentOutOfRangeException(nameof(tags), "Unexpected null tags."),
                timestamp,
                unit,
                aggregationType,
                aggregationParameters);
        }

        public new IHerculesTagsBuilder AddValue(string key, string value)
        {
            switch (key)
            {
                case TagNames.Unit:
                    unit = value;
                    break;
                case TagNames.AggregationType:
                    aggregationType = value;
                    break;
            }

            return this;
        }

        public new IHerculesTagsBuilder AddValue(string key, double value)
        {
            if (key == TagNames.Value)
                this.value = value;

            return this;
        }

        public new IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> valueBuilder)
        {
            if (key == TagNames.AggregationParameters)
            {
                AddAggregationParameters(valueBuilder);
            }
            else
            {
                valueBuilder(DummyBuilder);
            }

            return this;
        }

        public new IHerculesTagsBuilder AddVectorOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> valueBuilders)
        {
            if (key == TagNames.Tags)
            {
                AddTags(valueBuilders);
            }
            else
            {
                foreach (var valueBuilder in valueBuilders)
                {
                    valueBuilder(DummyBuilder);
                }
            }

            return this;
        }

        private void AddAggregationParameters(Action<IHerculesTagsBuilder> valueBuilder)
        {
            var startPosition = reader.Position;
            reader.SkipMode = true;
            valueBuilder(DummyBuilder);
            reader.SkipMode = false;
            var endPosition = reader.Position;

            var byteKey = new ByteArrayKey(reader.Buffer, startPosition, endPosition - startPosition);

            if (aggregationParametersCache.TryGetValue(byteKey, out aggregationParameters))
                return;

            reader.Position = startPosition;

            var builder = new HerculesMetricAggregationParametersReader();
            valueBuilder(builder);

            aggregationParametersCache[byteKey] = aggregationParameters = builder.Build();
        }

        private void AddTags(IReadOnlyList<Action<IHerculesTagsBuilder>> valueBuilders)
        {
            var startPosition = reader.Position;
            reader.SkipMode = true;
            foreach (var valueBuilder in valueBuilders)
            {
                valueBuilder(DummyBuilder);
            }

            reader.SkipMode = false;
            var endPosition = reader.Position;

            var byteKey = new ByteArrayKey(reader.Buffer, startPosition, endPosition - startPosition);

            if (tagsCache.TryGetValue(byteKey, out tags))
                return;

            reader.Position = startPosition;

            var list = new MetricTag[valueBuilders.Count];
            var tagBuilder = new HerculesMetricTagReader();

            for (var i = 0; i < valueBuilders.Count; i++)
            {
                valueBuilders[i](tagBuilder);
                list[i] = tagBuilder.Build();
            }

            tagsCache[byteKey] = tags = new MetricTags(list);
        }
    }
}