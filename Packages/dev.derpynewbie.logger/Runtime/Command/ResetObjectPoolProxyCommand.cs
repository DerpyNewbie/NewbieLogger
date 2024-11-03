using DerpyNewbie.Common;
using DerpyNewbie.Common.ObjectPool;
using UdonSharp;
using UnityEngine;

namespace DerpyNewbie.Logger.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ResetObjectPoolProxyCommand : ActionCommandHandler
    {
        [SerializeField]
        private string commandLabel;

        [SerializeField]
        private string[] commandAliases;

        [SerializeField]
        private string commandDescription;

        [SerializeField]
        private ObjectPoolProxy[] targetProxies;

        [SerializeField]
        private bool requireModerator;

        public override string Label => commandLabel;
        public override string[] Aliases => commandAliases;
        public override string Description => commandDescription;
        public override string Usage => "<command>";

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(commandLabel)) commandLabel = gameObject.name;
            if (string.IsNullOrWhiteSpace(commandDescription)) commandDescription = "Resets ObjectPool Proxy objects.";
        }

        public override void OnActionCommand(NewbieConsole console, string label, ref string[] vars,
            ref string[] envVars)
        {
            vars = vars.RemoveItem("-s", out var suppress);

            foreach (var proxy in targetProxies)
            {
                proxy.ReturnAll();
            }

            if (suppress) return;
            console.Println("Successfully reset object pool proxies.");
        }
    }
}