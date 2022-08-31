using System;
using UdonSharp;

namespace DerpyNewbie.Logger.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class OrCommand : NewbieConsoleCommandHandler
    {
        public override string Label => "Or";
        public override string Usage => "<command> <lhs> <rhs>";
        public override string Description => "Checks either <lhs> or <rhs> is 'true'.";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            if (vars.Length < 2)
            {
                console.Log("<color=red>Not enough arguments provided for OR command</color>");
                return ConsoleLiteral.GetNone();
            }

            return vars[0].Equals(ConsoleLiteral.GetTrue(), StringComparison.OrdinalIgnoreCase) ||
                   vars[1].Equals(ConsoleLiteral.GetTrue(), StringComparison.OrdinalIgnoreCase)
                ? ConsoleLiteral.GetTrue()
                : ConsoleLiteral.GetFalse();
        }
    }
}