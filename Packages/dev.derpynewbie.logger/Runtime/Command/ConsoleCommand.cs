using DerpyNewbie.Common;
using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ConsoleCommand : NewbieConsoleCommandHandler
    {
        [SerializeField]
        private VRCPickup pickup;
        [SerializeField]
        private GameObject consoleObject;

        private readonly string[] _testStringCases =
        {
            "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ",
            "ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ",
            "０１２３４５６７８９",
            "!\"#$%&'()*+,-./0123456789:;<=>?@[\\]^_`",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "abcdefghijklmnopqrstuvwxyz"
        };

        private int _currTestCount;
        private bool _isRunning;

        private NewbieConsole _testConsoleInstance;
        private int _testRetryCount;
        private float _testRetryDelay;

        private string HandleHere(NewbieConsole console, string[] arguments)
        {
            var head = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            var targetRot = head.rotation;
            var targetPos = head.position + targetRot * Vector3.forward;

            var pickupTransform = pickup.transform;
            pickupTransform.position = targetPos;
            pickupTransform.rotation = targetRot;
            console.Println("Successfully teleported console on you!");
            return "";
        }

        public void DoStressTest()
        {
            if (_currTestCount >= _testRetryCount)
            {
                _testConsoleInstance.Println("Stress Test Ended!");
                _isRunning = false;
                return;
            }

            _testConsoleInstance.Out.LogVerbose("StressTest-" + GetTestCase());
            ++_currTestCount;
            SendCustomEventDelayedSeconds(nameof(DoStressTest), _testRetryDelay);
        }

        private string StressTest(NewbieConsole console, string[] args)
        {
            if (_isRunning)
            {
                console.Println($"Currently Running: {_currTestCount}");
                return "running";
            }

            _testConsoleInstance = console;
            _testRetryCount = 500;
            _testRetryDelay = 0.25F;
            _currTestCount = 0;
            _isRunning = true;

            if (args.Length >= 3)
            {
                _testRetryCount = ConsoleParser.TryParseInt(args[1]);
                _testRetryDelay = ConsoleParser.TryParseFloat(args[2]);
            }

            console.Println($"Beginning stress test: count: {_testRetryCount}, delay: {_testRetryDelay}");
            DoStressTest();

            return "start";
        }

        private string HandleLength(NewbieConsole console, string[] args)
        {
            if (args.Length >= 2)
            {
                var request = ConsoleParser.TryParseInt(args[1]);
                console.Out.MaxChars = request;
            }

            console.Println($"MaxCharLength: {((NewbieLogger)console.Out).MaxChars}");
            return $"{((NewbieLogger)console.Out).MaxChars}";
        }

        private string HandlePickup(NewbieConsole console, string[] args)
        {
            if (args.Length >= 2)
            {
                var request = ConsoleParser.TryParseBoolean(args[1], pickup.pickupable);
                pickup.pickupable = request;
            }

            console.Println($"Pickupable: {pickup.pickupable}");
            return $"{pickup.pickupable}";
        }

        private string HandleEnable(NewbieConsole console, string[] args)
        {
            if (args.Length >= 2)
            {
                var request = ConsoleParser.TryParseBoolean(args[1], consoleObject.activeSelf);
                pickup.gameObject.SetActive(request);
                consoleObject.SetActive(request);
            }

            console.Println($"IsEnabled: {consoleObject.activeSelf}");
            return $"{consoleObject.activeSelf}";
        }

        private string HandleLevel(NewbieConsole console, string[] args)
        {
            if (args.Length >= 2)
            {
                if (!ParseLevel(args[1], out var request))
                {
                    console.Println("<color=red>Invalid log level name</color>");
                    return "invalid";
                }

                console.LogLevel = (int)request;
            }

            console.Println($"LogLevel: {GetLevelString(console.Out)}");
            return $"{GetLevelString(console.Out)}";
        }

        private string HandleLog(NewbieConsole console, string[] args)
        {
            var level = LogLevels.Info;
            var msg = args.GetSpanOfArray(1, args.Length - 1);
            if (args.Length >= 2 && ParseLevel(args[1], out var inLevel))
            {
                level = inLevel;
                msg = args.GetSpanOfArray(2, args.Length - 2);
            }

            var msgString = string.Join(" ", msg);
            console.LogWithLevelFormatted(msgString, (int)level);

            return msgString;
        }

        private string GetTestCase()
        {
            return _testStringCases[Random.Range(0, _testStringCases.Length - 1)];
        }

        private static bool ParseLevel(string text, out LogLevels level)
        {
            switch (text.ToLower())
            {
                case "a":
                case "all":
                    level = LogLevels.All;
                    return true;
                case "in":
                case "inner":
                case "internal":
                    level = LogLevels.Internal;
                    return true;
                case "v":
                case "verb":
                case "verbose":
                    level = LogLevels.Verbose;
                    return true;
                case "i":
                case "info":
                    level = LogLevels.Info;
                    return true;
                case "w":
                case "warn":
                case "warning":
                    level = LogLevels.Warn;
                    return true;
                case "e":
                case "err":
                case "error":
                    level = LogLevels.Error;
                    return true;
                case "off":
                    level = LogLevels.Off;
                    return true;
                default:
                    level = LogLevels.Off;
                    return false;
            }
        }

        private static string GetLevelString(PrintableBase l)
        {
            var level = l.LogLevel;
            return (int)LogLevels.Off <= level ? "Off" :
                (int)LogLevels.Error <= level ? "Error" :
                (int)LogLevels.Warn <= level ? "Warn" :
                (int)LogLevels.Info <= level ? "Info" :
                (int)LogLevels.Verbose <= level ? "Verb" :
                (int)LogLevels.Internal <= level ? "Internal" : "All";
        }

        public override string Label => "Console";
        public override string[] Aliases => new[]
            { "Log", "LogVerb", "LogVerbose", "LogInfo", "LogWarn", "LogWarning", "LogErr", "LogError" };
        public override string Description => "Provides basic control of console object.";
        public override string Usage => "<command> <pickup|here|enable|clear|level|stressTest|log>";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            var lowerLabel = label.ToLower();
            if (lowerLabel.StartsWith("log"))
                switch (lowerLabel)
                {
                    case "logverb":
                    case "logverbose":
                        console.LogVerbose(string.Join(" ", vars));
                        return ConsoleLiteral.GetNone();
                    case "logwarn":
                    case "logwarning":
                        console.LogWarn(string.Join(" ", vars));
                        return ConsoleLiteral.GetNone();
                    case "logerr":
                    case "logerror":
                        console.LogError(string.Join(" ", vars));
                        return ConsoleLiteral.GetNone();
                    case "log":
                    case "loginfo":
                    default:
                        console.Log(string.Join(" ", vars));
                        return ConsoleLiteral.GetNone();
                }

            if (vars == null || vars.Length == 0)
            {
                console.PrintUsage(this);
                return ConsoleLiteral.GetNone();
            }

            // ReSharper disable StringLiteralTypo
            switch (vars[0].ToLower())
            {
                case "p":
                case "pickup":
                    return HandlePickup(console, vars);
                case "h":
                case "here":
                    return HandleHere(console, vars);
                case "e":
                case "enable":
                    return HandleEnable(console, vars);
                case "l":
                case "level":
                    return HandleLevel(console, vars);
                case "len":
                case "length":
                    return HandleLength(console, vars);
                case "st":
                case "stresstest":
                    return StressTest(console, vars);
                case "log":
                    return HandleLog(console, vars);
                default:
                    console.PrintUsage(this);
                    return ConsoleLiteral.GetNone();
            }
            // ReSharper restore StringLiteralTypo
        }
    }
}