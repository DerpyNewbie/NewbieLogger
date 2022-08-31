using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ExecuteGlobalCommand : ActionCommandHandler
    {
        [SerializeField]
        private bool isOnlyModerator = true;

        [UdonSynced]
        private string _globalExecutionCommand;
        private int _lastRequestVersion;
        [UdonSynced]
        private int _requestVersion;

        private NewbieConsole _console;
        private bool _shouldExecuteOnRegister;

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
                _console.Println(
                    $"Globally executing command with request version of {_requestVersion}: \"{_globalExecutionCommand}\"");
                _console.Evaluate(_globalExecutionCommand);
            }
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (result.success)
                _Execute();
            else
                _console.LogError(
                    $"[{Label}] Serialization failed for {_requestVersion}: {_globalExecutionCommand}. Will not execute!");
        }

        public override void OnDeserialization()
        {
            _Execute();
        }

        public override string Label => "ExecuteGlobal";
        public override string Description => "Globally executes provided command.";
        public override string Usage => "<command> [commands...]";

        public override void OnRegistered(NewbieConsole console)
        {
            _console = console;
            if (_shouldExecuteOnRegister)
                _Execute();
        }

        public override void OnActionCommand(NewbieConsole console, string label,
            ref string[] vars, ref string[] envVars)
        {
            if (vars == null || vars.Length == 0)
                return;

            if (isOnlyModerator && !console.IsSuperUser)
            {
                console.Println($"Cannot execute this command: {string.Join(" ", vars)}");
                return;
            }

            _globalExecutionCommand = string.Join(" ", vars);
            ++_requestVersion;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            RequestSerialization();
        }
    }
}