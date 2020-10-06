namespace Vostok.Metrics.Hercules
{
    // https://github.com/vostok/hercules/blob/master/doc/event-schema/metric-event-schema.md
    // https://github.com/vostok/hercules/blob/master/doc/event-schema/annotation-event-schema.md
    internal static class TagNames
    {
        public const string Key = "key";
        public const string Value = "value";
        public const string Tags = "tags";
        public const string TagsHash = "tagsHash";
        public const string Unit = "unit";
        public const string Description = "description";
        public const string AggregationType = "aggregationType";
        public const string AggregationParameters = "aggregationParameters";
    }
}