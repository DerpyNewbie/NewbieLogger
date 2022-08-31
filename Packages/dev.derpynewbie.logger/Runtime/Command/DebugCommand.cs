using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DebugCommand : BoolCommandHandler
    {
        [SerializeField]
        private string commandLabel;
        [SerializeField]
        private string[] commandAliases;
        [SerializeField]
        private string commandDescription;
        [SerializeField] [TextArea]
        private string onEnabledCommand;
        [SerializeField] [TextArea]
        private string onDisabledCommand;
        [SerializeField]
        private float executionDelay = 0.5F;

        private NewbieConsole _instance;
        private bool _hasEnabled;
        private int _lastExecutedIndex;
        private string[] _onDisabledCommandArray;
        private string[] _onEnabledCommandArray;

        public override string Label => commandLabel;

        public override string[] Aliases => commandAliases;

        public override string Description => commandDescription;

        public override string Usage => "<command> [true|false]";

        public override bool OnBoolCommand(NewbieConsole console, string label, ref string[] vars, ref string[] envVars)
        {
            _instance = console;

            if (vars != null && vars.Length >= 1)
            {
                _onEnabledCommandArray = onEnabledCommand.Split('\n');
                _onDisabledCommandArray = onDisabledCommand.Split('\n');
                var request = ConsoleParser.TryParseBoolean(vars[0], _hasEnabled);
                _lastExecutedIndex = -1;

                if (request)
                    ExecuteNextEnabled();
                else
                    ExecuteNextDisabled();

                _hasEnabled = request;
                return _hasEnabled;
            }

            console.Println($"{label}: {_hasEnabled}");
            return _hasEnabled;
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(commandLabel))
                commandLabel = gameObject.name;
            if (string.IsNullOrEmpty(commandDescription))
                commandDescription = "Executes pre-defined commands";
        }

        public void ExecuteNextDisabled()
        {
            if (_onDisabledCommandArray.Length <= ++_lastExecutedIndex)
            {
                _instance.Println(
                    $"<color=#008080>Completed debug disable execution of</color> <color=green>{name}</color>");
                return;
            }

            var command = _onDisabledCommandArray[_lastExecutedIndex];
            if (command == "")
                _instance.NewLine();
            else
                _instance.Evaluate(command);

            SendCustomEventDelayedSeconds(nameof(ExecuteNextDisabled), executionDelay);
        }

        public void ExecuteNextEnabled()
        {
            if (_onEnabledCommandArray.Length <= ++_lastExecutedIndex)
            {
                _instance.Println(
                    $"<color=#008080>Completed debug enable execution of</color> <color=green>{name}</color>");
                return;
            }

            var command = _onEnabledCommandArray[_lastExecutedIndex];
            if (command == "")
                _instance.NewLine();
            else
                _instance.Evaluate(command);

            SendCustomEventDelayedSeconds(nameof(ExecuteNextEnabled), executionDelay);
        }
    }
}