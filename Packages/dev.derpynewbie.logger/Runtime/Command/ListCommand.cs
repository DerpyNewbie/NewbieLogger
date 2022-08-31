using DerpyNewbie.Logger;
using UdonSharp;
using VRC.SDKBase;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ListCommand : NewbieConsoleCommandHandler
    {
        private const string PlayerFormat = "{0, -20}, {1, 3}, {2, 7}, {3, 7}";

        public override string Label => "List";
        public override string Description => "Lists up VRCPlayerApis currently in world.";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            var msg = GetPlayerList();
            console.Println(msg);
            return msg;
        }

        private static string GetPlayerList()
        {
            var players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
            var result = string.Format(PlayerFormat, "Name", "Id", "Master", "Local");
            foreach (var player in players)
                result = string.Join
                (
                    "\n",
                    result,
                    string.Format
                    (
                        PlayerFormat,
                        player.displayName,
                        player.playerId,
                        player.isMaster,
                        player.isLocal
                    )
                );

            return result + $"\nThere is currently <color=green>{players.Length}</color> players in the world";
        }
    }
}