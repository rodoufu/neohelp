namespace com.github.neoresearch.NeoDataStructureTest
{
    using Xunit;
    using NeoDataStructure;

    public class StringToolsTest
    {
        [Fact]
        public void IsEquals()
        {
            string a = null;
            Assert.False(a.IsEquals("oi"));
            Assert.False(a.IsEquals(null));
            Assert.False(a.IsEquals("oi", true));
            Assert.True(a.IsEquals(null, true));

            Assert.False("oi".IsEquals(null));
            Assert.False("oi".IsEquals(null, true));
            Assert.True("oi".IsEquals("oi"));
            Assert.False("oi".IsEquals("oi1"));
            Assert.True("oi".IsEquals("oi", true));
            Assert.False("oi".IsEquals("1oi", true));
        }

        [Fact]
        public void IsEmpty()
        {
            string a = null;
            Assert.True(a.IsEmpty());
            Assert.True("".IsEmpty());
            Assert.True(" ".IsEmpty());
            Assert.False("1".IsEmpty());
        }

        [Fact]
        public void Hash()
        {
            var a = new[] {"oi", "ola"};
            var b = new[] {"oi", "ola"};
            var c = new[] {"oi1", "ola"};
            var d = new[] {"oi", "ola", null};
            var e = new[] {"oi", "ola", null};
            
            Assert.Equal(a.Hash(), b.Hash());
            Assert.NotEqual(a.Hash(), c.Hash());
            Assert.NotEqual(b.Hash(), c.Hash());
            Assert.NotEqual(a.Hash(), d.Hash());
            Assert.NotEqual(b.Hash(), d.Hash());
            Assert.Equal(d.Hash(), e.Hash());
        }
    }
}