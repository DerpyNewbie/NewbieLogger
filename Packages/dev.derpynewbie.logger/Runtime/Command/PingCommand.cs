using System;
using DerpyNewbie.Logger;
using UdonSharp;
using VRC.SDKBase;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PingCommand : NewbieConsoleCommandHandler
    {
        public override string Label => "Ping";
        public override string Description => "Shows network delay taken from `Networking.GetNetworkDateTime()`.";
        public override string Usage => "<command>";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            if (vars != null && vars.Length >= 1)
            {
                console.Println(vars[0] == "null" ? "ガッ!" : "Pong!");
                return ConsoleLiteral.GetNone();
            }

            var ping = DateTime.UtcNow.Subtract(Networking.GetNetworkDateTime()).TotalMilliseconds;
            var msg = $"{ping:F0} ms";
            console.Println(msg);
            return $"{ping:F0}";
        }
    }
}