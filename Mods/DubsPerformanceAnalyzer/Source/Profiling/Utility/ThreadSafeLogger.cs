using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    public class PendingMessage
    {
        public string message;
        public StackTrace stackTrace;
        public LogMessageType severity;

        public PendingMessage(string messsage, StackTrace trace, LogMessageType severity)
        {
            this.message = messsage;
            this.stackTrace = trace;
            this.severity = severity;
        }
    }

    public static class ThreadSafeLogger
    {
        private static ConcurrentQueue<PendingMessage> messages = new ConcurrentQueue<PendingMessage>();

        public static void Message(string message)
        {
            if (!message.StartsWith("[Analyzer]")) message = message.Insert(0, "[Analyzer]");
            messages.Enqueue(new PendingMessage(message, new StackTrace(1, false), LogMessageType.Message));
        }
        public static void Warning(string message)
        {
            if (!message.StartsWith("[Analyzer]")) message = message.Insert(0, "[Analyzer]");
            messages.Enqueue(new PendingMessage(message, new StackTrace(1, false), LogMessageType.Warning));
        }
        public static void Error(string message)
        {
            if (!message.StartsWith("[Analyzer]")) message = message.Insert(0, "[Analyzer]");
            messages.Enqueue(new PendingMessage(message, new StackTrace(1, false), LogMessageType.Error));
        }

        public static void DisplayLogs()
        {
            while (messages.TryDequeue(out PendingMessage res))
            {
                switch (res.severity)
                {
                    case LogMessageType.Message: Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Message, res.message, ExtractTrace(res.stackTrace))); break;
                    case LogMessageType.Warning:
                        UnityEngine.Debug.Log(res.message);
                        Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Warning, res.message, ExtractTrace(res.stackTrace)));
                        break;
                    case LogMessageType.Error:
                        UnityEngine.Debug.LogError(res.message);
                        if (Prefs.PauseOnError && Current.ProgramState == ProgramState.Playing)
                        {
                            Find.TickManager.Pause();
                        }
                        Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Error, res.message, ExtractTrace(res.stackTrace)));

                        if (!PlayDataLoader.Loaded || Prefs.DevMode)
                        {
                            Log.TryOpenLogWindow();
                        }
                        break;
                }
                Log.PostMessage();
            }
        }

        public static string ExtractTrace(StackTrace stackTrace)
        {
            StringBuilder stringBuilder = new StringBuilder(255);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame frame = stackTrace.GetFrame(i);
                MethodBase method = frame.GetMethod();
                if ((object)method == null)
                {
                    continue;
                }
                Type declaringType = method.DeclaringType;
                if ((object)declaringType == null)
                {
                    continue;
                }
                string @namespace = declaringType.Namespace;
                if (@namespace != null && @namespace.Length != 0)
                {
                    stringBuilder.Append(@namespace);
                    stringBuilder.Append(".");
                }
                stringBuilder.Append(declaringType.Name);
                stringBuilder.Append(":");
                stringBuilder.Append(method.Name);
                stringBuilder.Append("(");
                int j = 0;
                ParameterInfo[] parameters = method.GetParameters();
                bool flag = true;
                for (; j < parameters.Length; j++)
                {
                    if (!flag)
                    {
                        stringBuilder.Append(", ");
                    }

                    else
                    {
                        flag = false;
                    }
                    stringBuilder.Append(parameters[j].ParameterType.Name);
                }
                stringBuilder.Append(")");
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }
    }
}
