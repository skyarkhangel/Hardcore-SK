using System.Collections.Generic;
using System.Text;
using Verse;

namespace StorageSelector.UI.Logging
{
    internal static class UILogger
    {
        private const string LogPrefix = "[StorageSelector] ";
        private static readonly Dictionary<string, List<LogEntry>> ErrorAggregator = new();
        private static readonly StringBuilder LogBuilder = new();

        public class LogEntry
        {
            public string Message { get; }
            public System.Exception Exception { get; }
            public string Context { get; }
            public System.DateTime Timestamp { get; }

            public LogEntry(string message, System.Exception exception = null, string context = null)
            {
                Message = message;
                Exception = exception;
                Context = context;
                Timestamp = System.DateTime.Now;
            }
        }

        internal static void LogError(string message, System.Exception e = null, string context = null)
        {
            var entry = new LogEntry(message, e, context);
            AggregateError(entry);

            LogBuilder.Clear()
                .Append(LogPrefix)
                .Append(message);

            if (context != null)
                LogBuilder.Append(" [Context: ").Append(context).Append("]");

            if (e != null)
                LogBuilder.Append(": ").Append(e);

            Log.Error(LogBuilder.ToString());
        }

        internal static void LogWarning(string message, string context = null)
        {
            LogBuilder.Clear()
                .Append(LogPrefix)
                .Append(message);

            if (context != null)
                LogBuilder.Append(" [Context: ").Append(context).Append("]");

            Log.Warning(LogBuilder.ToString());
        }

        internal static void LogMessage(string message, string context = null)
        {
            LogBuilder.Clear()
                .Append(LogPrefix)
                .Append(message);

            if (context != null)
                LogBuilder.Append(" [Context: ").Append(context).Append("]");

            Log.Message(LogBuilder.ToString());
        }

        internal static void LogLayoutError(string component, string details, System.Exception e = null)
        {
            LogError($"Layout error in {component}: {details}", e, "Layout");
        }

        internal static void LogControlError(string control, string action, System.Exception e)
        {
            LogError($"Error in {control} while {action}", e, "Controls");
        }

        internal static void LogWMError(string action, System.Exception e)
        {
            LogError($"What's Missing integration error while {action}", e, "WM Integration");
        }

        internal static void LogStorageError(string action, System.Exception e)
        {
            LogError($"Storage operation error while {action}", e, "Storage");
        }

        internal static void LogStateInfo(string component, string state, Dictionary<string, object> values = null)
        {
            LogBuilder.Clear()
                .Append("State info for ")
                .Append(component)
                .Append(": ")
                .Append(state);

            if (values != null)
            {
                LogBuilder.Append(" {");
                foreach (var kvp in values)
                {
                    LogBuilder.Append($"{kvp.Key}={kvp.Value?.ToString() ?? "null"}, ");
                }
                LogBuilder.Length -= 2;
                LogBuilder.Append("}");
            }

            LogMessage(LogBuilder.ToString(), "State");
        }

        internal static void LogRecoveryAttempt(string component, string action, bool success)
        {
            var status = success ? "succeeded" : "failed";
            LogMessage($"Recovery attempt for {component} ({action}) {status}", "Recovery");
        }

        private static void AggregateError(LogEntry entry)
        {
            string key = entry.Context ?? "General";
            if (!ErrorAggregator.TryGetValue(key, out var entries))
            {
                entries = new List<LogEntry>();
                ErrorAggregator[key] = entries;
            }

            entries.Add(entry);

            if (entries.Count >= 3)
            {
                LogBuilder.Clear()
                    .Append("Multiple errors in ")
                    .Append(key)
                    .Append(" (last 3): ");

                for (int i = entries.Count - 3; i < entries.Count; i++)
                {
                    LogBuilder.Append("\n  ")
                        .Append(entries[i].Timestamp.ToString("HH:mm:ss"))
                        .Append(": ")
                        .Append(entries[i].Message);
                }

                Log.Warning(LogBuilder.ToString());
            }
        }

        internal static void ClearErrorAggregator()
        {
            ErrorAggregator.Clear();
        }

        internal static Dictionary<string, int> GetErrorCounts()
        {
            var counts = new Dictionary<string, int>();
            foreach (var kvp in ErrorAggregator)
            {
                counts[kvp.Key] = kvp.Value.Count;
            }
            return counts;
        }

        internal static List<LogEntry> GetErrorsForContext(string context)
        {
            return ErrorAggregator.TryGetValue(context, out var entries)
                ? new List<LogEntry>(entries)
                : new List<LogEntry>();
        }
    }
}
