namespace com.github.neoresearch.NeoDataStructureTest
{
    using System.Collections.Generic;
    using NeoDataStructure;
    using Xunit;

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
            Assert.True(merklePatricia.Validade());

            InserirTestar("11a2", "valor2");
            Assert.True(merklePatricia.Validade());

            InserirTestar("0212", "valor3");
            Assert.Equal(3, merklePatricia.Height());
            Assert.True(merklePatricia.Validade());

            merklePatricia["0"] = "valor4";
            Assert.True(merklePatricia.Validade());
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
            Assert.True(merklePatricia.Validade());

            merklePatricia["orelha"] = "batatatinha";
            Assert.Equal("batatatinha", merklePatricia["orelha"]);
            Assert.True(merklePatricia.Validade());

            merklePatricia["orfão"] = "criança";
            Assert.Equal("criança", merklePatricia["orfão"]);
            Assert.True(merklePatricia.Validade());

            merklePatricia["orfanato"] = "crianças";
            Assert.Equal("crianças", merklePatricia["orfanato"]);
            Assert.True(merklePatricia.Validade());

            merklePatricia.Remove("orfanato");
            Assert.Equal("criança", merklePatricia["orfão"]);
            Assert.False(merklePatricia.ContainsKey("orfanato"));
            Assert.True(merklePatricia.Validade());

