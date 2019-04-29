using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Model;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Metrics.Hercules.Tests
{
    [TestFixture]
    internal class EventConversion_Tests
    {
        private HerculesMetricSenderSettings settings;
        private HerculesMetricSender sender;
        private HerculesEventBuilder builder;

        [SetUp]
        public void TestSetup()
        {
            builder = new HerculesEventBuilder();

            var sink = Substitute.For<IHerculesSink>(); 

            sink
                .WhenForAnyArgs(s => s.Put(null, null))
                .Do(info => info.Arg<Action<IHerculesEventBuilder>>().Invoke(builder));

            settings = new HerculesMetricSenderSettings(sink);

            sender = new HerculesMetricSender(settings);
        }

        [Test]
        public void Should_convert_simple_events()
        {
            var tags = MetricTags.Empty
                .Append("k1", "v1")
                .Append("k2", "v2");

            var metricEvent = new MetricEvent(324.342d, tags, DateTimeOffset.UtcNow, null, null, null);

            TestEventConversion(metricEvent);
        }

        [Test]
        public void Should_convert_complex_events()
        {
            var tags = MetricTags.Empty
                .Append("k1", "v1")
                .Append("k2", "v2");

            var metricEvent = new MetricEvent(324.342d, tags, DateTimeOffset.UtcNow, 
                WellKnownUnits.Milliseconds, WellKnownAggregationTypes.Counter, new Dictionary<string, string>
                {
                    ["Param1"] = "value1",
                    ["Param2"] = "value2"
                });

            TestEventConversion(metricEvent);
        }

        private void TestEventConversion(MetricEvent metricEvent)
        {
            sender.Send(metricEvent);

            var herculesEvent = builder.BuildEvent();

            var convertedMetricEvent = HerculesMetricEventFactory.CreateFrom(herculesEvent);

            convertedMetricEvent.Should().Be(metricEvent);
        }
    }
}