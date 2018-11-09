namespace com.github.neoresearch.NeoDataStructure
{
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Modified Merkel Patricia.
    /// </summary>
    public class MerklePatricia
    {
        private const int LEAF_SIZE = 3;
        private readonly Dictionary<string, string[]> db = new Dictionary<string, string[]>();
        private string RootHash = string.Empty;

        public string this[string key]
        {
            get => RootHash.IsEmpty() ? null : Get(db[RootHash], key, key);
            set
            {
                var node = RootHash.IsEmpty() ? new string[0] : db[RootHash];
                if (db.ContainsKey(RootHash))
                {
                    db.Remove(RootHash);
                }

                RootHash = Append(node, key, key, value);
            }
        }

        private string Get(string[] node, string path, string key)
        {
            return string.Empty;
        }

        private string Append(string[] node, string path, string key, string value)
        {
            if (path.IsEmpty())
            {
                // Already found the node
                node[node.Length - 2] = key;
                node[node.Length - 1] = value;
            }
            else
            {
                if (node.Length == LEAF_SIZE)
                {
                    // When only has an optimization node
                    var innerNodeHash = Append(new string[18], node[0].Substring(1), node[1], node[2]);
                    node = db[innerNodeHash];
                    db.Remove(innerNodeHash);
                }

                if (node.Length == 0)
                {
                    // Creates a leaf
                    node = new[] {(2 + path.Length % 2) + path, key, value};
                    // Says to add a zero when it is even
//                    node = new[] {(2 + path.Length % 2) + (path.Length % 2 == 0 ? "0" : "") + path, key, value};
                }
                else
                {
                    int kint = int.Parse(path.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                    var nodeHash = node[kint];
                    var curnode = nodeHash.IsEmpty() ? new string[0] : db[nodeHash];
                    if (!nodeHash.IsEmpty() && db.ContainsKey(nodeHash))
                    {
                        db.Remove(nodeHash);
                    }

                    node[kint] = Append(curnode, path.Substring(1), key, value);
                }
            }

            var tempHash = Hash(node);
            db[tempHash] = node;
            return tempHash;
        }

        public int Height() =>
            RootHash.IsEmpty() ? 0 : (db[RootHash].Length == LEAF_SIZE ? 1 : 1 + db[RootHash].Select(Height).Max());

        private int Height(string nodeHash) =>
            nodeHash.IsEmpty() ? 0 : (db[nodeHash].Length == LEAF_SIZE ? 1 : 1 + db[nodeHash].Select(Height).Max());

        public static string Hash(string[] node)
        {
            var linha = new StringBuilder();
            for (int i = 0; i < node.Length; i++)
            {
                linha.Append(i).Append(',').Append(node[i] ?? "");
            }

            return linha.ToString().Sha256();
        }

        public bool Validade() => RootHash.IsEmpty() || Validade(RootHash, db[RootHash]);

        private bool Validade(string nodeHash, string[] node)
        {
            if (nodeHash != Hash(node))
            {
                return false;
            }

            if (node.Length == LEAF_SIZE)
            {
                return true;
            }

            for (int i = 0; i < node.Length - LEAF_SIZE + 1; i++)
            {
                var subNodeHash = node[i];
                if (!subNodeHash.IsEmpty() && !Validade(subNodeHash, db[subNodeHash]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}