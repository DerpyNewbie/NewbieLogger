using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PositionResetCommand : ActionCommandHandler
    {
        [SerializeField]
        private string commandLabel;
        [SerializeField]
        private string[] commandAliases;
        [SerializeField]
        private string commandDescription;

        [SerializeField]
        private bool onlyModerator = true;
        [SerializeField]
        private Transform[] transformsToReset;

        private Transform[] _initialParent;
        private Vector3[] _initialPosition;
        private Quaternion[] _initialRotation;

        private void Start()
        {
            if (string.IsNullOrEmpty(commandLabel))
                commandLabel = gameObject.name;
            if (string.IsNullOrEmpty(commandDescription))
                commandDescription = "Resets pre-defined objects transform";

            var len = transformsToReset.Length;
            _initialParent = new Transform[len];
            _initialPosition = new Vector3[len];
            _initialRotation = new Quaternion[len];

            for (var i = 0; i < len; i++)
            {
                var t = transformsToReset[i];
                if (t == null) continue;
                _initialParent[i] = t.parent;
                _initialPosition[i] = t.position;
                _initialRotation[i] = t.rotation;
            }
        }

        public override string Label => commandLabel;
        public override string[] Aliases => commandAliases;
        public override string Description => commandDescription;
        public override string Usage => "<command>";

        public override void OnActionCommand(NewbieConsole console, string label, ref string[] vars,
            ref string[] envVars)
        {
            if (onlyModerator && !console.IsSuperUser)
            {
                console.Println("Cannot access to this command unless you're moderator");
                return;
            }

            for (var i = 0; i < transformsToReset.Length; i++)
            {
                var t = transformsToReset[i];
                if (t == null) continue;
                Networking.SetOwner(Networking.LocalPlayer, t.gameObject);
                t.SetParent(_initialParent[i]);
                t.SetPositionAndRotation(_initialPosition[i], _initialRotation[i]);
            }

            console.Println($"{label}: Object positions has been reset.");
        }
    }
}