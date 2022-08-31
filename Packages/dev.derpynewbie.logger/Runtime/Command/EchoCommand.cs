using DerpyNewbie.Logger;
using UdonSharp;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EchoCommand : NewbieConsoleCommandHandler
    {
        public override string Label => "Echo";
        public override string Description => "Prints provided string into console.";
        public override string Usage => "<command> [messages...]";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            var msg = string.Join(" ", vars);
            console.Println(msg);
            return msg;
        }
    }
}