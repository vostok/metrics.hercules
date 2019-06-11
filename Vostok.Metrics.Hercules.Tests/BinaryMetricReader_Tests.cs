using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Vostok.Commons.Binary;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Metrics.Hercules.Tests
{
    [TestFixture, Explicit]
    internal class BinaryMetricReader_Tests
    {
        [Test]
        public void Should_read_many_events()
        {
            var log = new SynchronousConsoleLog();
            var sw = Stopwatch.StartNew();

            var bytes = File.ReadAllBytes(@"C:\vostok\vostok.metrics.test\Reader\events.bytes");

            var reader = new BinaryBufferReader(bytes, 0)
            {
                Endianness = Endianness.Big
            };

            var coordinates = reader.ReadByteArray(580);

            var eventsReader = new BinaryMetricReader(bytes, reader);
            var events = reader.ReadArray(_ => eventsReader.ReadEvent());

            log.Info("Responce body parsed in {Elapsed}.", sw.Elapsed);
        }
    }
}