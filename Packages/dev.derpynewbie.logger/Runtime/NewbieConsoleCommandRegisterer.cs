using DerpyNewbie.Common;
using UdonSharp;
using UnityEngine;

namespace DerpyNewbie.Logger
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NewbieConsoleCommandRegisterer : UdonSharpBehaviour
    {
        [NewbieInject]
        [SerializeField]
        [HideInInspector]
        private NewbieConsole console;

        [SerializeField]
        private NewbieConsoleCommandHandler[] registeringCommandHandler;

        [SerializeField]
        private bool registerChildOfThisObject = true;

        private void Start()
        {
            Register();
        }

        public void Register()
        {
            foreach (var handler in registeringCommandHandler)
                console.RegisterHandler(handler);

            if (!registerChildOfThisObject) return;

            foreach (var handler in GetComponentsInChildren<NewbieConsoleCommandHandler>())
                console.RegisterHandler(handler);
        }

        public void Unregister()
        {
            foreach (var handler in registeringCommandHandler)
                console.UnregisterHandler(handler);

            if (!registerChildOfThisObject) return;

            foreach (var handler in GetComponentsInChildren<NewbieConsoleCommandHandler>())
                console.UnregisterHandler(handler);
        }
    }
}