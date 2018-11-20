using System.Collections.Generic;
using System.Text;

namespace com.github.neoresearch.NeoDataStructure
{
    public class MPTNode
    {
        private const int BranchSize = 18;
        private const int ExtensionSize = 2;
        private const int LeafSize = 3;

        private readonly byte[][] _hashes;
        private MPTNode(int size = 0) => _hashes = new byte[size][];

        public bool IsBranch => _hashes.Length == BranchSize;
        public bool IsExtension => _hashes.Length == ExtensionSize;
        public bool IsLeaf => _hashes.Length == LeafSize;


        public byte[] this[int index]
        {
            get => _hashes[index];
            set => _hashes[index] = value;
        }

        public byte[] Path
        {
            get => _hashes[0];
            set => _hashes[0] = value;
        }

        public byte[] Key
        {
            get => _hashes[_hashes.Length - 2];
            set => _hashes[_hashes.Length - 2] = value;
        }

        public byte[] Value
        {
            get => _hashes[_hashes.Length - 1];
            set => _hashes[_hashes.Length - 1] = value;
        }

        public byte[] Next
        {
            get => _hashes[_hashes.Length - 1];
            set => _hashes[_hashes.Length - 1] = value;
        }

        public byte[] Hash()
        {
            var bytes = new List<byte>();
            for (var i = 0; i < _hashes.Length; i++)
            {
                bytes.Add((byte) i);
                if (_hashes[i] != null)
                {
                    bytes.AddRange(_hashes[i]);
                }
            }

            return new System.Security.Cryptography.SHA256Managed().ComputeHash(bytes.ToArray());
        }

        public override string ToString()
        {
            var resp = new StringBuilder("[");
            var virgula = false;
            for (var i = 0; i < _hashes.Length; i++)
            {
                if (IsBranch && _hashes[i] == null) continue;
                resp.Append(virgula ? "," : "")
                    .Append(IsBranch ? $"{i:x}:" : "")
                    .Append(_hashes[i] != null ? $"\"{_hashes[i].ByteToHexString(false, false)}\"" : "null");
                virgula = true;
            }

            return resp.Append("]").ToString();
        }

        public int Length => _hashes.Length;

        public static MPTNode BranchNode() => new MPTNode(BranchSize);
        public static MPTNode ExtensionNode() => new MPTNode(ExtensionSize);
        public static MPTNode LeafNode() => new MPTNode(LeafSize);
    }
}