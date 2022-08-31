using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ShowCommand : BoolCommandHandler
    {
        [SerializeField]
        private string commandLabel;
        [SerializeField]
        private string[] commandAliases;
        [SerializeField]
        private string commandDescription;

        [SerializeField]
        private GameObject[] objectsToActivate;
        [SerializeField]
        private GameObject[] objectsToDeactivate;
        [SerializeField]
        private Canvas[] canvasesToActivate;
        [SerializeField]
        private Canvas[] canvasesToDeactivate;
        [SerializeField]
        private VRCPickup[] pickupsToActivate;
        [SerializeField]
        private VRCPickup[] pickupsToDeactivate;
        [SerializeField]
        private bool isGlobal;
        [SerializeField] [UdonSynced] [FieldChangeCallback(nameof(IsOn))]
        private bool isOn;
        private bool IsOn
        {
            get => isOn;
            set
            {
                isOn = value;
                foreach (var o in objectsToActivate)
                {
                    if (o == null) continue;
                    o.SetActive(value);
                }

                foreach (var o in objectsToDeactivate)
                {
                    if (o == null) continue;
                    o.SetActive(!value);
                }

                foreach (var c in canvasesToActivate)
                {
                    if (c == null) continue;
                    c.enabled = value;
                }

                foreach (var c in canvasesToDeactivate)
                {
                    if (c == null) continue;
                    c.enabled = !value;
                }

                foreach (var p in pickupsToActivate)
                {
                    if (p == null) continue;
                    p.pickupable = value;
                }

                foreach (var p in pickupsToDeactivate)
                {
                    if (p == null) continue;
                    p.pickupable = !value;
                }
            }
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(commandLabel))
                commandLabel = gameObject.name;
            if (string.IsNullOrEmpty(commandDescription))
                commandDescription = "Toggles pre-assigned objects";
        }

        public override string Label => commandLabel;
        public override string[] Aliases => commandAliases;
        public override string Description => commandDescription;
        public override string Usage => "<command> [true|false]";

        public override bool OnBoolCommand(NewbieConsole console, string label, ref string[] vars, ref string[] envVars)
        {
            if (vars != null && vars.Length >= 1)
            {
                IsOn = ConsoleParser.TryParseBoolean(vars[0], IsOn);
                if (isGlobal)
                {
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                    RequestSerialization();
                }
            }

            console.Println($"{label}: {IsOn}");
            return IsOn;
        }
    }
}