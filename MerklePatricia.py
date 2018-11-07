import hashlib
import rlp


class MerklePatricia(object):
	def __init__(self):
		self.db = dict()

	@staticmethod
	def hash(text):
		return hashlib.sha256(text).hexdigest()

	@staticmethod
	def compact_encode(hexarray):
		term = 1 if hexarray[-1] == 16 else 0
		if term:
			hexarray = hexarray[:-1]
		oddlen = len(hexarray) % 2
		flags = 2 * term + oddlen
		if oddlen:
			hexarray = [flags] + hexarray
		else:
			hexarray = [flags, 0] + hexarray
		o = ''
		for i in xrange(0, len(hexarray), 2):
			o += chr(16 * hexarray[i] + hexarray[i + 1])
		return o

	def update(self, node_hash, path, value):
		if path == '':
			curnode = self.db.get(node_hash) if node else [None] * 17
			newnode = curnode.copy()
			newnode[-1] = value
		else:
			curnode = self.db.get(node_hash) if node else [None] * 17
			newnode = curnode.copy()
			newindex = self.update(curnode[path[0]], path[1:], value)
			newnode[path[0]] = newindex
		self.db.put(MerklePatricia.hash(newnode), newnode)
		return MerklePatricia.hash(newnode)

	def delete(self, node_hash, path):
		if node_hash is None:
			return None
		else:
			curnode = self.db.get(node_hash)
			newnode = curnode.copy()
			if path == '':
				newnode[-1] = None
			else:
				newindex = self.delete(curnode[path[0]], path[1:])
				newnode[path[0]] = newindex

			if len(filter(lambda x: x is not None, newnode)) == 0:
				return None
			else:
				self.db.put(MerklePatricia.hash(newnode), newnode)
				return MerklePatricia.hash(newnode)

	def get_helper(self, node, path):
		if not path:
			return node
		if node == '':
			return ''
		curnode = rlp.decode(node if len(node) < 32 else self.db.get(node))
		if len(curnode) == 2:
			(k2, v2) = curnode
			k2 = MerklePatricia.compact_encode(k2)
			if k2 == path[:len(k2)]:
				return self.get_helper(v2, path[len(k2):])
			else:
				return ''
		elif len(curnode) == 17:
			return self.get_helper(curnode[path[0]], path[1:])

	def get(self, node, path):
		path2 = []
		for i in xrange(len(path)):
			path2.append(int(ord(path[i]) / 16))
			path2.append(ord(path[i] % 16))
		path2.append(16)
		return self.get_helper(node, path2)


mp = MerklePatricia()


print MerklePatricia.hash('Nobody inspects the spammish repetition')
print MerklePatricia.hash(123)
print MerklePatricia.hash(MerklePatricia())
