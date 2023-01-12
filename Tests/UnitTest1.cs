using System.Diagnostics;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        private bool IsComplex(string path)
        {
            var i = new IsComplex(path);
            return i.IsComplexTom();
        }

        [TestMethod]
        public void Simple()
        {
            Assert.IsFalse(IsComplex("a/b/c"));
            Assert.IsFalse(IsComplex("a"));
            Assert.IsFalse(IsComplex("a/.b/c"));
            Assert.IsFalse(IsComplex("a/b/c.foo"));
            Assert.IsFalse(IsComplex("a.a.a.a/.a.b./.c"));
            Assert.IsFalse(IsComplex("_./._/._"));
            Assert.IsFalse(IsComplex(".../.../..."));
        }

        [TestMethod]
        public void Complex()
        {
            Assert.IsTrue(IsComplex("a/b/../c"));
            Assert.IsTrue(IsComplex("a/b/./c"));
            Assert.IsTrue(IsComplex("."));
            Assert.IsTrue(IsComplex(".."));
            Assert.IsTrue(IsComplex("./.."));
            Assert.IsTrue(IsComplex("../."));
            Assert.IsTrue(IsComplex("/."));
            Assert.IsTrue(IsComplex("./"));
        }
    }
}