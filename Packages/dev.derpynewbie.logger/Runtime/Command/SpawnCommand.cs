using System;
using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SpawnCommand : ActionCommandHandler
    {
        [SerializeField]
        private Transform defaultSpawn;
        [SerializeField]
        private Transform[] preDefinedSpawns;

        private Vector3 _currentSpawnPos;
        private Quaternion _currentSpawnRot;
        private DateTime _lastRespawnedTime;

        public override string Label => "Spawn";
        public override string[] Aliases => new[] { "Respawn" };

        public override string Description => "Sets respawn point at current position or pre-defined position";
        public override string Usage => "<command> [here|reset] or <command> <preset> <index>";

        public override void OnActionCommand(NewbieConsole console, string label, ref string[] vars,
            ref string[] envVars)
        {
            if (vars.Length < 1)
            {
                console.Println($"Current Spawn Position: {_currentSpawnPos.ToString("F2")}");
                return;
            }

            switch (vars[0].ToLower())
            {
                case "here":
                {
                    _currentSpawnPos = Networking.LocalPlayer.GetPosition();
                    _currentSpawnRot = Networking.LocalPlayer.GetRotation();
                    console.Println($"Successfully set new respawn position: {_currentSpawnPos.ToString("F2")}");
                    return;
                }
                case "reset":
                {
                    _currentSpawnPos = defaultSpawn.position;
                    _currentSpawnRot = defaultSpawn.rotation;
                    console.Println("Successfully reset spawn position");
                    return;
                }
                case "preset":
                {
                    if (vars.Length < 2)
                    {
                        console.Println("Not enough arguments provided for preset subcommand.");
                        return;
                    }

                    var presetIndex = ConsoleParser.TryParseInt(vars[1]);
                    if (0 > presetIndex || preDefinedSpawns.Length <= presetIndex)
                    {
                        console.Println("Preset index was out of bounds.");
                        return;
                    }

                    var spawn = preDefinedSpawns[presetIndex];
                    _currentSpawnPos = spawn.position;
                    _currentSpawnRot = spawn.rotation;
                    console.Println($"Successfully set spawn position to preset: {presetIndex}");
                    return;
                }
            }
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                if (DateTime.Now.Subtract(_lastRespawnedTime).TotalSeconds < 5)
                {
                    _currentSpawnPos = defaultSpawn.position;
                    _currentSpawnRot = defaultSpawn.rotation;
                }

                player.TeleportTo
                (
                    _currentSpawnPos,
                    _currentSpawnRot,
                    VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint,
                    false
                );

                _lastRespawnedTime = DateTime.Now;
            }
        }
    }
}