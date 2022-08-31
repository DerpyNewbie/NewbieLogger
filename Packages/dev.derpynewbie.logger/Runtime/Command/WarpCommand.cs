using System;
using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class WarpCommand : ActionCommandHandler
{
    [SerializeField]
    private Transform[] warpPositions;
    [SerializeField]
    private string[] warpNames;

    private Vector3 _beforeWarpPosition;

    public override string Label => "Warp";
    public override string Description => "Warps local player to new position";
    public override string Usage => "<command> <destination|back>";

    public override void OnActionCommand(NewbieConsole console, string label, ref string[] vars, ref string[] envVars)
    {
        if (vars.Length == 0)
        {
            console.Println($"Available destinations:\n{string.Join("\n", warpNames)}");
            return;
        }

        if (vars[0].Equals("back", StringComparison.OrdinalIgnoreCase))
        {
            console.Println("Successfully warped back to original position");
            Networking.LocalPlayer.TeleportTo(_beforeWarpPosition, Networking.LocalPlayer.GetRotation(),
                VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
            return;
        }

        for (int i = 0; i < warpNames.Length; i++)
        {
            if (warpNames[i].Equals(vars[0], StringComparison.OrdinalIgnoreCase))
            {
                _beforeWarpPosition = Networking.LocalPlayer.GetPosition();
                Networking.LocalPlayer.TeleportTo(warpPositions[i].position, Networking.LocalPlayer.GetRotation(),
                    VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
                console.Println($"Successfully warped to '{warpNames[i]}'");
                return;
            }
        }
        
        console.Println($"<color=red>Destination with name '{vars[0]}' not found.</color>");
    }
}