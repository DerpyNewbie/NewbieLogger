using UdonSharp;
using UnityEngine;

namespace DerpyNewbie.Logger.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RandomCommand : FloatCommandHandler
    {
        public override string Label => "Random";
        public override string[] Aliases => new[] { "Rnd" };
        public override string Usage => "<command> or <command> <min=0> <max=1>";
        public override string Description => "Returns random float value between 0 to 1 or specified values.";

        public override float OnFloatCommand(NewbieConsole console, string label, ref string[] vars,
            ref string[] envVars)
        {
            return vars.Length < 2
                ? Random.value
                : Random.Range(ConsoleParser.TryParseFloat(vars[0]), ConsoleParser.TryParseFloat(vars[1]));
        }
    }
}