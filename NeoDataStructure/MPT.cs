using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.github.neoresearch.NeoDataStructure
{
    public class MPT
    {
        private byte[] _rootHash;
        private readonly Dictionary<byte[], MPTNode> _db = new Dictionary<byte[], MPTNode>();

        public byte[] this[byte[] key]
        {
//            get => _rootHash == null ? null : Get(_db[_rootHash], ConvertToNibble(key));
            get => _rootHash == null ? null : _rootHash;
            set
            {
                var node = _rootHash == null ? null : _db[_rootHash];
                if (_rootHash != null && _db.ContainsKey(_rootHash))
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

        public string this[string key]
        {
            get => Encoding.Default.GetString(this[Encoding.UTF8.GetBytes(key)]);
            set => this[Encoding.UTF8.GetBytes(key)] = Encoding.UTF8.GetBytes(value);
        }

        private byte[] Set(ExtensionNode node, byte[] path, byte[] key, byte[] value)
        {
            /*
            if (path.Length == 0)
            {
                var newNode = new BranchNode
                {
                    Key = key,
                    Value = value
                };
                var nodeHash = Set(newNode, node.Path, node.Key, node.Value);
                node = _db[nodeHash];
            }
            else if (node.Path == path)
            {
                node.Key = key;
                node.Value = value;
            }
            else if (node.Path[0] == path[0])
            {
                for (var pos = 0;; pos++)
                {
                    if (pos + 1 != node.Path.Length && pos + 1 != path.Length &&
                        node.Path[pos + 1] == path[pos + 1]) continue;
                    var newNode = new MPTNode {Path = path.Take(pos + 1).ToArray()};
                    var innerHash = Set(new MPTNode(true), node.Path.Skip(pos + 1).ToArray(), node.Key,
                        node.Value);
                    var innerNode = _db[innerHash];
                    _db.Remove(innerHash);
                    newNode[1] = Set(innerNode, path.Skip(pos + 1).ToArray(), key, value);
                    node = newNode;
                    break;
                }
            }
            else
            {
                var newNode = new MPTNode {Path = path.Take(pos + 1).ToArray()};
                var innerHash = Set(new MPTNode(true), node.Path.Skip(pos + 1).ToArray(), node.Key, node.Value);
                var innerNode = _db[innerHash];
                _db.Remove(innerHash);
                newNode[1] = Set(innerNode, path.Skip(pos + 1).ToArray(), key, value);
                node = newNode;
            }
            */

            var tempHash = node.Hash();
            _db[tempHash] = node;
            return tempHash;
        }

        private byte[] Set(LeafNode node, byte[] path, byte[] key, byte[] value)
        {
            var tempHash = node.Hash();
            _db[tempHash] = node;
            return tempHash;
        }

        private byte[] Set(BranchNode node, byte[] path, byte[] key, byte[] value)
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
                if (innerHash != null && _db.ContainsKey(innerHash))
                {
                    _db.Remove(innerHash);
                }

                node[path[0]] = Set(innerNode, path.Skip(1).ToArray(), key, value);
            }

            var tempHash = node.Hash();
            _db[tempHash] = node;
            return tempHash;
        }

        private byte[] Set(MPTNode node, byte[] path, byte[] key, byte[] value)
        {
            if (node == null)
            {
                node = new LeafNode
                {
                    Path = path,
                    Key = key,
                    Value = value
                };
            }

            var tempHash = node.Hash();
            _db[tempHash] = node;
            return tempHash;
        }

        public override string ToString() => _rootHash == null ? "{}" : $"{ToString(_db[_rootHash])}";

        private string ToString(MPTNode node)
        {
            if (node is LeafNode || node is ExtensionNode)
            {
                return node.ToString();
            }

            var resp = new StringBuilder("{");
            for (var i = 0; i < node.Length; i++)
            {
                if (node[i] != null)
                {
                    resp.Append($"\"{i}\":{ToString(_db[node[i]])}");
                }
            }

            return resp.Append("}").ToString();
        }
    }
}