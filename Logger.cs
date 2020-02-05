using System;
using System.Collections.Generic;

namespace ModTheGungeon {
    public class Logger {
        public enum LogLevel {
            Error = 0,
            Warn = 1,
            Info = 2,
            Debug = 3
        }

        public LogLevel MaxLogLevel =
#if DEBUG
            LogLevel.Debug;
#else
            LogLevel.Warn;
#endif

        public string ID;

        public Logger(string id, bool? write_console = null) {
            ID = id;
            WriteConsoleLocal = write_console;
        }

        public delegate void Subscriber(
            Logger logger,
            LogLevel loglevel,
            bool indent,
            string str
        );
        public delegate void LocalSubscriber(
            LogLevel loglevel,
            bool indent,
            string str
        );

        private static List<Subscriber> _Subscribers = null;
        private List<LocalSubscriber> _LocalSubscribers = null;

        public static void Subscribe(Subscriber sub) {
            if (_Subscribers == null) _Subscribers = new List<Subscriber> { sub };
            else _Subscribers.Add(sub);
        }

        public static void Unsubscribe(Subscriber sub) {
            _Subscribers.Remove(sub);
        }

        public void Subscribe(LocalSubscriber sub) {
            if (_LocalSubscribers == null) _LocalSubscribers = new List<LocalSubscriber>{ sub };
            else _LocalSubscribers.Add(sub);
        }

        public void Unsubscribe(LocalSubscriber sub) {
            _LocalSubscribers.Remove(sub);
        }

        public bool LogLevelEnabled(LogLevel level) {
            return MaxLogLevel >= level;
        }

        public void NotifySubscribers(LogLevel level, bool indent, object o, IList<LocalSubscriber> local_subscriber_blacklist = null, IList<Subscriber> subscriber_blacklist = null) {
            if (_LocalSubscribers != null) {
                for (int i = 0; i < _LocalSubscribers.Count; i++) {
                    if (local_subscriber_blacklist != null && local_subscriber_blacklist.Contains(_LocalSubscribers[i])) continue;
                    _LocalSubscribers[i].Invoke(level, indent, o.ToString());
                }
            }
            if (_Subscribers != null) {
                for (int i = 0; i < _Subscribers.Count; i++) {
                    if (subscriber_blacklist != null && subscriber_blacklist.Contains(_Subscribers[i])) continue;
                    _Subscribers[i].Invoke(this, level, indent, o.ToString());
                }
            }
        }

        private string _DebugPrefix = null;
        private string _InfoPrefix = null;
        private string _WarnPrefix = null;
        private string _ErrorPrefix = null;

        public static bool WriteConsoleDefault = true;
        public bool? WriteConsoleLocal;

        public bool WriteConsole => WriteConsoleLocal ?? WriteConsoleDefault;

        public string DebugPrefix {
            get {
                if (_DebugPrefix != null) return _DebugPrefix;
                return _DebugPrefix = $"[{ID} DEBUG] ";
            }
        }
        public string InfoPrefix {
            get {
                if (_InfoPrefix != null) return _InfoPrefix;
                return _InfoPrefix = $"[{ID} INFO] ";
            }
        }
        public string WarnPrefix {
            get {
                if (_WarnPrefix != null) return _WarnPrefix;
                return _WarnPrefix = $"[{ID} WARNING] ";
            }
        }
        public string ErrorPrefix {
            get {
                if (_ErrorPrefix != null) return _ErrorPrefix;
                return _ErrorPrefix = $"[{ID} ERROR] ";
            }
        }

        private string _DebugIndentPrefix = null;
        private string _InfoIndentPrefix = null;
        private string _WarnIndentPrefix = null;
        private string _ErrorIndentPrefix = null;

        public string DebugIndentPrefix {
            get {
                if (_DebugIndentPrefix != null) return _DebugIndentPrefix;
                return _DebugIndentPrefix = new String(' ', DebugPrefix.Length);
            }
        }
        public string InfoIndentPrefix {
            get {
                if (_InfoIndentPrefix != null) return _InfoIndentPrefix;
                return _InfoIndentPrefix = new String(' ', InfoPrefix.Length);
            }
        }
        public string WarnIndentPrefix {
            get {
                if (_WarnIndentPrefix != null) return _WarnIndentPrefix;
                return _WarnIndentPrefix = new String(' ', WarnPrefix.Length);
            }
        }
        public string ErrorIndentPrefix {
            get {
                if (_ErrorIndentPrefix != null) return _ErrorIndentPrefix;
                return _ErrorIndentPrefix = new String(' ', ErrorPrefix.Length);
            }
        }

