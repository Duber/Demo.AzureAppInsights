using System;
using System.Reflection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace Demo.AppInsights.Log
{
    public class AppInsightsLog : ILogger
    {
        protected readonly LogLevel _logLevel = LogLevel.Debug; // ToDo: read from config
        private readonly TelemetryClient _telemetryClient;

        public AppInsightsLog(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (exception != null)
            {
                _telemetryClient.TrackException(exception);
                _telemetryClient.Flush();
                return;
            }
            var message = formatter(state, null);
            _telemetryClient.TrackTrace(message, GetSeverityLevel(logLevel));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logLevel <= logLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoOpDisposable.Instance;
        }

        private class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance = new NoOpDisposable();

            public void Dispose()
            {
            }
        }

        private static SeverityLevel GetSeverityLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical: return SeverityLevel.Critical;
                case LogLevel.Error: return SeverityLevel.Error;
                case LogLevel.Warning: return SeverityLevel.Warning;
                case LogLevel.Information: return SeverityLevel.Information;
                case LogLevel.Trace:
                default: return SeverityLevel.Verbose;
            }
        }
    }
}