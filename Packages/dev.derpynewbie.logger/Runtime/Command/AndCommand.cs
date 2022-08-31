using System;
using UdonSharp;

namespace DerpyNewbie.Logger.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AndCommand : NewbieConsoleCommandHandler
    {
        public override string Label => "And";
        public override string[] Aliases => new[] { "Is" };
        public override string Usage => "<command> <lhs> <rhs>";
        public override string Description => "Compares <lhs> to <rhs> are equal.";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            if (vars.Length < 2)
            {
                console.Log("<color=red>Not enough arguments provided for AND command</color>");
                return ConsoleLiteral.GetNone();
            }

            return vars[0].Equals(vars[1], StringComparison.OrdinalIgnoreCase)
                ? ConsoleLiteral.GetTrue()
                : ConsoleLiteral.GetFalse();
        }
    }
}