namespace com.github.neoresearch.NeoDataStructureTest
{
    using Xunit;
    using NeoDataStructure;

    public class MerklePatriciaTest
    {
        [Fact]
        public void OperatorEqual()
        {
            var merklePatricia = new MerklePatricia();
            Assert.False(merklePatricia == null);
        }

        [Fact]
        public void One()
        {
            var merklePatricia = new MerklePatricia();
            Assert.True(merklePatricia.Validade());
            Assert.Equal(0, merklePatricia.Height());

            void InserirTestar(string x, string y)
            {
                merklePatricia[x] = y;
                Assert.True(merklePatricia.Validade());
                Assert.True(merklePatricia.ContainsKey(x));
                Assert.False(merklePatricia.ContainsKey(x + "123"));
                Assert.Equal(y, merklePatricia[x]);
                Assert.Null(merklePatricia[x + "123k"]);
            }

            InserirTestar("01a2", "valor1");
            Assert.Equal(1, merklePatricia.Height());

            InserirTestar("11a2", "valor2");

            InserirTestar("0212", "valor3");
            Assert.Equal(3, merklePatricia.Height());

            merklePatricia["0"] = "valor4";
        }

        [Fact]
        public void ColideKeys()
        {
            var merklePatricia = new MerklePatricia
            {
                ["oi"] = "batata",
                ["oi"] = "batatatinha"
            };
            Assert.True(merklePatricia.ContainsKey("oi"));
            Assert.Equal("batatatinha", merklePatricia["oi"]);

            merklePatricia["orelha"] = "batatatinha";
            Assert.Equal("batatatinha", merklePatricia["orelha"]);

            merklePatricia["orfão"] = "criança";
            Assert.Equal("criança", merklePatricia["orfão"]);

            merklePatricia["orfanato"] = "crianças";
            Assert.Equal("crianças", merklePatricia["orfanato"]);

            merklePatricia.Remove("orfanato");
            Assert.Equal("criança", merklePatricia["orfão"]);
            Assert.False(merklePatricia.ContainsKey("orfanato"));

            merklePatricia["orfã"] = "menina";
            Assert.Equal("menina", merklePatricia["orfã"]);
        }

        [Fact]
        public void Remove()
        {
            var merklePatricia = new MerklePatricia();

            void RemoverTestar(string x, string y)
            {
                merklePatricia[x] = y;
                Assert.True(merklePatricia.ContainsKey(x));
                Assert.False(merklePatricia.ContainsKey(x + "123"));
                Assert.Equal(y, merklePatricia[x]);
                Assert.Null(merklePatricia[x + "123k"]);

                Assert.True(merklePatricia.Remove(x));
                Assert.False(merklePatricia.Remove(x));
            }

            RemoverTestar("oi", "bala");
        }

        [Fact]
        public void EqualsThree()
        {
            var mpA = new MerklePatricia
            {
                ["oi"] = "bola",
                ["oi1"] = "1bola",
                ["oi2"] = "b2ola",
                ["oi1"] = "bola1"
            };

            var mpB = new MerklePatricia
            {
                ["oi"] = "bola",
                ["oi1"] = "1bola",
                ["oi2"] = "b2ola",
                ["oi1"] = "bola1"
            };
            Assert.Equal(mpA, mpB);

            mpA["oi"] = "escola";
            Assert.NotEqual(mpA, mpB);

            mpB["oi"] = "escola";
            Assert.Equal(mpA, mpB);

            mpA["oi123"] = "escola";
            mpA["oi12"] = "escola1";
            mpA["bola"] = "escola2";
            mpA["dog"] = "escola2";

            mpB["bola"] = "escola2";
            mpB["dog"] = "escola2";
            mpB["oi12"] = "escola1";
            mpB["oi123"] = "escola";
            Assert.Equal(mpA, mpB);

            mpA.Remove("oi");
            mpA.Remove("oi");
            Assert.NotEqual(mpA, mpB);

            mpB.Remove("oi");
            Assert.Equal(mpA, mpB);
        }
    }
}