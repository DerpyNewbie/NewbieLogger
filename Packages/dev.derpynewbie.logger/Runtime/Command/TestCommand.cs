using DerpyNewbie.Logger;
using UdonSharp;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TestCommand : ActionCommandHandler
    {
        public override string Label => "Test";
        public override string Description => "Prints out provided label, vars, envVars for debugging.";
        public override string Usage => "<command> [args]";

        public override void OnActionCommand(NewbieConsole console, string label,
            ref string[] vars, ref string[] envVars)
        {
            console.Println($"label: {label}\n" +
                            $"vars   : {(vars != null ? string.Join(", ", vars) : "__args null__")}\n" +
                            $"envVars: {(envVars != null ? string.Join(", ", envVars) : "__envVars null__")}");
        }
    }
}