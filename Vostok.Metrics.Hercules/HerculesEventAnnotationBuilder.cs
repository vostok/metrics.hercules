using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Metrics.Models;

namespace Vostok.Metrics.Hercules
{
    /// <summary>
    /// Converts <see cref="AnnotationEvent"/>s to <see cref="HerculesEvent"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesEventAnnotationBuilder
    {
        public static HerculesEvent Build([NotNull] AnnotationEvent @event)
        {
            var builder = new HerculesEventBuilder();
            Build(@event, builder);
            return builder.BuildEvent();
        }

        public static void Build([NotNull] AnnotationEvent @event, [NotNull] IHerculesEventBuilder builder)
        {
            builder.SetTimestamp(@event.Timestamp);

            builder.AddValue(TagNames.Description, @event.Description ?? "no description");
            
            builder.AddVectorOfContainers(
                TagNames.Tags,
                @event.Tags,
                (b, tag) =>
                {
                    b.AddValue(TagNames.Key, tag.Key);
                    b.AddValue(TagNames.Value, tag.Value);
                });
        }
    }
}