            merklePatricia["orfã"] = "menina";
            Assert.Equal("menina", merklePatricia["orfã"]);
            Assert.True(merklePatricia.Validade());
        }

        [Fact]
        public void Remove()
        {
            var mp = new MerklePatricia();
            Assert.True(mp.Validade());

            void RemoverTestar(string x, string y)
            {
                mp[x] = y;
                Assert.True(mp.ContainsKey(x));
                Assert.False(mp.ContainsKey(x + "123"));
                Assert.Equal(y, mp[x]);
                Assert.Null(mp[x + "123k"]);

                Assert.True(mp.Remove(x));
                Assert.False(mp.Remove(x));
            }

            RemoverTestar("oi", "bala");
            Assert.True(mp.Validade());
            mp.Remove("oi");
            Assert.False(mp.ContainsKey("oi"));
            Assert.True(mp.Validade());

            mp["123"] = "abc";
            mp["a123"] = "1abc";
            Assert.Equal(2, mp.Count());
            Assert.True(mp.Validade());

            Assert.False(mp.Remove("b123"));
            Assert.Equal(2, mp.Count());
            Assert.True(mp.Remove("a123"));
            Assert.True(mp.Validade());
            Assert.Equal(1, mp.Count());
            Assert.False(mp.ContainsKey("a123"));
            Assert.True(mp.ContainsKey("123"));
            Assert.True(mp.ContainsKey("123"));
            Assert.True(mp.Validade());

            var mp2 = new MerklePatricia {["123"] = "abc"};
            Assert.Equal(mp2, mp);
            Assert.True(mp.Validade());
            Assert.True(mp2.Validade());
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
            Assert.True(mpA.Validade());

            var mpB = new MerklePatricia
            {
                ["oi"] = "bola",
                ["oi1"] = "1bola",
                ["oi2"] = "b2ola",
                ["oi1"] = "bola1"
            };
            Assert.True(mpB.Validade());
            Assert.Equal(mpA, mpB);

            mpA["oi"] = "escola";
            Assert.NotEqual(mpA, mpB);
            Assert.True(mpA.Validade());

            mpB["oi"] = "escola";
            Assert.Equal(mpA, mpB);
            Assert.True(mpB.Validade());

            mpA["oi123"] = "escola";
            mpA["oi12"] = "escola1";
            mpA["bola"] = "escola2";
            mpA["dog"] = "escola2";
            Assert.True(mpA.Validade());

            mpB["bola"] = "escola2";
            mpB["dog"] = "escola2";
            mpB["oi12"] = "escola1";
            mpB["oi123"] = "escola";
            Assert.Equal(mpA, mpB);
            Assert.True(mpB.Validade());

            mpA.Remove("oi");
            mpA.Remove("oi");
            Assert.NotEqual(mpA, mpB);
            Assert.True(mpA.Validade());

            mpB.Remove("oi");
            Assert.Equal(mpA, mpB);
            Assert.True(mpB.Validade());
        }

        [Fact]
        public void Lista()
        {
            var lista = new[] {"oi", "oi1", "oi2", "oi12", "bola", "birosca123", "ca123", "oi123"};
            var mp = new MerklePatricia();
            foreach (var it in lista)
            {
                mp[it] = it.CompactEncodeString();
                System.Console.WriteLine($"{mp}");
            Assert.True(mp.Validade());
            }
        }

        [Fact]
        public void Dictionary()
        {
            var exemplo = new Dictionary<string, string>
            {
                ["oi"] = "bala",
                ["oi1"] = "bala1",
                ["oi2"] = "bala2",
                ["oi12"] = "bala12",
                ["bola"] = "oi",
                ["birosca123"] = "bruca123",
                ["ca123"] = "que123",
                ["oi123"] = "bala123"
            };

            var merklePatricia = new MerklePatricia();
            foreach (var keyValue in exemplo)
            {
                merklePatricia[keyValue.Key] = keyValue.Value;
            }
            Assert.True(merklePatricia.Validade());

            foreach (var keyValue in exemplo)
            {
                Assert.Equal(keyValue.Value, merklePatricia[keyValue.Key]);
            }
            Assert.True(merklePatricia.Validade());
        }

        [Fact]
        public void PatriciaToString()
        {
            var mp = new MerklePatricia();
            Assert.True(mp.Validade());
            System.Console.WriteLine($"a:\n {mp}");
            mp["oi"] = "bala";
            Assert.True(mp.Validade());
            System.Console.WriteLine($"a:\n {mp}");
            mp["oi1"] = "bala1";
            System.Console.WriteLine($"a:\n {mp}");
            mp["oi2"] = "bala2";
            System.Console.WriteLine($"a:\n {mp}");
            mp["oi12"] = "bala12";
            Assert.True(mp.Validade());
            System.Console.WriteLine($"a:\n {mp}");
            mp["bola"] = "123bala12";
            System.Console.WriteLine($"a:\n {mp}");
            mp["birosca123"] = "13bala12";
            System.Console.WriteLine($"a:\n {mp}");
            mp["ca123"] = "3bala12";
            mp["oi123"] = "asfbala12";
            Assert.True(mp.Validade());
            System.Console.WriteLine($"a:\n {mp}");
        }

        [Fact]
        public void PatriciaCount()
        {
            var mp = new MerklePatricia();
            Assert.Equal(0, mp.Count());
            Assert.True(mp.Validade());

            mp["oi"] = "oi";
            Assert.Equal(1, mp.Count());
            Assert.True(mp.Validade());

            mp["oi"] = "oi";
            Assert.Equal(1, mp.Count());
            Assert.True(mp.Validade());

            mp["oi"] = "oi1";
            Assert.Equal(1, mp.Count());

            mp["oi1"] = "oi1";
            mp["oi2"] = "oi2";
            Assert.Equal(3, mp.Count());

            mp["bala"] = "bala2";
            Assert.Equal(4, mp.Count());
            Assert.True(mp.Validade());
        }

        [Fact]
        public void ContainsValue()
        {
            var mp = new MerklePatricia
            {
                ["aoi"] = "oi",
                ["boi2"] = "oi2",
                ["coi1"] = "oi3"
            };
            Assert.True(mp.Validade());
            Assert.True(mp.ContainsValue("oi"));
            Assert.True(mp.ContainsValue("oi2"));
            Assert.True(mp.ContainsValue("oi3"));

            Assert.False(mp.ContainsValue("aoi"));
            Assert.False(mp.ContainsValue("boi2"));
            Assert.False(mp.ContainsValue("coi3"));
            Assert.True(mp.Validade());
        }
    }
}