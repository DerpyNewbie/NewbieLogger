using DerpyNewbie.Common;
using UdonSharp;
using UnityEngine;

namespace DerpyNewbie.Logger.Command
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DrawLineCommand : ActionCommandHandler
    {
        [SerializeField] [HideInInspector] [NewbieInject]
        private LineManager lineManager;

        public override string Label => "DrawLine";
        public override string[] Aliases => new[] { "DebugLine", "DL" };
        public override string Description => "Draws a line. Useful for debugging.";
        // Zero-width space is added for workaround. Unity interprets "<.>"(regex) as rich-text tag for some reason.
        // and then fucking explodes whole rich text by thinking it didnt close properly
        public override string Usage =>
            "<command> draw <​x> <​y> <​z> <bx> <by> <bz> [color=white] [time=5] [startWidth=0.01] [endWidth=0.01] OR <command> <list|clear>";

        public override void OnActionCommand(NewbieConsole console, string label, ref string[] vars,
            ref string[] envVars)
        {
            vars = vars.RemoveItem("-s", out var suppress);

            if (vars.Length <= 0)
            {
                console.PrintUsage(this);
                return;
            }

            switch (vars[0].ToLower())
            {
                case "draw":
                    DrawSubCommand(console, label, ref vars, ref envVars, suppress);
                    break;
                case "list":
                    console.Println("List of all lines drawn:");
                    var lines = lineManager.GetAllLines();
                    foreach (var line in lines)
                    {
                        if (line == null)
                        {
                            console.Println("null");
                            continue;
                        }

                        var positions = line.GetPositions();
                        console.Println(
                            $"{line.transform.hierarchyCount}:{(positions.Length != 0 ? positions[0].ToString("F2") : null)}:{(positions.Length != 0 ? positions[positions.Length - 1].ToString("F2") : null)}");
                    }

                    console.Println($"<color=green>{lines.Length}</color> Line(s) are drawn.");
                    break;
                case "clear":
                    var count = lineManager.Clear();
                    console.Println($"Successfully cleared <color=green>{count}</color> lines.");
                    break;
                default:
                    console.PrintUsage(this);
                    break;
            }
        }

        private void DrawSubCommand(NewbieConsole console, string label, ref string[] vars, ref string[] envVars,
            bool suppress)
        {
            if (vars.Length < 6)
            {
                console.Println("<color=red>Not enough arguments are provided.</color>");
                return;
            }

            Gradient gradient = null;
            float time = 5F, startWidth = 0.01F, endWidth = 0.01F;

            var color = Color.white;

            var x = ConsoleParser.TryParseFloat(vars[1]);
            var y = ConsoleParser.TryParseFloat(vars[2]);
            var z = ConsoleParser.TryParseFloat(vars[3]);
            var bx = ConsoleParser.TryParseFloat(vars[4]);
            var by = ConsoleParser.TryParseFloat(vars[5]);
            var bz = ConsoleParser.TryParseFloat(vars[6]);

            if (7 < vars.Length)
            {
                color = NewbieUtils.GetColorFromAlias(vars[7]);
                gradient = new Gradient();
                gradient.SetKeys(new[] { new GradientColorKey(color, 0F), new GradientColorKey(color, 1F) },
                    new[] { new GradientAlphaKey(1F, 0F), new GradientAlphaKey(1F, 0F) });
            }

            if (8 < vars.Length)
                time = ConsoleParser.TryParseFloat(vars[8]);
            if (9 < vars.Length)
                startWidth = ConsoleParser.TryParseFloat(vars[9]);
            if (10 < vars.Length)
                endWidth = ConsoleParser.TryParseFloat(vars[10]);

            var v1 = new Vector3(x, y, z);
            var v2 = new Vector3(bx, by, bz);
            lineManager.Draw(v1, v2, time, startWidth, endWidth, gradient);
            if (!suppress)
                console.Println(
                    $"Successfully printed line on {v1.ToString("F2")} to {v2.ToString("F2")}{(color != Color.white ? $" with <color=#{NewbieUtils.ToHtmlStringRGBA(color)}>THIS COLOR!</color>." : ".")}");
        }


    }
}