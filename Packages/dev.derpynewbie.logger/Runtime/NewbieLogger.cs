using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace DerpyNewbie.Logger
{
    [DefaultExecutionOrder(-10000000), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NewbieLogger : PrintableBase
    {
        [SerializeField]
        private Text loggingText;
        private int _maxChars = 7000;
        private bool _isLoggingTextNotNull;
        private string _cachedLoggingString = "";

        private void Start()
        {
            _isLoggingTextNotNull = loggingText != null;
            this.LogInternal("Logger::Start complete");
        }

        public override int MaxChars
        {
            get => _maxChars;
            set
            {
                _maxChars = value;
                UpdateText();
            }
        }
        public override int LogLevel { get; set; }

        public override void Print(string text)
        {
            _cachedLoggingString += text;
            UpdateText();
        }

        public override void Clear()
        {
            _cachedLoggingString = "";
            UpdateText();
            this.LogInternal("Logger::Cleared text");
        }

        public override void ClearLine()
        {
            var clearIndex = _cachedLoggingString.LastIndexOf('\n');
            _cachedLoggingString = clearIndex == -1 ? "" : _cachedLoggingString.Substring(0, clearIndex);
            UpdateText();
            this.LogInternal("Logger::Cleared line");
        }

        private void UpdateText()
        {
            _cachedLoggingString = ClipStringUntil(_cachedLoggingString, MaxChars);

            if (!_isLoggingTextNotNull) return;
            loggingText.text = _cachedLoggingString;
        }

        private static string ClipStringUntil(string text, int max)
        {
            const int prefixColorCharsLength = 25;
            var chars = text.ToCharArray();
            var targetClipPoint = chars.Length - max;
            var lastNewLineAt = 0;
            // TODO: check by reverse for more optimal search

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '\n')
                {
                    lastNewLineAt = i;
                    targetClipPoint += prefixColorCharsLength;
                }

                if (i > targetClipPoint)
                    break;
            }

            return text.Substring(lastNewLineAt);
        }
    }
}