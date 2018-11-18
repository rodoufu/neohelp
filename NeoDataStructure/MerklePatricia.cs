namespace com.github.neoresearch.NeoDataStructure
{
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Modified Merkel Patricia Tree.
    /// Note: It is not a thread safe implementation.
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
            get => _rootHash.IsEmpty() ? null : Get(_db[_rootHash], ConvertToNibble(key));
            set
            {
                var node = _rootHash.IsEmpty() ? null : _db[_rootHash];
                if (!_rootHash.IsEmpty() && _db.ContainsKey(_rootHash))
                {
                    _db.Remove(_rootHash);
                }

                _rootHash = Set(node, ConvertToNibble(key), "0-" + key, value);
            }
        }

        // value.CompactEncodeString();
        private static string ConvertToNibble(string value) => Encoding.UTF8.GetBytes(value).ByteToHexString(false);

        /// <summary>
        /// Test if contains a specific key.
        /// </summary>
        /// <param name="key">Key to be tested.</param>
        /// <returns>true in the case the tree contains the key.</returns>
        public bool ContainsKey(string key) => this[key] != null;

        /// <summary>
        /// Test if the tree contains a specific value.
        /// Takes O(n) operations.
        /// </summary>
        /// <param name="value">Value to look for.</param>
        /// <returns>true if the value is present.</returns>
        public bool ContainsValue(string value) => _db.Any(x => x.Value[x.Value.Length - 1] == value);

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

                // TODO Improve not using hex notation
                int kint = path.Substring(0, 1).FromHex();
                if (node[kint] == null) return null;
                node = _db[node[kint]];
                path = path.Substring(1);
            }

            return node[node.Length - 1];
        }

        private static string KeyHash(string key) => key.StartsWith("0-") ? "0+" + key.Sha256() : key;

        private string Set(string[] node, string path, string key, string value)
        {
            if (node == null)
            {
                node = new[] {2 + path.Length % 2 + path, KeyHash(key), value};
            }
            else if (node.Length == LeafSize)
            {
                if (path == node[0].Substring(1))
                {
                    node[2] = value;
                }
                else if (path[0] == node[0][1])
                {
                    var pos = 0;
                    for (;;)
                    {
                        if (pos + 1 == path.Length || pos + 2 == node[0].Length || path[pos + 1] != node[0][pos + 2])
                        {
                            break;
                        }

                        pos++;
                    }

                    var newNode = new[] { "2" + path.Substring(0, pos + 1), null};
                    var innerHash = Set(new string[NodeSize], node[0].Substring(pos + 2), node[1], node[2]);
                    var innerNode = _db[innerHash];
                    _db.Remove(innerHash);
                    newNode[1] = Set(innerNode, path.Substring(pos + 1), key, value);
                    node = newNode;
                }
                else
                {
                    var innerHash = Set(new string[NodeSize], node[0].Substring(1), node[1], node[2]);
                    node = _db[innerHash];
                    _db.Remove(innerHash);
                    innerHash = Set(node, path, key, value);
                    node = _db[innerHash];
                }
            }
            else
            {
                if (path.IsEmpty())
                {
                    node[node.Length - 2] = KeyHash(key);
                    node[node.Length - 1] = value;
                }
                else
                {
                    // TODO Improve not using hex notation
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

            var removido = Remove(_rootHash, ConvertToNibble(key));
            var resp = removido != _rootHash;
            if (resp)
            {
                _db.Remove(_rootHash);
            }

            _rootHash = removido;
            return resp;
        }

        private string Remove(string nodeHash, string path)
        {
            var node = _db[nodeHash];
            if (node.Length == 0)
            {
                return null;
            }

            if (node.Length == LeafSize)
            {
                if (node[0].Substring(1) == path)
                {
                    _db.Remove(nodeHash);
                    return null;
                }

                _db[nodeHash] = node;
                return nodeHash;
            }

            if (path.IsEmpty())
            {
                node[node.Length - 2] = node[node.Length - 1] = null;
            }
            else
            {
                // TODO Improve not using hex notation
                int kint = path.Substring(0, 1).FromHex();
                if (node[kint] != null)
                {
                    var innerHash = Remove(node[kint], path.Substring(1));
                    if (node[kint] != innerHash)
                    {
                        _db.Remove(node[kint]);
                        node[kint] = innerHash;

                        int contar = 0;
                        int indexInnerNode = 0;
                        for (int i = 0; i < node.Length - 2; i++)
                        {
                            if (node[i] != null)
                            {
                                contar++;
                                indexInnerNode = i;
                            }
                        }

                        if (contar == 0)
                        {
                            node = new[] {"2", node[node.Length - 2], node[node.Length - 1]};
                        }
                        else if (contar == 1)
                        {
                            if (node[node.Length - 1] == null)
                            {
                                var innerNodeHash = node[indexInnerNode];
                                var innerNode = _db[innerNodeHash];
                                if (innerNode.Length == LeafSize)
                                {
                                    _db.Remove(innerNodeHash);
                                    node = new[]
                                    {
                                        2 + innerNode[0].Length % 2 + "" + indexInnerNode.ToString("x") +
                                        innerNode[0].Substring(1),
                                        innerNode[innerNode.Length - 2],
                                        innerNode[innerNode.Length - 1]
                                    };
                                }
                            }
                        }
                    }
                }
            }

            nodeHash = node.Hash();
            _db[nodeHash] = node;
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

        /// <summary>
        /// Indicates if the tree is empty.
        /// </summary>
        /// <returns>true when there is no value on the tree.</returns>
        public bool IsEmpty() => Count() == 0;

        /// <summary>
        /// Merges this tree with anoter.
        /// </summary>
        /// <param name="mpB">The other tree to be merged with.</param>
        /// <returns>The merged one tree.</returns>
        public MerklePatricia Merge(MerklePatricia mpB)
        {
            var resp = new MerklePatricia();
            resp._rootHash = resp.Merge(this, mpB, _rootHash, mpB._rootHash);
            return resp;
        }

        private string Merge(MerklePatricia mpA, MerklePatricia mpB, string hashA, string hashB, string lastNibble = "")
        {
            if (hashA == null && hashB == null)
            {
                return null;
            }

            if (hashA == null)
            {
                return MergeCopy(hashB, mpB);
            }

            if (hashB == null)
            {
                return MergeCopy(hashA, mpA);
            }

            var nodeA = mpA._db[hashA];
            var nodeB = mpB._db[hashB];

            string[] node = null;
            string nodeHash = null;
            if (nodeA.Length == NodeSize && nodeB.Length == NodeSize)
            {
                node = new string[NodeSize];
                node[NodeSize - 2] = nodeB[NodeSize - 2];
                node[NodeSize - 1] = nodeB[NodeSize - 1];
                if (nodeA[NodeSize - 1] != null)
                {
                    node[NodeSize - 2] = nodeA[NodeSize - 2];
                    node[NodeSize - 1] = nodeA[NodeSize - 1];
                }

                for (var i = 0; i < NodeSize - 2; i++)
                {
                    if (nodeA[i] != null || nodeB[i] != null)
                    {
                        node[i] = Merge(mpA, mpB, nodeA[i], nodeB[i], i.ToString("x"));
                    }
                }

                nodeHash = node.Hash();
            }
            else if (nodeA.Length == LeafSize && nodeB.Length == LeafSize)
            {
                node = new[] {nodeA[0], nodeA[1], nodeA[2]};
                nodeHash = hashA;
            }
            else if (nodeA.Length == NodeSize)
            {
                nodeHash = MergeCopy(hashA, mpA);
                node = _db[nodeHash];
                _db.Remove(nodeHash);
                nodeHash = Set(node, nodeB[0].Substring(1), nodeB[1], nodeB[2]);
                node = _db[nodeHash];
            }
            else if (nodeB.Length == NodeSize)
            {
                nodeHash = MergeCopy(hashB, mpB);
                node = _db[nodeHash];
                _db.Remove(nodeHash);
                nodeHash = Set(node, nodeA[0].Substring(1), nodeA[1], nodeA[2]);
                node = _db[nodeHash];
            }

            _db[nodeHash] = node;
            return nodeHash;
        }

        private string MergeCopy(string hashNode, MerklePatricia mp)
        {
            var node = mp._db[hashNode];
            if (node.Length != LeafSize)
            {
                var mpNode = node;
                node = new string[NodeSize];
                node[NodeSize - 2] = mpNode[NodeSize - 2];
                node[NodeSize - 1] = mpNode[NodeSize - 1];
                for (var i = 0; i < node.Length - 2; i++)
                {
                    if (mpNode[i] != null)
                    {
                        node[i] = MergeCopy(mpNode[i], mp);
                    }
                }

                hashNode = node.Hash();
            }

            _db[hashNode] = node;
            return hashNode;
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
                resp.Append($",\"{node.Length - 1}\":\"{node[node.Length - 1]}\"");
            }

            resp.Append("}");
            return resp.ToString();
        }

        public int Count() => _db.Count(x => x.Value[x.Value.Length - 1] != null);
    }
}