using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.github.neoresearch.NeoDataStructure
{
    public class MPT
    {
        private byte[] _rootHash;
        private readonly Dictionary<byte[], MPTNode> _db = new Dictionary<byte[], MPTNode>();

        public string this[string key]
        {
            get
            {
                var resp = this[Encoding.UTF8.GetBytes(key)];
                return resp == null ? null : Encoding.UTF8.GetString(resp);
            }
            set => this[Encoding.UTF8.GetBytes(key)] = value != null ? Encoding.UTF8.GetBytes(value) : null;
        }

        public byte[] this[byte[] key]
        {
            get => _rootHash == null ? null : Get(_db[_rootHash], ConvertToNibble(key));
            set
            {
                var node = _rootHash == null ? null : _db[_rootHash];
                if (_rootHash != null)
                {
                    _db.Remove(_rootHash);
                }

                _rootHash = Set(node, ConvertToNibble(key), key, value);
            }
        }

        private byte[] ConvertToNibble(byte[] key)
        {
            var resp = new byte[key.Length * 2];
            for (var i = 0; i < key.Length; i++)
            {
                resp[2 * i] = (byte) (key[i] / 16);
                resp[2 * i + 1] = (byte) (key[i] % 16);
            }

            return resp;
        }

        private byte[] Set(MPTNode node, byte[] path, byte[] key, byte[] value)
        {
            if (node == null)
            {
                node = MPTNode.LeafNode();
                node.Path = path;
                node.Key = key;
                node.Value = value;
            }
            else if (node.IsLeaf)
            {
                if (path.Length == 0 || path.SequenceEqual(node.Path))
                {
                    node.Key = key;
                    node.Value = value;
                }
                else if (path[0] == node.Path[0])
                {
                    for (var pos = 0;; pos++)
                    {
                        if (pos + 1 == path.Length || pos + 1 == node.Path.Length ||
                            path[pos + 1] != node.Path[pos + 1])
                        {
                            var innerNode = MPTNode.ExtensionNode();
                            innerNode.Path = path.Take(pos + 1).ToArray();
                            innerNode.Next = Set(MPTNode.BranchNode(), node.Path.Skip(pos + 1).ToArray(), node.Key,
                                node.Value);
                            node = innerNode;
                            innerNode = _db[node.Next];
                            node.Next = Set(innerNode, path.Skip(pos + 1).ToArray(), key, value);
                            break;
                        }
                    }
                }
                else
                {
                    var innerHash = Set(MPTNode.BranchNode(), node.Path, node.Key, node.Value);
                    node = _db[innerHash];
                    _db.Remove(innerHash);
                    innerHash = Set(node, path, key, value);
                    node = _db[innerHash];
                    _db.Remove(innerHash);
                }
            }
            else if (node.IsExtension)
            {
                if (path.Length == 0 || path.SequenceEqual(node.Path))
                {
                    var innerHash = node.Next;
                    var innerNode = _db[innerHash];
                    _db.Remove(innerHash);
                    node.Next = Set(innerNode, new byte[0], key, value);
                }
                else if (path[0] == node.Path[0])
                {
                    for (var pos = 0;; pos++)
                    {
                        if (pos + 1 == node.Path.Length)
                        {
                            var innerHash = node.Next;
                            node.Next = Set(_db[innerHash], path.Skip(pos + 1).ToArray(), key, value);
                            _db.Remove(innerHash);
                            break;
                        }

                        if (pos + 1 == path.Length)
                        {
                            var newHash = Set(MPTNode.BranchNode(), new byte[0], key, value);
                            var newNode = _db[newHash];
                            _db.Remove(newHash);
                            newNode[node.Path[pos + 1]] = node.Next;
                            node.Path = node.Path.Skip(pos + 1).ToArray();
                            node = newNode;
                            break;
                        }

                        if (path[pos + 1] != node.Path[pos + 1])
                        {
                            var newHash = Set(MPTNode.BranchNode(), path.Skip(pos + 1).ToArray(), key, value);
                            var newNode = _db[newHash];
                            _db.Remove(newHash);
                            newNode[node.Path[pos + 1]] = node.Next;
                            node.Path = node.Path.Skip(pos + 1).ToArray();
                            node = newNode;
                            break;
                        }
                    }
                }
                else
                {
                    var newHash = Set(MPTNode.BranchNode(), path, key, value);
                    var newNode = _db[newHash];
                    _db.Remove(newHash);
                    newNode[node.Path[1]] = node.Next;
                    node.Path = node.Path.Skip(1).ToArray();
                    node = newNode;
                }
            }
            else
            {
                if (path.Length == 0)
                {
                    node.Key = key;
                    node.Value = value;
                }
                else
                {
                    var innerHash = node[path[0]];
                    var innerNode = innerHash != null ? _db[innerHash] : null;
                    if (innerHash != null)
                    {
                        _db.Remove(innerHash);
                    }

                    node[path[0]] = Set(innerNode, path.Skip(1).ToArray(), key, value);
                }
            }

            var hashNode = node.Hash();
            _db[hashNode] = node;
            return hashNode;
        }

        private byte[] Get(MPTNode node, byte[] path)
        {
            if (node == null)
            {
                return null;
            }

            if (node.IsLeaf)
            {
                if (path.Length == 0)
                {
                    return node.Value;
                }

                return node.Path.SequenceEqual(path) ? node.Value : null;
            }

            if (path.Length == 0 && !node.IsExtension)
            {
                return node.Value;
            }

            if (node.IsExtension)
            {
                if (path.SequenceEqual(node.Path))
                {
                    return Get(_db[node.Next], new byte[0]);
                }

                if (node.Path.Length < path.Length && path.Take(node.Path.Length).ToArray().SequenceEqual(node.Path))
                {
                    return Get(_db[node.Next], path.Skip(node.Path.Length).ToArray());
                }

                return null;
            }

            // Branch node
            return node[path[0]] != null ? Get(_db[node[path[0]]], path.Skip(1).ToArray()) : null;
        }

        /// <summary>
        /// Test if contains a specific key.
        /// </summary>
        /// <param name="key">Key to be tested.</param>
        /// <returns>true in the case the tree contains the key.</returns>
        public bool ContainsKey(string key) => this[key] != null;

        public bool Remove(string key) => Remove(Encoding.UTF8.GetBytes(key));

        public bool Remove(byte[] key)
        {
            if (_rootHash == null)
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

        private byte[] Remove(byte[] nodeHash, byte[] path)
        {
            if (nodeHash == null)
            {
                return null;
            }

            var node = _db[nodeHash];
            if (node.IsLeaf)
            {
                if (node.Path == path)
                {
                    _db.Remove(nodeHash);
                    return null;
                }

                _db[nodeHash] = node;
                return nodeHash;
            }

            if (node.IsExtension)
            {
                if (path.Length == 0)
                {
                    node.Next = Remove(node.Next, new byte[0]);
                }
                else if (path.Length >= node.Path.Length &&
                         path.Take(node.Path.Length).ToArray().SequenceEqual(node.Path))
                {
                    node.Next = Remove(node.Next, path.Skip(node.Path.Length).ToArray());
                }
                else
                {
                    _db[nodeHash] = node;
                    return nodeHash;
                }
            }
            else if (node.IsBranch)
            {
                if (path.Length == 0)
                {
                    node.Key = null;
                    node.Value = null;
                }

                if (node[path[0]] != null)
                {
                    node[path[0]] = Remove(node[path[0]], path.Skip(1).ToArray());
                }
                else
                {
                    return nodeHash;
                }
            }

            nodeHash = node.Hash();
            _db[nodeHash] = node;
            return nodeHash;
        }


        public override string ToString() => _rootHash == null ? "{}" : ToString(_db[_rootHash]);

        private string ToString(MPTNode node)
        {
            if (node.IsExtension)
            {
                return $"{{\"{node.Path.ByteToHexString(false, false)}\": {ToString(_db[node.Next])}}}";
            }

            if (node.IsLeaf)
            {
                return node.ToString();
            }

            var resp = new StringBuilder("{");
            var virgula = false;
            for (var i = 0; i < node.Length; i++)
            {
                if (node[i] == null) continue;
                resp.Append(virgula ? "," : "").Append($"\"{i:x}\":{ToString(_db[node[i]])}");
                virgula = true;
            }

            return resp.Append("}").ToString();
        }
    }
}