        public void DebugPretty(object o) {
            if (!LogLevelEnabled(LogLevel.Debug)) return;
            var s = o.ToString();
            var split = s.Split('\n');
            for (int i = 0; i < split.Length; i++) {
                var e = split[i];
                if (i == 0) Debug(e);
                else DebugIndent(e);
            }
        }

        public void InfoPretty(object o) {
            if (!LogLevelEnabled(LogLevel.Info)) return;
            var s = o.ToString();
            var split = s.Split('\n');
            for (int i = 0; i < split.Length; i++) {
                var e = split[i];
                if (i == 0) Info(e);
                else InfoIndent(e);
            }
        }

        public void WarnPretty(object o) {
            if (!LogLevelEnabled(LogLevel.Warn)) return;
            var s = o.ToString();
            var split = s.Split('\n');
            for (int i = 0; i < split.Length; i++) {
                var e = split[i];
                if (i == 0) Warn(e);
                else WarnIndent(e);
            }
        }

        public void ErrorPretty(object o) {
            if (!LogLevelEnabled(LogLevel.Error)) return;
            var s = o.ToString();
            var split = s.Split('\n');
            for (int i = 0; i < split.Length; i++) {
                var e = split[i];
                if (i == 0) Error(e);
                else ErrorIndent(e);
            }
        }

        public void Debug(object o) {
            if (!LogLevelEnabled(LogLevel.Debug)) return;

            if (WriteConsole) Console.WriteLine(String(LogLevel.Debug, o));
            NotifySubscribers(LogLevel.Debug, false, o);
        }

        public void Info(object o) {
            if (!LogLevelEnabled(LogLevel.Info)) return;

            if (WriteConsole) Console.WriteLine(String(LogLevel.Info, o));
            NotifySubscribers(LogLevel.Info, false, o);
        }

        public void Warn(object o) {
            if (!LogLevelEnabled(LogLevel.Warn)) return;

            if (WriteConsole) Console.WriteLine(String(LogLevel.Warn, o));
            NotifySubscribers(LogLevel.Warn, false, o);
        }

        public void Error(object o, bool @throw = false) {
            if (!LogLevelEnabled(LogLevel.Error)) return;

            if (WriteConsole) Console.WriteLine(String(LogLevel.Error, o));
            NotifySubscribers(LogLevel.Error, false, o);
            if (@throw) {
                throw new Exception(o.ToString());
            }
        }

        public void DebugIndent(object o) {
            if (!LogLevelEnabled(LogLevel.Debug)) return;

            if (WriteConsole) Console.WriteLine(String(LogLevel.Debug, o, indent: true));
            NotifySubscribers(LogLevel.Debug, false, o);
        }

        public void InfoIndent(object o) {
            if (!LogLevelEnabled(LogLevel.Info)) return;

            if (WriteConsole) Console.WriteLine(String(LogLevel.Info, o, indent: true));
            NotifySubscribers(LogLevel.Info, false, o);
        }

        public void WarnIndent(object o) {
            if (!LogLevelEnabled(LogLevel.Warn)) return;

            if (WriteConsole) Console.WriteLine(String(LogLevel.Warn, o, indent: true));
            NotifySubscribers(LogLevel.Warn, false, o);
        }

        public void ErrorIndent(object o) {
            if (!LogLevelEnabled(LogLevel.Error)) return;

            if (WriteConsole) Console.WriteLine(String(LogLevel.Error, o, indent: true));
            NotifySubscribers(LogLevel.Error, false, o);
        }


        public void Log(LogLevel level, object o) {
            switch (level) {
            case LogLevel.Info: Info(o); break;
            case LogLevel.Debug: Debug(o); break;
            case LogLevel.Error: Error(o); break;
            case LogLevel.Warn: Warn(o); break;
            default: throw new Exception($"Wrong log level: {level}");
            }
        }

        public string String(LogLevel level, object o, bool indent = false) {
            if (indent) {
                switch (level) {
                case LogLevel.Info: return $"{InfoIndentPrefix}{o}";
                case LogLevel.Debug: return $"{DebugIndentPrefix}{o}";
                case LogLevel.Warn: return $"{WarnIndentPrefix}{o}";
                case LogLevel.Error: return $"{ErrorIndentPrefix}{o}";
                default: throw new Exception($"Wrong log level: {level}");
                }
            }

            switch (level) {
            case LogLevel.Info: return $"{InfoPrefix}{o}";
            case LogLevel.Debug: return $"{DebugPrefix}{o}";
            case LogLevel.Warn: return $"{WarnPrefix}{o}";
            case LogLevel.Error: return $"{ErrorPrefix}{o}";
            default: throw new Exception($"Wrong log level: {level}");
            }
        }
    }
}
