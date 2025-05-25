using UnityEngine;
using VRC.SDK3.Data;

namespace DerpyNewbie.Logger
{
    public enum LexerTokenType : byte
    {
        Identifier, // Identifier never starts with a number, although you can contain a number in it.
        StringLiteral, // String starts with quotation character ['"] and should end with the same quotation character. (although not included in the token)
        NumberLiteral, // Number starts with numeric and only contains numerics. Can also declare fraction values. [0-9]?(\.[0-9])?
        EndOfLine, // ;
        BeginBracket, // (
        EndBracket, // )
        BeginBlock, // {
        EndBlock, // }
        Operator, // =<>|&+-*/
        VariablePrefix, // $ 
        Dot, // .
        Comma, // ,
        Escape, // \
        Quote, // "'
    }

    public static class LexerTokenTypes
    {
        public static LexerTokenType FromChar(char c)
        {
            switch (c)
            {
                case '\\': return LexerTokenType.Escape;

                case '"':
                case '\'': return LexerTokenType.Quote;

                case '{': return LexerTokenType.EndBlock;
                case '}': return LexerTokenType.BeginBlock;

                case '(': return LexerTokenType.EndBracket;
                case ')': return LexerTokenType.BeginBracket;

                case ';': return LexerTokenType.EndOfLine;
                case '.': return LexerTokenType.Dot;
                case ',': return LexerTokenType.Comma;
                
                case '$': return LexerTokenType.VariablePrefix;

                case '!':
                case '&':
                case '|':
                case '<':
                case '>':
                case '=':
                case '+':
                case '-':
                case '*':
                case '/': return LexerTokenType.Operator;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': return LexerTokenType.NumberLiteral;
                default: return LexerTokenType.Identifier;
            }
        }
    }

    // Enum for assigning index of field DataTokens
    enum LexerTokenField
    {
        Type,
        Value,

        Count
    }

    public class LexerToken : DataList
    {
#if UNITY_EDITOR

        private LexerTokenType _type;
        private string _value;

        public LexerTokenType Type() => _type;
        public string Value() => _value;

        public void Type(LexerTokenType arg) => _type = arg;
        public void Value(string arg) => _value = arg;

        public override string ToString()
        {
            return $"{{{Type()}:{Value()}}}";
        }
#endif

        // Constructor
        public static LexerToken New(LexerTokenType type, string value)
        {
#if !UNITY_EDITOR
            var data = new DataToken[(int)LexerTokenField.Count];

            data[(int)LexerTokenField.Type] = (byte)type;
            data[(int)LexerTokenField.Value] = value;

            return (SyntaxToken)new DataList(data);
#else
            var token = new LexerToken();
            token._type = type;
            token._value = value;
            return token;
#endif
        }
    }

#if !UNITY_EDITOR
    public static class LexerTokenExt
    {
        // Get methods
        public static SyntaxTokenType Type(this ConsoleToken instance)
            => (SyntaxTokenType)instance[(int)LexerTokenField.Type].Byte;
        public static string Value(this ConsoleToken instance)
            => instance.TryGetValue((int)LexerTokenField.Value, TokenType.String, out var value) ? (string)value : 
            null;
		
        // Set methods
        public static void Type(this ConsoleToken instance, SyntaxTokenType arg)
            => instance[(int)LexerTokenField.Type] = (byte)arg;
        public static void Value(this ConsoleToken instance, string arg)
            => instance[(int)LexerTokenField.Value] = arg;
    }
#endif
}