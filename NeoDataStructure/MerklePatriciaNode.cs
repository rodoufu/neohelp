namespace com.github.neoresearch.NeoDataStructure
{
    using System.Text;

    public class MerklePatriciaNode
    {
        private string[] Nibble = new string[16];
        public string Value { get; set; }

        public string this[int key]
        {
            get => Nibble[key];
            set => Nibble[key] = value;
        }

        public string Hash()
        {
            var total = new StringBuilder();
            foreach (var x in Nibble)
            {
                total.Append(x);
            }

            return total.ToString().Sha256();
        }
    }
}