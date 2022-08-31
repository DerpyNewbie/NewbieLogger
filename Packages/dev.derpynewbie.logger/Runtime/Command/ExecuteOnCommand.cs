using DerpyNewbie.Common;
using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ExecuteOnCommand : ActionCommandHandler
    {
        [SerializeField]
        private bool isOnlyModerator = true;

        private int _lastRequestVersion;
        [UdonSynced]
        private int _requestVersion;
        [UdonSynced]
        private string _targetExecutionCommand;

        private NewbieConsole _console;
        private bool _shouldExecuteOnRegister;

        public override void OnPostSerialization(SerializationResult result)
        {
            _Execute();
        }

        public override void OnDeserialization()
        {
            _Execute();
        }

        private void _Execute()
        {
            if (_console == null)
            {
                _shouldExecuteOnRegister = true;
                return;
            }

            if (_lastRequestVersion < _requestVersion)
            {
                _lastRequestVersion = _requestVersion;
                var args = _targetExecutionCommand.Split(' ');

                if (args.Length <= 1) return;

                var cmd = FormatCommand(args);
                var target = ConsoleParser.TryParsePlayer(args[0]);

                if (target != null && target.playerId == Networking.LocalPlayer.playerId)
                {
                    _console.Println(
                        $"{NewbieUtils.GetPlayerName(Networking.GetOwner(gameObject))} remotely executing command with request version of {_requestVersion}: {cmd}");
                    _console.Evaluate(cmd);
                }
                else
                {
                    _console.Println(
                        $"{NewbieUtils.GetPlayerName(Networking.GetOwner(gameObject))} is executing command with request version of {_requestVersion}: '{cmd}' at '{NewbieUtils.GetPlayerName(target)}");
                }
            }
        }

        private string FormatCommand(string[] args)
        {
            if (args.Length <= 1) return "";
            var commands = new string[args.Length - 1];
            for (var i = 1; i < args.Length; i++)
                commands[i - 1] = args[i];
            return string.Join(" ", commands);
        }

        public override string Label => "ExecuteOn";
        public override string Description => "Executes command at specified player's client.";
        public override string Usage => "<command> <player> [commands...]";

        public override void OnRegistered(NewbieConsole console)
        {
            _console = console;
            if (_shouldExecuteOnRegister)
                _Execute();
        }

        public override void OnActionCommand(NewbieConsole console, string label, ref string[] vars,
            ref string[] envVars)
        {
            if (vars == null || vars.Length == 0)
                return;

            if (isOnlyModerator && !console.IsSuperUser)
            {
                console.Println($"Cannot execute this command: {FormatCommand(vars)}");
                return;
            }

            if (ConsoleParser.TryParsePlayer(vars[0]) == null)
            {
                console.Println("<color=red>Player name or id is invalid</color>");
                return;
            }

            _targetExecutionCommand = string.Join(" ", vars);
            ++_requestVersion;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            RequestSerialization();
        }
    }
}