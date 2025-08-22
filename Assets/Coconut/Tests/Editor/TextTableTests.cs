using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Aloha.Coconut.Tests.Editor
{
    public class TextTableTests
    {
        [SetUp]
        public void SetUp()
        {
            TextTableV2.Initialize();
        }
        
        [Test]
        public void SimpleTest()
        {
            TextTableV2.ChangeLanguage(SystemLanguage.English);
            TextTableV2.AddTextTableEntry("test" ,"hello", new Dictionary<SystemLanguage, string>()
            { 
                { SystemLanguage.English, "Hello" },
                { SystemLanguage.Korean, "안녕" }
            });
            
            Assert.AreEqual("Hello", TextTableV2.Get("test", "hello"));
            Assert.AreEqual("안녕", TextTableV2.Get("test", "hello", SystemLanguage.Korean));
            
            TextTableV2.ChangeLanguage(SystemLanguage.Korean);
            Assert.AreEqual("안녕", TextTableV2.Get("test", "hello"));
            Assert.AreEqual("안녕", TextTableV2.Get("test/hello"));
            Assert.AreEqual("Hello", TextTableV2.Get("test/hello", SystemLanguage.English));
        }
    }
}