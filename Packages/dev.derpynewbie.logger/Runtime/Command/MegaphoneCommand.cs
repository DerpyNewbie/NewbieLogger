using DerpyNewbie.Common;
using DerpyNewbie.Logger;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace DerpyNewbie.Shooter.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MegaphoneCommand : NewbieConsoleCommandHandler
    {
        [SerializeField]
        private Megaphone[] megaphones;

        private static string GetMegaphoneList(Megaphone[] megaphones)
        {
            const string format = "{0, -20}, {1, 8}, {2, 8}";
            var result = "Available Megaphones:";
            var held = 0;

            result = string.Join("\n", result, string.Format(format, "Name", "Is Held", "Is Toggle"));

            foreach (var megaphone in megaphones)
            {
                result = string.Join
                (
                    "\n",
                    result,
                    string.Format
                    (
                        format,
                        megaphone.name,
                        megaphone.IsHeld,
                        megaphone.enableToggleForVoiceAmplifier
                    )
                );

                if (megaphone.IsHeld)
                    ++held;
            }

            return result +
                   $"\nThere is <color=green>{megaphones.Length}</color> megaphones, " +
                   $"<color=green>{held}</color> instances held, " +
                   $"<color=green>{megaphones.Length - held}</color> instances stored";
        }

        public override string Label => "Megaphone";

        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            if (vars == null || vars.Length == 0) return console.PrintUsage(this);
            switch (vars[0].ToLower())
            {
                case "r":
                case "reset":
                    foreach (var megaphone in megaphones)
                    {
                        Networking.SetOwner(Networking.LocalPlayer, megaphone.gameObject);
                        megaphone.ResetPosition();
                    }

                    console.Println("<color=green>Megaphone reset complete</color>");
                    return ConsoleLiteral.GetNone();
                case "l":
                case "list":
                    var msg = GetMegaphoneList(megaphones);
                    console.Println(msg);
                    return msg;
                case "s":
                case "spawn":
                case "summon":
                    foreach (var megaphone in megaphones)
                    {
                        if (megaphone.IsHeld) continue;

                        Networking.SetOwner(Networking.LocalPlayer, megaphone.gameObject);

                        var rot = Networking.LocalPlayer.GetRotation();
                        var pos = Networking.LocalPlayer.GetPosition() + Vector3.up + rot * Vector3.forward;

                        megaphone.MoveTo(pos, rot);
                        console.Println(
                            $"<color=green>Successfully summoned megaphone {megaphone.name} in front of you!</color>");
                        return ConsoleLiteral.GetTrue();
                    }

                    console.Println(
                        "<color=red>Failed to summon megaphone: There is no megaphones currently available</color>");
                    return ConsoleLiteral.GetFalse();
                default:
                    return console.PrintUsage(this);
            }
        }
    }
}