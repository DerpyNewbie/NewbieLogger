using VRC.SDK3.Data;

namespace DerpyNewbie.Logger
{
    public enum LexerResult
    {
        Success = 0,

        CLexerError = 1,
        LexerNotImplemented = 2,
        LexerNotSupportedSyntaxType = 3,

        CSyntaxError = 500,
        NumberCannotHaveTwoDecimalPoints = 501,
        ExpectedSymbolFoundEof = 1002,
        ExpectedSymbolFoundEmpty = 1003,
        ExpectedQuoteFoundEof = 1004,
        ExpectedEscapedCharFoundInvalidChar = 1005,
        ExpectedEqualOpFoundInvalidOp = 1006,
    }

    public class LexerResults
    {
        public static string GetHumanFriendlyReason(LexerResult resultNum)
        {
            switch (resultNum)
            {
                case LexerResult.Success:
                    return "Success";
                case LexerResult.LexerNotImplemented:
                    return "Lexer error, Not implemented.";
                case LexerResult.LexerNotSupportedSyntaxType:
                    return "Lexer error, Found non-supported SyntaxToken type.";
                case LexerResult.NumberCannotHaveTwoDecimalPoints:
                    return "Number cannot contain more than one decimal points.";
                case LexerResult.ExpectedSymbolFoundEmpty:
                    return "Expected Symbol Name, Found Empty.";
                case LexerResult.ExpectedSymbolFoundEof:
                    return "Expected Symbol Name, Found EOF.";
                case LexerResult.ExpectedQuoteFoundEof:
                    return "Expected Quote ending, Found EOF.";
                case LexerResult.ExpectedEscapedCharFoundInvalidChar:
                    return "Expected Escaped character, Found invalid character.";
                case LexerResult.ExpectedEqualOpFoundInvalidOp:
                    return "Expected Equal Operator, Found invalid operator.";
                default:
                    return "Unknown";
            }
        }
    }

    public static class Lexer
    {
        public static bool Tokenize(string input, out DataList tokens,
            out LexerResult resultType, out int errorIndex)
        {
            tokens = new DataList();
            for (var i = 0; i < input.Length; i++)
            {
                var currChar = input[i];
                if (currChar == ' ')
                    continue;
                var currToken = LexerTokenTypes.FromChar(currChar);

                switch (currToken)
                {
                    case LexerTokenType.Quote:
                    {
                        if (!RetrieveString(input, ref i, out var str, out resultType, out errorIndex))
                            return false;

                        tokens.Add(LexerToken.New(LexerTokenType.StringLiteral, str));
                        continue;
                    }
                    case LexerTokenType.Identifier:
                    {
                        if (!RetrieveIdentifier(input, ref i, out var literal, out resultType, out errorIndex))
                            return false;

                        tokens.Add(LexerToken.New(LexerTokenType.Identifier, literal));
                        continue;
                    }
                    case LexerTokenType.NumberLiteral:
                    {
                        if (!RetrieveNumber(input, ref i, out var number, out resultType, out errorIndex))
                            return false;

                        tokens.Add(LexerToken.New(LexerTokenType.NumberLiteral, number));
                        continue;
                    }
                    case LexerTokenType.Operator:
                    {
                        if (!RetrieveOperator(input, ref i, out var op, out resultType, out errorIndex))
                            return false;

                        tokens.Add(LexerToken.New(LexerTokenType.Operator, op));
                        continue;
                    }
                    case LexerTokenType.VariablePrefix:
                    case LexerTokenType.Dot:
                    case LexerTokenType.Comma:
                    case LexerTokenType.BeginBracket:
                    case LexerTokenType.EndBracket:
                    case LexerTokenType.BeginBlock:
                    case LexerTokenType.EndBlock:
                    case LexerTokenType.EndOfLine:
                    {
                        tokens.Add(LexerToken.New(currToken, currChar.ToString()));
                        continue;
                    }
                    default:
                    {
                        resultType = LexerResult.LexerNotImplemented;
                        errorIndex = i;
                        return false;
                    }
                }
            }

            resultType = LexerResult.Success;
            errorIndex = 0;
            return true;
        }

