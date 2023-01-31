﻿using Microsoft.Extensions.Logging;
using Mindscape.Raygun4Maui;
using Mindscape.Raygun4Net;

namespace Raygun4Maui.RaygunLogger
{
    public sealed class RaygunLogger : ILogger
    {
        private readonly string _name;
        private readonly RaygunLoggerConfiguration _raygunLoggerConfiguration;

        public RaygunLogger(string name, RaygunLoggerConfiguration raygunLoggerConfiguration) =>
            (_name, _raygunLoggerConfiguration) = (name, raygunLoggerConfiguration);

        //TODO: Get from https://github.com/MindscapeHQ/serilog-sinks-raygun/blob/dev/src/Serilog.Sinks.Raygun/Sinks/Raygun/RaygunSink.cs
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) => (logLevel >= _raygunLoggerConfiguration.MinLogLevel && logLevel <= _raygunLoggerConfiguration.MaxLogLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            RaygunClient raygunClient = RaygunClientFactory(_raygunLoggerConfiguration);
            raygunClient.SendInBackground(
                new Exception(formatter(state, exception)),
                _raygunLoggerConfiguration.SendDefaultTags ? new List<string>() {logLevel.ToString(), RaygunMauiClient.GetBuildPlatform()} : null,
                _raygunLoggerConfiguration.SendDefaultCustomData ? new Dictionary<string, object>() { {"logLevel", logLevel}, {"eventId", eventId}, { "state", state }, { "name", _name }, {"message", formatter(state, exception) } } : null
            );
        }

        private static RaygunClient RaygunClientFactory(RaygunSettingsBase raygunSettingsBase) =>
            new(raygunSettingsBase);

    }
}
