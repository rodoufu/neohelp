namespace com.github.neoresearch.NeoDataStructure
{
    using System.Collections.Generic;

    public class MerklePatricia
    {
        private Dictionary<string, MerklePatriciaNode> db = new Dictionary<string, MerklePatriciaNode>();
        private string RootHash;

        public void Append(string key, string value)
        {
            byte[] keyBytes = key.CompactEncode();
//            byte[] valueBytes = value.CompactEncode();
            var root = RootHash != null ? db[RootHash] : new MerklePatriciaNode();
            while (key.Length > 0)
            {
                var node = root[keyBytes[0]];
                if (node == null)
                {
//                    root[keyBytes[0]] = new MerklePatricia();
//                    root[keyBytes[0]].Value = value;
                }
                else
                {
                    root = db[node];
                }
            }
        }
    }
}