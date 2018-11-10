namespace com.github.neoresearch.NeoDataStructure
{
    using System.Linq.Expressions;
    using System.Security.Cryptography.X509Certificates;
    using System;
    using System.Text;
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
        private string RootHash = string.Empty;

        public string this[string key]
        {
            get => RootHash.IsEmpty() ? null : Get(db[RootHash], key.CompactEncodeString());
            set
            {
                var node = RootHash.IsEmpty() ? null : db[RootHash];
                if (db.ContainsKey(RootHash))
                {
                    db.Remove(RootHash);
                }

                RootHash = Append(node, key.CompactEncodeString(), key, value);
            }
        }

        public bool ContainsKey(string key) => this[key] != null;

        private string Get(string[] node, string path)
        {
            while (true)
            {
                if (path == string.Empty)
                {
                    return node[node.Length - 1];
                }

                switch (node.Length)
                {
                    case LEAF_SIZE when node[0].Substring(1) == path:
                        return node[node.Length - 1];
                    case LEAF_SIZE:
                        return null;
                }

                int kint = int.Parse(path.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                if (node[kint] == null)
                {
                    return null;
                }

                node = db[node[kint]];
                path = path.Substring(1);
            }
        }

        private string Append(string[] node, string path, string key, string value)
        {
            if (node == null)
            {
                node = new[] {2 + path.Length % 2 + path, key, value};
            }
            else if (node.Length == LEAF_SIZE)
            {
                var innerHash = Append(new string[NODE_SIZE], node[0], node[1], node[2]);
                node = db[innerHash];
                db.Remove(innerHash);
                innerHash = Append(node, path, key, value);
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
                    int kint = int.Parse(path.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                    var innerHash = node[kint];
                    var innerNode = innerHash != null ? db[innerHash] : null;
                    if (innerHash != null && db.ContainsKey(innerHash))
                    {
                        db.Remove(innerHash);
                    }

                    node[kint] = Append(innerNode, path.Substring(1), key, value);
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

            int kint = int.Parse(path.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
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
    }
}