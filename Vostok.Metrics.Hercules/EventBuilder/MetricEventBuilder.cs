using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Models;
// ReSharper disable ParameterHidesMember

namespace Vostok.Metrics.Hercules.EventBuilder
{
    [PublicAPI]
    public class MetricEventBuilder : DummyHerculesTagsBuilder, IHerculesEventBuilder<MetricEvent>
    {
        private readonly IBinaryBufferReader reader;

        private double? value;
        private MetricTags tags;
        private DateTimeOffset timestamp;
        private string unit;
        private string aggregationType;
        private Dictionary<string, string> aggregationParameters;

        private readonly Dictionary<ByteArrayKey, MetricTags> tagsCache;
        private readonly Dictionary<ByteArrayKey, Dictionary<string, string>> aggregationParametersCache;

        private static readonly DummyHerculesTagsBuilder DummyBuilder = new DummyHerculesTagsBuilder();

        public MetricEventBuilder(IBinaryBufferReader reader)
        {
            this.reader = reader;

            // Note(kungurtsev): deleting old cache with byte array buffer.
            tagsCache = MetricEventBuilderCache.TagsCache.Get(reader);
            aggregationParametersCache = MetricEventBuilderCache.AggregationParametersCache.Get(reader);
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

        IHerculesTagsBuilder IHerculesTagsBuilder.AddValue(string key, double value)
        {
            if (key == TagNames.Value)
                this.value = value;

            return this;
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

        IHerculesTagsBuilder IHerculesTagsBuilder.AddContainer(string key, Action<IHerculesTagsBuilder> valueBuilder)
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

        IHerculesTagsBuilder IHerculesTagsBuilder.AddVectorOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> valueBuilders)
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

            var builder = new AggregationParametersBuilder();
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
            var tagBuilder = new TagBuilder();

            for (var i = 0; i < valueBuilders.Count; i++)
            {
                valueBuilders[i](tagBuilder);
                list[i] = tagBuilder.Build();
            }

            tagsCache[byteKey] = tags = new MetricTags(list);
        }
    }
}