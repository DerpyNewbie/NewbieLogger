using DerpyNewbie.Common;
using UdonSharp;
using VRC.SDKBase;

namespace DerpyNewbie.Logger.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DisplayNameCommand : NewbieConsoleCommandHandler
    {
        public override string Label => "DisplayName";
        public override string[] Aliases => new[] { "Me" };
        public override string Description => "Returns local player's display name.";
        public override string Usage => "<command> [-s]";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            var displayName = Networking.LocalPlayer.displayName;
            if (!vars.ContainsItem("-s"))
                console.Println(displayName);
            return displayName;
        }
    }
}