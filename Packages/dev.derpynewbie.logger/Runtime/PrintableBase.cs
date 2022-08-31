using UdonSharp;

namespace DerpyNewbie.Logger
{
    public abstract class PrintableBase : UdonSharpBehaviour
    {
        public abstract int LogLevel { get; set; }
        public abstract void Print(string message);
        public abstract void Clear();
        public abstract void ClearLine();
    }
}