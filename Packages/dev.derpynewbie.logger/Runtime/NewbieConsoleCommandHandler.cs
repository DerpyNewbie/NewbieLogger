using UdonSharp;

namespace DerpyNewbie.Logger
{
    public abstract class NewbieConsoleCommandHandler : UdonSharpBehaviour
    {
        public abstract string Label { get; }
        public virtual string[] Aliases { get; } = NewbieConsole.EmptyAlias();
        public virtual string Description { get; } = "No description provided.";
        public virtual string Usage { get; } = "No usage provided.";

        /// <summary>
        /// Called when this command is registered on <see cref="NewbieConsole"/> instance.
        /// </summary>
        /// <param name="console"></param>
        public virtual void OnRegistered(NewbieConsole console)
        {
        }

        /// <summary>
        /// Called when this command is unregistered from <see cref="NewbieConsole"/> instance.
        /// </summary>
        /// <param name="console"></param>
        public virtual void OnUnregistered(NewbieConsole console)
        {
        }

        /// <summary>
        /// Invokes when certain command label was entered on <see cref="NewbieConsole.In"/>.
        /// </summary>
        /// <param name="console">a console object which invoked this command.</param>
        /// <param name="label">a label which invoked this command, it could be <see cref="Label"/>, <see cref="Aliases"/> or something else.</param>
        /// <param name="vars">a variables, or an arguments, which was appended with <paramref name="label"/>.</param>
        /// <param name="envVars">an environment variables, stored on a <paramref name="console"/> instance, and will be provided across all other command calls.</param>
        /// <returns>result of a operation. could be an empty string, </returns>
        public abstract string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars);
    }

    public abstract class BoolCommandHandler : NewbieConsoleCommandHandler
    {
        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            return ConsoleLiteral.Of(OnBoolCommand(console, label, ref vars, ref envVars));
        }

        public abstract bool OnBoolCommand(NewbieConsole console, string label,
            ref string[] vars, ref string[] envVars);
    }

    public abstract class IntCommandHandler : NewbieConsoleCommandHandler
    {
        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            return ConsoleLiteral.Of(OnIntCommand(console, label, ref vars, ref envVars));
        }

        public abstract int OnIntCommand(NewbieConsole console, string label, ref string[] vars, ref string[] envVars);
    }

    public abstract class FloatCommandHandler : NewbieConsoleCommandHandler
    {
        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            return ConsoleLiteral.Of(OnFloatCommand(console, label, ref vars, ref envVars));
        }

        public abstract float OnFloatCommand(NewbieConsole console, string label,
            ref string[] vars, ref string[] envVars);
    }

    public abstract class ActionCommandHandler : NewbieConsoleCommandHandler
    {
        public override string OnCommand(NewbieConsole console, string label, string[] vars, ref string[] envVars)
        {
            OnActionCommand(console, label, ref vars, ref envVars);
            return ConsoleLiteral.GetNone();
        }

        public abstract void OnActionCommand(NewbieConsole console, string label,
            ref string[] vars, ref string[] envVars);
    }
}