namespace com.github.neoresearch.NeoDataStructureTest
{
    using Xunit;
    using NeoDataStructure;

    public class MerklePatriciaTest
    {
        [Fact]
        public void MerklePatriciaOne()
        {
            var merklePatricia = new MerklePatricia();
            Assert.True(merklePatricia.Validade());
            Assert.Equal(0, merklePatricia.Height());

            merklePatricia["01a2"] = "valor1";
            Assert.True(merklePatricia.Validade());
            Assert.Equal(1, merklePatricia.Height());

            merklePatricia["11a2"] = "valor2";
            Assert.True(merklePatricia.Validade());
            Assert.Equal(2, merklePatricia.Height());

            merklePatricia["0212"] = "valor3";
            Assert.True(merklePatricia.Validade());
            Assert.Equal(3, merklePatricia.Height());

            merklePatricia["0"] = "valor4";
        }
    }
}