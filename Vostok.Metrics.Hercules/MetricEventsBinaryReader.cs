using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Commons.Binary;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Models;

namespace Vostok.Metrics.Hercules
{
    [PublicAPI]
    public class MetricEventsBinaryReader : IHerculesEventsBinaryReader<MetricEvent>
    {
        private const byte ProtocolVersion = 1;

        private byte[] bytes;
        private IBinaryReader reader;

        private Dictionary<ByteArrayKey, MetricTags> tagsCache;
        private Dictionary<ByteArrayKey, string> stringsCache;
        
        // ReSharper disable once ParameterHidesMember
        public IList<MetricEvent> Read(byte[] bytes, int offset)
        {
            this.bytes = bytes;

            reader = new BinaryBufferReader(bytes, offset)
            {
                Endianness = Endianness.Big
            };

            tagsCache = new Dictionary<ByteArrayKey, MetricTags>();
            stringsCache = new Dictionary<ByteArrayKey, string>();

            return reader.ReadArray(_ => ReadEvent());
        }

        private MetricEvent ReadEvent()
        {
            var version = reader.ReadByte();
            if (version != ProtocolVersion)
                throw new NotSupportedException($"Unsupported Hercules protocol version: {version}");

            var utcTimestamp = EpochHelper.FromUnixTimeUtcTicks(reader.ReadInt64());
            var timestamp = new DateTimeOffset(utcTimestamp, TimeSpan.Zero);

            reader.Position += 16;

            return ReadEvent(timestamp);
        }

        private MetricEvent ReadEvent(DateTimeOffset timestamp)
        {
            MetricTags tags = null;
            double? value = null;
            string unit = null;
            string aggregationType = null;
            IReadOnlyDictionary<string, string> aggregationParameters = null;

            var tagsCount = reader.ReadInt16();

            for (var i = 0; i < tagsCount; i++)
            {
                var key = ReadShortString();
                var valueType = (TagType)reader.ReadByte();

                switch (key)
                {
                    case TagNames.Value when valueType == TagType.Double:
                        value = reader.ReadDouble();
                        break;
                    case TagNames.Tags when valueType == TagType.Vector:
                        tags = ReadTags();
                        break;
                    case TagNames.AggregationParameters when valueType == TagType.Container:
                        aggregationParameters = ReadAggregationParameters();
                        break;
                    case TagNames.Unit when valueType == TagType.String:
                        unit = reader.ReadString();
                        break;
                    case TagNames.AggregationType when valueType == TagType.String:
                        aggregationType = reader.ReadString();
                        break;
                    case TagNames.TagsHash when valueType == TagType.Integer:
                        reader.Position += 4;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(key), key, $"Unexpected key with type {valueType}.");
                }
            }

            return new MetricEvent(
                value ?? throw new ArgumentOutOfRangeException(nameof(value), "Unexpected null value."),
                tags ?? throw new ArgumentOutOfRangeException(nameof(tags), "Unexpected null tags."),
                timestamp,
                unit,
                aggregationType,
                aggregationParameters);
        }

        private string ReadShortString()
        {
            var length = bytes[reader.Position];
            var byteKey = new ByteArrayKey(bytes, reader.Position, length);
            if (stringsCache.TryGetValue(byteKey, out var cachedValue))
            {
                reader.Position += length + 1;
                return cachedValue;
            }
            return stringsCache[byteKey] = reader.ReadShortString();
        }

        private IReadOnlyDictionary<string, string> ReadAggregationParameters()
        {
            return null;
        }

        private MetricTags ReadTags()
        {
            var lenght = GetTagsLength();
            var byteKey = new ByteArrayKey(bytes, reader.Position, lenght);
            if (tagsCache.TryGetValue(byteKey, out var cachedValue))
            {
                reader.Position += lenght;
                return cachedValue;
            }

            var elementType = (TagType)reader.ReadByte();
            if (elementType != TagType.Container)
                throw new ArgumentOutOfRangeException(nameof(elementType), "Unexpected tags element type.");

            var tagsCount = reader.ReadInt32();
            var tags = new MetricTag[tagsCount];

            for (var i = 0; i < tagsCount; i++)
            {
                var (key, value) = ReadKeyValueContainer();
                tags[i] = new MetricTag(key, value);
            }

            return tagsCache[byteKey] = new MetricTags(tags);
        }

        private (string key, string value) ReadKeyValueContainer()
        {
            var tagsCount = reader.ReadInt16();
            if (tagsCount != 2)
                throw new ArgumentOutOfRangeException(nameof(tagsCount), "Unexpected tags count.");

            string key = null;
            string value = null;

            for (var i = 0; i < tagsCount; i++)
            {
                var valueKey = reader.ReadShortString();
                var valueType = (TagType)reader.ReadByte();

                switch (valueKey)
                {
                    case TagNames.Key when valueType == TagType.String:
                        key = reader.ReadString();
                        break;
                    case TagNames.Value when valueType == TagType.String:
                        value = reader.ReadString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(key), key, $"Unexpected key with type {valueType}.");
                }
            }

            return (key, value);
        }

        private long GetTagsLength()
        {
            using (var jump = reader.JumpTo(reader.Position))
            {
                reader.Position++;
                var tagsCount = reader.ReadInt32();

                for (var i = 0; i < tagsCount; i++)
                {
                    var length = GetKeyValueContainerLength();
                    reader.Position += length;
                }

                return reader.Position - jump.Position;
            }
        }

        private long GetKeyValueContainerLength()
        {
            using (var jump = reader.JumpTo(reader.Position))
            {
                var tagsCount = reader.ReadInt16();

                for (var i = 0; i < tagsCount; i++)
                {
                    var keyLength = reader.ReadByte();
                    reader.Position += keyLength + 1;
                    var valueLength = reader.ReadInt32();
                    reader.Position += valueLength;
                }

                return reader.Position - jump.Position;
            }
        }
    }
}