namespace com.github.neoresearch.NeoDataStructureTest
{
    using System.Collections.Generic;
    using NeoDataStructure;
    using Xunit;

    public class MPTTest
    {
        [Fact]
        public void OperatorEqual()
        {
            var mp = new MPT();
            Assert.False(mp == null);
            mp[new byte[] {0, 0, 1}] = new byte[] {0, 0, 1};
            mp[new byte[] {0, 0, 2}] = new byte[] {0, 0, 2};
        }
    }
}