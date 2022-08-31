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
                ((NewbieLogger)console.Out).MaxChars = request;
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
                var request = ParseLevel(args[1]);
                if (request == -1)
                {
                    console.Println("<color=red>Invalid log level name</color>");
                    return "invalid";
                }

                console.LogLevel = request;
            }

            console.Println($"LogLevel: {GetLevelString(console.Out)}");
            return $"{GetLevelString(console.Out)}";
        }

        private string GetTestCase()
        {
            return _testStringCases[Random.Range(0, _testStringCases.Length - 1)];
        }

        private static int ParseLevel(string text)
        {
            switch (text.ToLower())
            {
                case "a":
                case "all":
                    return (int)LogLevels.All;
                case "in":
                case "internal":
                    return (int)LogLevels.Internal;
                case "v":
                case "verbose":
                    return (int)LogLevels.Verbose;
                case "i":
                case "info":
                    return (int)LogLevels.Info;
                case "w":
                case "warn":
                case "warning":
                    return (int)LogLevels.Warn;
                case "e":
                case "error":
                    return (int)LogLevels.Error;
                case "off":
                    return (int)LogLevels.Off;
                default:
                    return -1;
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
        public override string[] Aliases => NewbieConsole.EmptyAlias();
        public override string Description => "Provides basic control of console object.";
        public override string Usage => "<command> <pickup|here|enable|clear|level|stressTest>";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
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
                default:
                    console.PrintUsage(this);
                    return ConsoleLiteral.GetNone();
            }
            // ReSharper restore StringLiteralTypo
        }
    }
}