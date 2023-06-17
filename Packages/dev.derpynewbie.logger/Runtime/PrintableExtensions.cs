using JetBrains.Annotations;
using UnityEngine;

namespace DerpyNewbie.Logger
{
    public static class PrintableExtensions
    {
        private const string Aqua = "<color=#00ffffff>";
        private const string Black = "<color=#000000ff>";
        private const string Blue = "<color=#0000ffff>";
        private const string Brown = "<color=#a52a2aff>";
        private const string Cyan = Aqua;
        private const string DarkBlue = "<color=#0000a0ff>";
        private const string Fuchsia = "<color=#ff00ffff>";
        private const string Green = "<color=#008000ff>";
        private const string Grey = "<color=#808080ff>";
        private const string LightBlue = "<color=#add8e6ff>";
        private const string Lime = "<color=#00ff00ff>";
        private const string Magenta = Fuchsia;
        private const string Maroon = "<color=#800000ff>";
        private const string Navy = "<color=#800000ff>";
        private const string Olive = "<color=#808000ff>";
        private const string Orange = "<color=#ffa500ff>";
        private const string Purple = "<color=#800080ff>";
        private const string Red = "<color=#ff0000ff>";
        private const string Silver = "<color=#c0c0c0ff>";
        private const string Teal = "<color=#008080ff>";
        private const string White = "<color=#ffffffff>";
        private const string Yellow = "<color=#ffff00ff>";

        private const string EndColor = "</color>";

        private const string ErrorColor = Red;
        private const string WarnColor = Yellow;
        private const string InfoColor = Silver;
        private const string VerboseColor = Grey;

        private const string ErrorPrefix = ErrorColor + "[Error] " + EndColor;
        private const string WarnPrefix = WarnColor + "[Warn] " + EndColor;
        private const string InfoPrefix = InfoColor + "[Info] " + EndColor;
        private const string VerbosePrefix = VerboseColor + "[Verb] " + EndColor;
        private const string InternalPrefix = VerboseColor + "[Inner] " + EndColor;

        private const string FallbackPrefix = "[F]";

        [PublicAPI]
        public static void Log(this PrintableBase printable, string message)
        {
            printable.LogWithLevelFormatted(message, (int)LogLevels.Info);
        }

        [PublicAPI]
        public static void LogError(this PrintableBase printable, string message)
        {
            printable.LogWithLevelFormatted(message, (int)LogLevels.Error);
        }

        [PublicAPI]
        public static void LogWarn(this PrintableBase printable, string message)
        {
            printable.LogWithLevelFormatted(message, (int)LogLevels.Warn);
        }

        [PublicAPI]
        public static void LogVerbose(this PrintableBase printable, string message)
        {
            printable.LogWithLevelFormatted(message, (int)LogLevels.Verbose);
        }

        [PublicAPI]
        internal static void LogInternal(this PrintableBase printable, string message)
        {
            printable.LogWithLevelFormatted(message, (int)LogLevels.Internal);
        }

        [PublicAPI]
        public static string FormatWithLevel(string obj, int level)
        {
            if ((int)LogLevels.Error <= level) return $"{ErrorPrefix}{obj}";
            if ((int)LogLevels.Warn <= level) return $"{WarnPrefix}{obj}";
            if ((int)LogLevels.Info <= level) return $"{InfoPrefix}{obj}";
            if ((int)LogLevels.Verbose <= level) return $"{VerbosePrefix}{obj}";
            if ((int)LogLevels.Internal <= level) return $"{InternalPrefix}{obj}";
            return $"[L]{obj}";
        }

        [PublicAPI]
        public static void LogWithLevelFormatted(this PrintableBase printable, string obj, int level)
        {
            printable.LogWithLevel(FormatWithLevel(obj, level), level);
        }

        [PublicAPI]
        public static void LogWithLevel(this PrintableBase printable, string obj, int level)
        {
            if (printable == null)
            {
                Debug.Log(FallbackPrefix + obj);
                return;
            }

            if (level < printable.LogLevel)
            {
                if (level <= (int)LogLevels.Internal)
                    return;
                Debug.Log($"HIDDEN: {obj}");
                return;
            }

            Debug.Log(obj);
            printable.Println(obj);
        }

        [PublicAPI]
        public static void Println(this PrintableBase printable, string message)
        {
            NewLine(printable);
            Internal_Print(printable, message);
        }

        [PublicAPI]
        public static void NewLine(this PrintableBase printable)
        {
            Internal_Print(printable, "\n");
        }

        private static void Internal_Print(PrintableBase printable, string message)
        {
            if (printable)
                printable.Print(message);
            else
                Debug.Log(message);
        }
    }

    [PublicAPI]
    public enum LogLevels
    {
        Off = int.MaxValue,
        Error = 1000,
        Warn = 900,
        Info = 800,
        Verbose = 600,
        Internal = 100,
        All = 0
    }
}