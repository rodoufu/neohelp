namespace com.github.neoresearch.NeoDataStructure
{
    public class TrieCommon
    {
        private TrieCommon[] node = new TrieCommon[16];
        private byte[] value;

        public void Insert(byte[] key, byte[] valor)
        {
        }

        public byte FirstNibble(byte value) => (byte) (value & 0xf0 >> 15);
        public byte LastNibble(byte value) => (byte) (value & 0x0f);
    }
}