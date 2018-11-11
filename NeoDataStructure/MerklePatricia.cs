namespace com.github.neoresearch.NeoDataStructure
{
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Modified Merkel Patricia Tree.
    /// </summary>
    public class MerklePatricia
    {
        private const int LeafSize = 3;
        private const int NodeSize = 18;
        private readonly Dictionary<string, string[]> _db = new Dictionary<string, string[]>();
        private string _rootHash;

        /// <summary>
        /// Get and set the key and valeu pairs of the tree.
        /// </summary>
        /// <param name="key">The key that indicates the reference.</param>
        public string this[string key]
        {
            get => _rootHash.IsEmpty() ? null : Get(_db[_rootHash], key.CompactEncodeString());
            set
            {
                var node = _rootHash.IsEmpty() ? null : _db[_rootHash];
                if (!_rootHash.IsEmpty() && _db.ContainsKey(_rootHash))
                {
                    _db.Remove(_rootHash);
                }

                _rootHash = Set(node, key.CompactEncodeString(), key, value);
            }
        }

        /// <summary>
        /// Test is contains a specific key.
        /// </summary>
        /// <param name="key">Key to be tested.</param>
        /// <returns>true in the case the tree contains the key.</returns>
        public bool ContainsKey(string key) => this[key] != null;

        private string Get(string[] node, string path)
        {
            while (true)
            {
                if (node == null)
                {
                    return null;
                }

                if (node.Length == LeafSize)
                {
                    return node[0].Substring(1) == path ? node[node.Length - 1] : null;
                }

                if (path.IsEmpty())
                {
                    break;
                }

                int kint = path.Substring(0, 1).FromHex();
                if (node[kint] == null) return null;
                node = _db[node[kint]];
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
            else if (node.Length == LeafSize)
            {
                var innerHash = Set(new string[NodeSize], node[0].Substring(1), node[1], node[2]);
                node = _db[innerHash];
                _db.Remove(innerHash);
                innerHash = Set(node, path, key, value);
                node = _db[innerHash];
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
                    var innerNode = innerHash != null ? _db[innerHash] : null;
                    if (innerHash != null && _db.ContainsKey(innerHash))
                    {
                        _db.Remove(innerHash);
                    }

                    node[kint] = Set(innerNode, path.Substring(1), key, value);
                }
            }

            var tempHash = node.Hash();
            _db[tempHash] = node;
            return tempHash;
        }

        /// <summary>
        /// Removes a specific value.
        /// </summary>
        /// <param name="key">Remove this key from the tree.</param>
        /// <returns>true is the key was present and sucessifully removed.</returns>
        public bool Remove(string key)
        {
            if (_rootHash.IsEmpty())
            {
                return false;
            }

            var removido = Remove(_rootHash, key.CompactEncodeString());
            _db.Remove(_rootHash);
            var resp = removido != _rootHash;
            _rootHash = removido;

            return resp;
        }

        private string Remove(string nodeHash, string path)
        {
            var node = _db[nodeHash];
            switch (node.Length)
            {
                case LeafSize when node[0].Substring(1) == path:
                    _db.Remove(nodeHash);
                    return null;
                case LeafSize:
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
                _db.Remove(node[kint]);
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
                _db[removedHash] = node;
                return removedHash;
            }

            return nodeHash;
        }

        /// <summary>
        /// Calculate the tree heigh.
        /// </summary>
        /// <returns>The height.</returns>
        public int Height() =>
            _rootHash.IsEmpty() ? 0 : (_db[_rootHash].Length == LeafSize ? 1 : 1 + _db[_rootHash].Select(Height).Max());

        private int Height(string nodeHash) =>
            nodeHash.IsEmpty() ? 0 : (_db[nodeHash].Length == LeafSize ? 1 : 1 + _db[nodeHash].Select(Height).Max());


        /// <summary>
        /// Checks if the hashes correspond to their nodes.
        /// </summary>
        /// <returns>In the case the validation is Ok.</returns>
        public bool Validade() => _rootHash.IsEmpty() || Validade(_rootHash, _db[_rootHash]);

        private bool Validade(string nodeHash, string[] node)
        {
            if (nodeHash != node.Hash())
            {
                return false;
            }

            if (node.Length == LeafSize)
            {
                return true;
            }

            for (int i = 0; i < node.Length - LeafSize + 1; i++)
            {
                var subNodeHash = node[i];
                if (!subNodeHash.IsEmpty() && !Validade(subNodeHash, _db[subNodeHash]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares two trees.
        /// </summary>
        /// <param name="obj">The other tree.</param>
        /// <returns>true in case the two tree are equal.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is MerklePatricia))
            {
                return false;
            }

            var objMp = (MerklePatricia) obj;
            var nodeHashA = _rootHash;
            var nodeHashB = objMp._rootHash;
            return !nodeHashA.IsEmpty() && nodeHashA == nodeHashB && NodeEquals(objMp, nodeHashA);
        }

        private bool NodeEquals(MerklePatricia mpB, string nodeHash)
        {
            var nodeA = _db[nodeHash];
            var nodeB = mpB._db[nodeHash];
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

            if (nodeA.Length == LeafSize)
            {
                return nodeA[0].IsEquals(nodeB[0], true) && nodeA[2].IsEquals(nodeB[2], true);
            }

            for (var i = 0; i < nodeA.Length - 2; i++)
            {
                if (!nodeA[i].IsEquals(nodeB[i], true))
                {
                    return false;
                }

                if (!nodeA[i].IsEmpty() && !NodeEquals(mpB, nodeA[i]))
                {
                    return false;
                }
            }

            return nodeA[nodeB.Length - 1].IsEquals(nodeB[nodeB.Length - 1], true);
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

        public override int GetHashCode()
        {
            return _rootHash != null ? _rootHash.GetHashCode() : 0;
        }

        /// <summary>
        /// Generates a JSON for the tree.
        /// </summary>
        /// <returns>The generated JSON tree.</returns>
        public override string ToString()
        {
            return _rootHash.IsEmpty() ? "{}" : $"{NodeToString(_db[_rootHash])}";
        }

        private string NodeToString(string[] node)
        {
            if (node == null)
            {
                return "{}";
            }

            if (node.Length == LeafSize)
            {
                return $"[\"{node[0]}\",\"{node[1]}\",\"{node[2]}\"]";
            }

            var resp = new StringBuilder("{");
            var virgula = false;
            for (var i = 0; i < node.Length - 2; i++)
            {
                var nit = node[i];
                if (nit == null) continue;
                resp.Append(virgula ? "," : "").Append($"\"{i}\":{NodeToString(_db[nit])}");
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
    }
}