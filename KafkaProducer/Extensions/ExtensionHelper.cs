using System;
using Serilog.Core;
using Serilog.Events;

namespace KafkaProducer.Extensions
{
    public static class ExtensionHelper
    {
        public static LoggingLevelSwitch GetLogLevel(this string level)
        {
            var loglevel = string.IsNullOrWhiteSpace(level) ? string.Empty : level.ToLower();

            return loglevel switch
            {
                "information" => new LoggingLevelSwitch(LogEventLevel.Information),
                "debug" => new LoggingLevelSwitch(LogEventLevel.Debug),
                "warning" => new LoggingLevelSwitch(LogEventLevel.Warning),
                "verbose" => new LoggingLevelSwitch(LogEventLevel.Verbose),
                "error" => new LoggingLevelSwitch(LogEventLevel.Error),
                "fatal" => new LoggingLevelSwitch(LogEventLevel.Fatal),
                _ => new LoggingLevelSwitch(LogEventLevel.Information)
            };
        }
    }
}
