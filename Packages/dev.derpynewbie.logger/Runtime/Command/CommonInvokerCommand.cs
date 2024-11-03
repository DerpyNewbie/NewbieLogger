using System;
using DerpyNewbie.Common;
using DerpyNewbie.Common.Invoker;
using UnityEngine;

namespace DerpyNewbie.Logger.Command
{
    public class CommonInvokerCommand : ActionCommandHandler
    {
        [SerializeField]
        private string commandLabel;

        [SerializeField]
        private string[] commandAliases;

        [SerializeField]
        private string commandDescription;

        [SerializeField]
        private CommonInvokerBase[] invokers;

        public override string Label => commandLabel;
        public override string[] Aliases => commandAliases;
        public override string Description => commandDescription;
        public override string Usage => "<command>";

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(commandLabel))
                commandLabel = gameObject.name;
            if (string.IsNullOrWhiteSpace(commandDescription))
                commandDescription = "Executes pre-defined invokers";
        }

        public override void OnActionCommand(NewbieConsole console, string label, ref string[] vars,
            ref string[] envVars)
        {
            vars = vars.RemoveItem("-s", out var suppress);
            foreach (var invoker in invokers)
            {
                if (invoker == null) continue;
                invoker.Invoke();
            }

            if (suppress) return;
            console.Println($"Successfully invoked {Label}");
        }
    }
}