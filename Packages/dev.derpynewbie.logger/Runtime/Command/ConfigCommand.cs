using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ConfigCommand : NewbieConsoleCommandHandler
    {
        [SerializeField]
        private string commandLabel;
        [SerializeField]
        private string[] commandAliases;
        [SerializeField]
        private string commandDescription;
        [SerializeField] [TextArea(5, 50)]
        private string commandsToExecute;
        [SerializeField]
        private float executionDelay = 0.5F;
        [SerializeField]
        private bool printBeginEnd = true;

        private NewbieConsole _instance;
        private string[] _commandsToExecuteArray;
        private int _lastExecutedIndex;

        public override string Label => commandLabel;

        public override string[] Aliases => commandAliases;

        public override string Description => commandDescription;

        public override string Usage => "<command>";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            _instance = console;
            _commandsToExecuteArray = commandsToExecute.Split('\n');
            if (printBeginEnd)
                console.Println($"<color=#008080>Starting config execution of</color> <color=green>{Label}</color>");
            _lastExecutedIndex = -1;
            ExecuteNext();
            return "";
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(commandLabel))
                commandLabel = gameObject.name;
            if (string.IsNullOrEmpty(commandDescription))
                commandDescription = "Executes pre-defined commands";
        }

        public void ExecuteNext()
        {
            if (_commandsToExecuteArray.Length <= ++_lastExecutedIndex)
            {
                if (printBeginEnd)
                    _instance.Println(
                        $"<color=#008080>Completed config execution of</color> <color=green>{Label}</color>");
                return;
            }

            var command = _commandsToExecuteArray[_lastExecutedIndex];
            if (command == "")
                _instance.NewLine();
            else
                _instance.Evaluate(command);

            SendCustomEventDelayedSeconds(nameof(ExecuteNext), executionDelay);
        }
    }
}