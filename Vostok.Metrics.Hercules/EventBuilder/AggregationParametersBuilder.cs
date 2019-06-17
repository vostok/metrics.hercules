﻿using System.Collections.Generic;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Metrics.Hercules.EventBuilder
{
    internal class AggregationParametersBuilder : DummyHerculesTagsBuilder, IHerculesTagsBuilder
    {
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        IHerculesTagsBuilder IHerculesTagsBuilder.AddValue(string key, string value)
        {
            parameters[key] = value;
            return this;
        }

        public Dictionary<string, string> Build()
        {
            return parameters;
        }
    }
}