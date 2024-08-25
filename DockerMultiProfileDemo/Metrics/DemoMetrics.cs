using System.Diagnostics.Metrics;

namespace DockerMultiProfileDemo.Metrics
{
    public class DemoMetrics
    {
        public const string MeterName = "dockermultiprofiledemo.api";

        private readonly Counter<long> _requestCounter;
        private readonly Histogram<double> _requestDuration;

        public DemoMetrics(IMeterFactory meterFactory)
        {
            using var meter = meterFactory.Create(MeterName);

            _requestCounter = meter.CreateCounter<long>(
                "dockermultiprofiledemo.api.dockermultiprofiledemo_requests.count");

            _requestDuration = meter.CreateHistogram<double>(
                "dockermultiprofiledemo.api.dockermultiprofiledemo_requests.count");
        }

        public void IncreaseImageRequestCount() => _requestCounter.Add(1);

        public TrackedRequestDuration MeasureRequestDuration() => new TrackedRequestDuration(_requestDuration);
    }

    public sealed class TrackedRequestDuration : IDisposable
    {
        private readonly long _requestStartTime = TimeProvider.System.GetTimestamp();
        private readonly Histogram<double> _histogram;

        public TrackedRequestDuration(Histogram<double> histogram)
        {
            _histogram = histogram;
        }

        public void Dispose()
        {
            var elapsed = TimeProvider.System.GetElapsedTime(_requestStartTime);
            _histogram.Record(elapsed.Microseconds);

            GC.SuppressFinalize(this);
        }
    }
}
