namespace com.github.neoresearch.NeoDataStructureTest
{
    using Xunit;
    using com.github.neoresearch.NeoDataStructure;

    public class TrieCommonTest
    {
        [Fact]
        public void ByteToHexString()
        {
            Assert.Equal("01", ((byte) 1).ByteToHexString());
            Assert.Equal("00", ((byte) 0).ByteToHexString());
            Assert.Equal("09", ((byte) 9).ByteToHexString());
            Assert.Equal("0a", ((byte) 10).ByteToHexString());
            Assert.Equal("0f", ((byte) 15).ByteToHexString());
            Assert.Equal("10", ((byte) 16).ByteToHexString());
        }

        [Fact]
        public void CompactEncode()
        {
            Assert.Equal("11 23 45", new byte[] {0x1, 0x2, 0x3, 0x4, 0x5}.CompactEncode().ByteToHexString());
            Assert.Equal("00 01 23 45", new byte[] {0x0, 0x1, 0x2, 0x3, 0x4, 0x5}.CompactEncode().ByteToHexString());
            Assert.Equal("20 0f 1c b8",
                new byte[] {0x0, 0xf, 0x1, 0xc, 0xb, 0x8, 0x10}.CompactEncode().ByteToHexString());
            Assert.Equal("3f 1c b8", new byte[] {0xf, 0x1, 0xc, 0xb, 0x8, 0x10}.CompactEncode().ByteToHexString());
        }
    }
}