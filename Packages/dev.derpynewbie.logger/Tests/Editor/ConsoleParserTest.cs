using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DerpyNewbie.Logger.Tests.Editor
{
    public class ConsoleParserTest
    {
        [Test]
        public void SimpleTokenizerTest()
        {
            var result = ConsoleParser.Tokenize("Label true false toggle;(abc) \"(abc)\"");
            Assert.AreEqual(result.Length, 9);
            Assert.AreEqual(result[0], "Label");
            Assert.AreEqual(result[1], "true");
            Assert.AreEqual(result[2], "false");
            Assert.AreEqual(result[3], "toggle");
            Assert.AreEqual(result[4], ";");
            Assert.AreEqual(result[5], "(");
            Assert.AreEqual(result[6], "abc");
            Assert.AreEqual(result[7], ")");
            Assert.AreEqual(result[8], "(abc)");
        }

        [Test]
        public void SimpleParserTest()
        {
            var preBuiltTokens = new[]
            {
                "Label",
                "true",
                "false",
                "toggle",
                ";",
                "(", "abc",
                "(", "def", ")",
                "ghj",
                "(", "kln", ")",
                ")", "(abc)"
            };
            var scResult = ConsoleParser.Parse(preBuiltTokens,
                out var scTable,
                out var scResultIndex,
                out var scResultMessage);

            Debug.Log($"ParseResult: {scResult}, {scResultIndex}, {scResultMessage}");

            Assert.IsTrue(scResult);
            Assert.IsNotNull(scTable);

            Debug.Log("scTable:");
            foreach (var table in scTable)
                Debug.Log(string.Join(" ", table));

            Assert.AreEqual(scResultIndex, 0);
            Assert.AreEqual(scResultMessage, "Success");
            Assert.AreEqual(scTable.GetLength(0), 5);
        }
    }
}