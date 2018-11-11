using System.Text;

namespace com.github.neoresearch.NeoDataStructure
{
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Modified Merkel Patricia.
    /// </summary>
    public class MerklePatricia
    {
        private const int LEAF_SIZE = 3;
        private const int NODE_SIZE = 18;
        private readonly Dictionary<string, string[]> db = new Dictionary<string, string[]>();
        private string RootHash;

        public string this[string key]
        {
            get => RootHash.IsEmpty() ? null : Get(db[RootHash], key.CompactEncodeString());
            set
            {
                var node = RootHash.IsEmpty() ? null : db[RootHash];
                if (!RootHash.IsEmpty() && db.ContainsKey(RootHash))
                {
                    db.Remove(RootHash);
                }

                RootHash = Set(node, key.CompactEncodeString(), key, value);
            }
        }

        public bool ContainsKey(string key) => this[key] != null;

        private string Get(string[] node, string path)
        {
            while (true)
            {
                if (node == null)
                {
                    return null;
                }

                if (node.Length == LEAF_SIZE)
                {
                    return node[0].Substring(1) == path ? node[node.Length - 1] : null;
                }

                if (path.IsEmpty())
                {
                    break;
                }

                int kint = path.Substring(0, 1).FromHex();
                if (node[kint] == null) return null;
                node = db[node[kint]];
                path = path.Substring(1);
            }

            return node[node.Length - 1];
        }

        private string Set(string[] node, string path, string key, string value)
        {
            if (node == null)
            {
                node = new[] {2 + path.Length % 2 + path, key, value};
            }
            else if (node.Length == LEAF_SIZE)
            {
                var innerHash = Set(new string[NODE_SIZE], node[0].Substring(1), node[1], node[2]);
                node = db[innerHash];
                db.Remove(innerHash);
                innerHash = Set(node, path, key, value);
                node = db[innerHash];
            }
            else
            {
                if (path.IsEmpty())
                {
                    node[node.Length - 2] = key;
                    node[node.Length - 1] = value;
                }
                else
                {
                    int kint = path.Substring(0, 1).FromHex();
                    var innerHash = node[kint];
                    var innerNode = innerHash != null ? db[innerHash] : null;
                    if (innerHash != null && db.ContainsKey(innerHash))
                    {
                        db.Remove(innerHash);
                    }

                    node[kint] = Set(innerNode, path.Substring(1), key, value);
                }
            }

            var tempHash = node.Hash();
            db[tempHash] = node;
            return tempHash;
        }

        public bool Remove(string key)
        {
            if (RootHash.IsEmpty())
            {
                return false;
            }

            var removido = Remove(RootHash, key.CompactEncodeString());
            db.Remove(RootHash);
            var resp = removido != RootHash;
            RootHash = removido;

            return resp;
        }

        private string Remove(string nodeHash, string path)
        {
            var node = db[nodeHash];
            switch (node.Length)
            {
                case LEAF_SIZE when node[0].Substring(1) == path:
                    db.Remove(nodeHash);
                    return null;
                case LEAF_SIZE:
                    return nodeHash;
            }

            int kint = path.Substring(0, 1).FromHex();
            if (node[kint] == null)
            {
                return nodeHash;
            }

            var innerNodeHash = Remove(node[kint], path.Substring(1));
            if (node[kint] != innerNodeHash)
            {
                db.Remove(node[kint]);
                node[kint] = innerNodeHash;
                if (node[node.Length - 2] == null && node.Count(x => x != null) == 1)
                {
                    var index = 0;
                    while (node[index] == null)
                    {
                        index++;
                    }

//                    node = db[node[index]];
                }

                var removedHash = node.Hash();
                db[removedHash] = node;
                return removedHash;
            }

            return nodeHash;
        }

        public int Height() =>
            RootHash.IsEmpty() ? 0 : (db[RootHash].Length == LEAF_SIZE ? 1 : 1 + db[RootHash].Select(Height).Max());

        private int Height(string nodeHash) =>
            nodeHash.IsEmpty() ? 0 : (db[nodeHash].Length == LEAF_SIZE ? 1 : 1 + db[nodeHash].Select(Height).Max());


        public bool Validade() => RootHash.IsEmpty() || Validade(RootHash, db[RootHash]);

        private bool Validade(string nodeHash, string[] node)
        {
            if (nodeHash != node.Hash())
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

        public override bool Equals(object obj)
        {
            if (!(obj is MerklePatricia))
            {
                return false;
            }

            var objMp = (MerklePatricia) obj;
            var nodeHashA = RootHash;
            var nodeHashB = objMp.RootHash;
            return !nodeHashA.IsEmpty() && nodeHashA == nodeHashB && nodeEquals(objMp, nodeHashA);
        }

        private bool nodeEquals(MerklePatricia mpB, string nodeHash)
        {
            var nodeA = db[nodeHash];
            var nodeB = mpB.db[nodeHash];
            if (nodeA.Length != nodeB.Length)
            {
                return false;
            }

            if (nodeA.Length == 0)
            {
                return true;
            }

            if (!nodeA[nodeA.Length - 2].IsEquals(nodeB[nodeB.Length - 2], true) ||
                !nodeA[nodeA.Length - 1].IsEquals(nodeB[nodeB.Length - 1], true))
            {
                return false;
            }

            if (nodeA.Length == LEAF_SIZE)
            {
                return nodeA[0].IsEquals(nodeB[0], true);
            }

            for (var i = 0; i < nodeA.Length - 2; i++)
            {
                if (!nodeA[i].IsEquals(nodeB[i], true))
                {
                    return false;
                }

                if (!nodeA[i].IsEmpty() && !nodeEquals(mpB, nodeA[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(MerklePatricia b1, MerklePatricia b2)
        {
            if ((object) b1 == null)
            {
                return (object) b2 == null;
            }

            return b1.Equals(b2);
        }

        public static bool operator !=(MerklePatricia b1, MerklePatricia b2)
        {
            return !(b1 == b2);
        }

        private string nodeToString(string[] node)
        {
            if (node == null)
            {
                return "{}";
            }

            if (node.Length == LEAF_SIZE)
            {
                return $"[\"{node[0]}\",\"{node[1]}\",\"{node[2]}\"]";
            }

            var resp = new StringBuilder("{");
            var virgula = false;
            for (var i = 0; i < node.Length - 2; i++)
            {
                var nit = node[i];
                if (nit == null) continue;
                resp.Append(virgula ? "," : "").Append($"\"{i}\":{nodeToString(db[nit])}");
                virgula = true;
            }

            if (node[node.Length - 2] != null)
            {
                resp.Append(virgula ? "," : "").Append($"\"{node.Length - 2}\":\"{node[node.Length - 2]}\"");
                resp.Append(virgula ? "," : "").Append($"\"{node.Length - 1}\":\"{node[node.Length - 1]}\"");
            }

            resp.Append("}");
            return resp.ToString();
        }

        public override string ToString()
        {
            return RootHash.IsEmpty() ? "{}" : $"{nodeToString(db[RootHash])}";
        }
    }
}