        private static bool RetrieveIdentifier(string input, ref int index,
            out string literal, out LexerResult resultNum, out int errorIndex)
        {
            literal = "";
            var hasDone = false;
            for (; index < input.Length && !hasDone; index++)
            {
                var c = input[index];
                if (c == ' ')
                {
                    hasDone = true;
                    continue;
                }

                switch (LexerTokenTypes.FromChar(c))
                {
                    case LexerTokenType.Escape:
                    case LexerTokenType.Quote:
                    case LexerTokenType.EndOfLine:
                    case LexerTokenType.BeginBlock:
                    case LexerTokenType.EndBlock:
                    case LexerTokenType.BeginBracket:
                    case LexerTokenType.EndBracket:
                    case LexerTokenType.Dot:
                    case LexerTokenType.Operator:
                    {
                        --index;
                        hasDone = true;
                        continue;
                    }
                    case LexerTokenType.NumberLiteral:
                    case LexerTokenType.StringLiteral:
                    case LexerTokenType.Identifier:
                    {
                        literal += c;
                        continue;
                    }
                    default:
                    {
                        literal = "";
                        resultNum = LexerResult.LexerNotSupportedSyntaxType;
                        errorIndex = index;
                        return false;
                    }
                }
            }

            resultNum = LexerResult.Success;
            errorIndex = 0;
            --index;
            return true;
        }

        private static bool RetrieveString(string input, ref int index,
            out string str, out LexerResult resultNum, out int errorIndex)
        {
            str = "";
            var isEscaping = false;
            var inQuote = false;
            var quoteType = '"';
            for (; index < input.Length; index++)
            {
                var c = input[index];
                if (isEscaping == false)
                {
                    var t = LexerTokenTypes.FromChar(c);
                    if (t == LexerTokenType.Quote)
                    {
                        if (!inQuote)
                        {
                            inQuote = true;
                            quoteType = c;
                            continue;
                        }

                        if (quoteType == c)
                        {
                            inQuote = false;
                            break;
                        }
                    }

                    if (t == LexerTokenType.Escape)
                    {
                        isEscaping = true;
                        continue;
                    }
                }

                if (isEscaping)
                {
                    isEscaping = false;
                    switch (c)
                    {
                        case 'r':
                            str += '\r';
                            continue;
                        case 'n':
                            str += '\n';
                            continue;
                        case 't':
                            str += '\t';
                            continue;
                        case '\\':
                            str += '\\';
                            continue;
                        case '"':
                            str += '"';
                            continue;
                        case '\'':
                            str += '\'';
                            continue;
                    }

                    errorIndex = index;
                    resultNum = LexerResult.ExpectedEscapedCharFoundInvalidChar;
                    return false;
                }

                str += c;
            }

            if (inQuote)
            {
                errorIndex = index;
                resultNum = LexerResult.ExpectedQuoteFoundEof;
                return false;
            }

            resultNum = LexerResult.Success;
            errorIndex = 0;
            return true;
        }

        private static bool RetrieveNumber(string input, ref int index,
            out string number, out LexerResult resultNum, out int errorIndex)
        {
            number = "";
            var hasDecimal = false;
            for (; index < input.Length; index++)
            {
                var c = input[index];
                var type = LexerTokenTypes.FromChar(c);

                if (type != LexerTokenType.NumberLiteral && type != LexerTokenType.Dot)
                {
                    break;
                }

                if (type == LexerTokenType.Dot)
                {
                    if (hasDecimal)
                    {
                        resultNum = LexerResult.NumberCannotHaveTwoDecimalPoints;
                        errorIndex = index;
                        return false;
                    }

                    hasDecimal = true;
                }

                number += c;
            }

            resultNum = LexerResult.Success;
            errorIndex = 0;
            --index;
            return true;
        }

        private static bool RetrieveOperator(string input, ref int index,
            out string op, out LexerResult resultNum, out int errorIndex)
        {
            op = input[index++].ToString();
            if (index < input.Length)
            {
                var c = input[index];
                if (LexerTokenTypes.FromChar(c) == LexerTokenType.Operator)
                {
                    if (op == "=" && c != '=')
                    {
                        resultNum = LexerResult.ExpectedEqualOpFoundInvalidOp;
                        errorIndex = index;
                        return false;
                    }

                    op += c;
                }
            }

            resultNum = LexerResult.Success;
            errorIndex = 0;
            return true;
        }
    }
}