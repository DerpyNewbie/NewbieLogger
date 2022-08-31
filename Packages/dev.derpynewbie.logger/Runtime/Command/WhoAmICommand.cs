using DerpyNewbie.Common;
using DerpyNewbie.Logger;
using UdonSharp;
using VRC.SDKBase;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhoAmICommand : ActionCommandHandler
    {
        public override string Label => "WhoAmI";
        public override string Description => "Prints out LocalPlayer's information.";
        public override string Usage => "<command>";

        public override void OnActionCommand(NewbieConsole console, string label,
            ref string[] vars, ref string[] envVars)
        {
            console.Println($"{NewbieUtils.GetPlayerName(Networking.LocalPlayer)}");
            console.Println($"Is Master: {Networking.IsMaster}\n" +
                            $"Is Instance Owner: {Networking.IsInstanceOwner}\n" +
                            $"Is Clogged: {Networking.IsClogged}\n" +
                            $"Is Settled: {Networking.IsNetworkSettled}\n");
        }
    }
}