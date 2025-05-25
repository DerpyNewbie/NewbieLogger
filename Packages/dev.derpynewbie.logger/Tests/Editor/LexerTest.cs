using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace DerpyNewbie.Logger.Tests.Editor
{
    public class LexerTest
    {
        private static void Check(string input, params LexerTokenType[] typeResults)
        {
            Debug.Log($"Check: \"{input}\" with types: {string.Join(", ", typeResults)} ");
            var tokenizeResult = Lexer.Tokenize(input, out var tokens, out var error, out var errorIndex);
            Debug.Log(
                $"  result: {tokenizeResult}, {(int)error}@{errorIndex}: {LexerResults.GetHumanFriendlyReason(error)}");
            Assert.IsTrue(tokenizeResult);
            Assert.IsNotEmpty(tokens);
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = (LexerToken)tokens[i].DataList;
                Assert.NotNull(token);
                Debug.Log($"  {i}: {token}");

                Assert.AreEqual(typeResults[i], token.Type());
            }
        }

        private static void CheckError(string input, LexerResult expectedResult, int expectedErrorIndex)
        {
            Debug.Log(
                $"CheckError: \"{input}\" with error: {expectedResult}@{expectedErrorIndex}: {LexerResults.GetHumanFriendlyReason(expectedResult)}");
            var tokenizeResult =
                Lexer.Tokenize(input, out var tokens, out var errorNum, out var errorIndex);

            Debug.Log($"  {errorNum}@{errorIndex}: {LexerResults.GetHumanFriendlyReason(errorNum)}");
            Debug.Log($"  {string.Join(", ", tokens.Select(t => ((LexerToken)t.DataList).ToString()))}");

            Assert.IsFalse(tokenizeResult);
            Assert.AreEqual(expectedResult, errorNum);
            Assert.AreEqual(expectedErrorIndex, errorIndex);
        }
        
        [Test]
        public void SimpleTokenizerTest()
        {
            Check("abcdefg", LexerTokenType.Identifier);
            Check("230", LexerTokenType.NumberLiteral);
            Check("\"abs;cedf\"", LexerTokenType.StringLiteral);
            Check("\"aaa'aaa\"", LexerTokenType.StringLiteral);
            Check("'aa\"aa'", LexerTokenType.StringLiteral);
            Check("'aaa\\'aaa'", LexerTokenType.StringLiteral);
            Check("$symbol", LexerTokenType.Identifier);

            // Check operators
            Check("+", LexerTokenType.Operator);
            Check("==", LexerTokenType.Operator);
            Check("===", LexerTokenType.Operator, LexerTokenType.Operator);
            Check("+=", LexerTokenType.Operator);

            // Check unique
            Check(".", LexerTokenType.Dot);
            Check(",", LexerTokenType.Comma);
            Check("{", LexerTokenType.BeginBlock);
            Check("}", LexerTokenType.BeginBlock);
            Check("(", LexerTokenType.BeginBracket);
            Check(")", LexerTokenType.BeginBracket);
            Check(";", LexerTokenType.EndOfLine);

            // Check combinations
            Check("230.0 ghcjed", LexerTokenType.NumberLiteral, LexerTokenType.Identifier);
            Check("\"adadada\"01234.00what", LexerTokenType.StringLiteral, LexerTokenType.NumberLiteral, LexerTokenType.Identifier);
            Check("a+b", LexerTokenType.Identifier, LexerTokenType.Operator, LexerTokenType.Identifier);

            CheckError("'aaa", LexerResult.ExpectedQuoteFoundEof, 4);
            CheckError("0.0.0", LexerResult.NumberCannotHaveTwoDecimalPoints, 3);
            CheckError("what1.2.3.4", LexerResult.NumberCannotHaveTwoDecimalPoints, 9);
        }

        [Test]
        public void SimpleHelloWorldTest()
        {
            // Check a simple hello world
            const string helloWorldScript = "var h = \"Hello\", w = \"\\\"World!\\\"\"; echo(h + ' ' + w);";
            Check(helloWorldScript,
                LexerTokenType.Identifier,
                LexerTokenType.Identifier, LexerTokenType.Operator, LexerTokenType.StringLiteral,
                LexerTokenType.Comma,
                LexerTokenType.Identifier, LexerTokenType.Operator, LexerTokenType.StringLiteral,
                LexerTokenType.EndOfLine,
                LexerTokenType.Identifier, LexerTokenType.BeginBracket,
                LexerTokenType.Identifier,
                LexerTokenType.Operator, LexerTokenType.StringLiteral,
                LexerTokenType.Operator, LexerTokenType.Identifier,
                LexerTokenType.BeginBracket,
                LexerTokenType.EndOfLine);
        }
    }
}