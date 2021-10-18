using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace shLog
{
    static class Tracer
    {
        public static ILog GetDyamicLogger(string _LogName, string _Path)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            RollingFileAppender roller = new RollingFileAppender();
            roller.LockingModel = new log4net.Appender.FileAppender.MinimalLock();
            roller.AppendToFile = true;
            roller.RollingStyle = RollingFileAppender.RollingMode.Date;
            roller.StaticLogFileName = false;

            roller.Name = _LogName;
            roller.DatePattern = $"yyyy-MM-dd\\\\HH\\\\'{_LogName}.Log'";
            roller.Layout = new log4net.Layout.PatternLayout();
            roller.File = _Path;

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%message%newline";
            patternLayout.ActivateOptions();

            roller.Layout = patternLayout;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            DummyLogger dummyILogger = new DummyLogger(_LogName.ToString());
            dummyILogger.Hierarchy = hierarchy;
            dummyILogger.Level = log4net.Core.Level.All;
            dummyILogger.AddAppender(roller);

            return new LogImpl(dummyILogger);
        }

        internal sealed class DummyLogger : Logger
        {
            // Methods
            internal DummyLogger(string name)
                : base(name)
            {
            }
        }
    }
}