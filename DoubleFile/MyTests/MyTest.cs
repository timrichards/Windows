using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DoubleFile;

namespace MyTests
{
    /// <summary>
    /// Summary description for UnitTest3
    /// </summary>
    [TestClass]
    public class MyTest
    {
        public MyTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        struct test_struct
        {
            public string key;
            public byte val;

            public byte Expected { get { return expected != default(byte) ? expected : val; } set { expected = value; } }
            private byte expected;
        };

        [TestMethod]
        public void TestMethod1()
        {
            //
            // TODO: Add test logic here
            //

            TernarySearchTrie<byte> trie = new TernarySearchTrie<byte>();

            List<test_struct> tests = new List<test_struct>
            {
                new test_struct{ key = @"C:\_vs\DoubleFile", val = 1 },
                new test_struct{ key = @"C:\_v s\Doubl eFile", val = 2 },
                new test_struct{ key = @"\C:\/v s\Dou/bl eFile", val = 3 },
                new test_struct{ key = @"/\C:\/v \\\s\D/ou/bl eF/il/e/", val = 4 },
                new test_struct{ key = @"/\C:\/v \\\s\D/ou/bl eF/il/e/", Expected = 4, val = 5 },
                new test_struct{ key = @"/\C:\/v \\\s\D/ou/bl eF/il/e/", Expected = 4, val = 6 },
            };

            Assert.AreEqual(trie.Get(@"a"), default(byte));
            Assert.AreEqual(trie.Get(@"\a"), default(byte));
            Assert.AreEqual(trie.Get(@"a\"), default(byte));
            Assert.AreEqual(trie.Get(@"/a"), default(byte));
            Assert.AreEqual(trie.Get(@"a/"), default(byte));
            Assert.AreEqual(trie.Get(@"a/a"), default(byte));
   //         Assert.AreEqual(trie.Get(@"a/a"), 1);                 // test the test

            foreach (var test in tests)
            {
                TestContext.WriteLine($"Testing { test.key }");
                trie.Put(test.key, test.val);
                Assert.AreEqual(test.Expected, trie.Get(test.key));
            }
        }
    }
}
