using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpNameParser.Tests
{
    [TestClass]
    public class NameParserTests
    {
        [TestMethod]
        public void Can_parse_basic_fn_ln_name()
        {
            const string NAME = "John Doe";
            var expectedResult = new Name() {FirstName = "John", LastName = "Doe"};
            var parser = new NameParser();
            var result = parser.Parse(NAME);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void Can_parse_name_with_all_parts()
        {
            const string NAME = "Mr Anthony R Von Fange III";
            var expectedResult = new Name()
            {
                Salutation = "Mr",
                FirstName = "Anthony",
                MiddleInitials = "R",
                LastName = "Von Fange",
                Suffix = "III"
            };
            var parser = new NameParser();
            var result = parser.Parse(NAME);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void Can_parse_name_in_ln_comma_fn_format()
        {
            const string NAME = "Doe, John";
            var expectedResult = new Name()
            {
                Salutation = "",
                FirstName = "John",
                MiddleInitials = "",
                LastName = "Doe",
                Suffix = ""
            };
            var parser = new NameParser();
            var result = parser.Parse(NAME);
            Assert.AreEqual(expectedResult, result);
        }
    }
}
