using System.Collections.Generic;
using MokomoGamesLib.Editor.CommandLine;
using NUnit.Framework;
using UnityEngine;

namespace MokomoGamesLib.Tests.Editor.CommandLine
{
    public class ArgsParserTest : MonoBehaviour
    {
        [Test]
        public void GetStringOptionTest()
        {
            {
                // 値が設定されている場合
                var value = "TestPort";
                var args = new List<string>
                {
                    "-port",
                    value,
                    ""
                };

                Assert.AreEqual(
                    value,
                    ArgsParser.GetStringOption("-port", args));
            }

            {
                // 値が設定されていない場合
                var args = new List<string>
                {
                    "",
                    "",
                    "-port"
                };

                Assert.AreEqual(string.Empty, ArgsParser.GetStringOption("-port", args));
            }
        }

        [Test]
        public void GetIntOptionTest()
        {
            {
                // 値が設定されている場合
                var args = new List<string>
                {
                    "-port",
                    "7777",
                    ""
                };

                Assert.AreEqual(7777, ArgsParser.GetIntOption("-port", args));
            }

            {
                // 値が設定されていない場合
                var args = new List<string>
                {
                    "",
                    "",
                    "-port"
                };

                Assert.AreEqual(0, ArgsParser.GetIntOption("-port", args));
            }
        }
    }
}