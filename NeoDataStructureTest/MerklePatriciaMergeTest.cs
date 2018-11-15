namespace com.github.neoresearch.NeoDataStructureTest
{
    using NeoDataStructure;
    using Xunit;

    public class MerklePatriciaMergeTest
    {
        [Fact]
        public void MergeEmpty()
        {
            var mpA = new MerklePatricia();
            var mpB = new MerklePatricia();
            var mpMerge = mpA.Merge(mpB);
            Assert.True(mpMerge.IsEmpty());
            Assert.True(mpMerge.Validade());
            Assert.Null(mpMerge["oi"]);
            Assert.Null(mpMerge["bala"]);
            Assert.Null(mpMerge[""]);
            Assert.True(mpMerge.Validade());
        }

        [Fact]
        public void MergeDistinctElements()
        {
            var mpA = new MerklePatricia
            {
                ["aa"] = "aa".CompactEncodeString(),
                ["ab"] = "ab".CompactEncodeString(),
                ["ba"] = "ba".CompactEncodeString(),
                ["bb"] = "bb".CompactEncodeString(),
                ["ca"] = "ca".CompactEncodeString()
            };
            Assert.True(mpA.Validade());
            
            var mpB = new MerklePatricia
            {
                ["za"] = "za".CompactEncodeString(),
                ["ya"] = "ya".CompactEncodeString(),
                ["xa"] = "xa".CompactEncodeString(),
                ["zf"] = "zf".CompactEncodeString(),
                ["ze"] = "ze".CompactEncodeString(),
                ["zb"] = "zb".CompactEncodeString()
            };
            Assert.True(mpB.Validade());
            var mpMerge = mpA.Merge(mpB);
            Assert.True(mpMerge.Validade());
            Assert.False(mpMerge.IsEmpty());
            Assert.NotNull(mpMerge["aa"]);
            Assert.NotNull(mpMerge["zb"]);
            Assert.Null(mpMerge["bala"]);
            Assert.Equal(11, mpMerge.Count());
            Assert.True(mpMerge.Validade());
        }

        [Fact]
        public void MergeSameElements()
        {
            var mpA = new MerklePatricia
            {
                ["oi"] = "ola",
                ["oi1"] = "ola1",
                ["oi2"] = "ola2",
                ["oi3"] = "ola3",
                ["bola"] = "bola0",
                ["bola1"] = "bola1",
                ["bola2"] = "bola2",
                ["zza"] = "zza123",
                ["o"] = "o0",
                ["of"] = "of0"
            };
            Assert.True(mpA.Validade());
            var mpB = new MerklePatricia
            {
                ["oi1"] = "ola1",
                ["oi"] = "ola",
                ["oi3"] = "ola3",
                ["o"] = "o0",
                ["zza"] = "zza123",
                ["bola1"] = "bola1",
                ["of"] = "of0",
                ["bola"] = "bola0",
                ["bola2"] = "bola2",
                ["oi2"] = "ola2"
            };
            Assert.True(mpB.Validade());
            
            var mpMerge = mpA.Merge(mpB);
            Assert.True(mpMerge.Validade());
            Assert.False(mpMerge.IsEmpty());
            Assert.NotNull(mpMerge["oi"]);
            Assert.Null(mpMerge["bala"]);
            Assert.Equal(10, mpMerge.Count());
            Assert.True(mpMerge.Validade());

            Assert.Equal("ola", mpMerge["oi"]);
            Assert.Equal("ola1", mpMerge["oi1"]);
            Assert.Equal("ola2", mpMerge["oi2"]);
            Assert.Equal("ola3", mpMerge["oi3"]);
            Assert.Equal("bola0", mpMerge["bola"]);
            Assert.Equal("bola1", mpMerge["bola1"]);
            Assert.Equal("bola2", mpMerge["bola2"]);
            Assert.Equal("o0", mpMerge["o"]);
            Assert.Equal("zza123", mpMerge["zza"]);
        }

        [Fact]
        public void Merge()
        {
            var mpA = new MerklePatricia
            {
                ["oi"] = "oi".CompactEncodeString(),
                ["oi1"] = "oi1".CompactEncodeString(),
                ["oi2"] = "oi2".CompactEncodeString(),
                ["oi3"] = "oi3".CompactEncodeString(),
                ["bola"] = "bola".CompactEncodeString(),
                ["bola1"] = "bola1".CompactEncodeString(),
                ["bola2"] = "bola2".CompactEncodeString(),
                ["zza"] = "zza".CompactEncodeString(),
                ["o"] = "o".CompactEncodeString(),
                ["of"] = "of".CompactEncodeString()
            };
            Assert.True(mpA.Validade());
            var mpB = new MerklePatricia
            {
                ["o"] = "o".CompactEncodeString(),
                ["bola1"] = "bola1".CompactEncodeString(),
                ["of"] = "of".CompactEncodeString(),
                ["bola"] = "bola".CompactEncodeString(),
                ["cao2"] = "cao2".CompactEncodeString(),
                ["cao"] = "cao".CompactEncodeString(),
                ["cao1"] = "cao1".CompactEncodeString(),
                ["bola2"] = "bola2".CompactEncodeString(),
                ["oi2"] = "oi2".CompactEncodeString()
            };
            Assert.True(mpB.Validade());
            
            var mpMerge = mpA.Merge(mpB);
            Assert.False(mpMerge.IsEmpty());
            Assert.NotNull(mpMerge["oi"]);
            Assert.Null(mpMerge["bala"]);
//            Assert.Equal(13, mpMerge.Count());
            Assert.True(mpMerge.Validade());

            Assert.Equal("oi".CompactEncodeString(), mpMerge["oi"]);
            Assert.Equal("oi1".CompactEncodeString(), mpMerge["oi1"]);
            Assert.Equal("oi2".CompactEncodeString(), mpMerge["oi2"]);
            Assert.Equal("oi3".CompactEncodeString(), mpMerge["oi3"]);
            Assert.Equal("bola".CompactEncodeString(), mpMerge["bola"]);
            Assert.Equal("bola1".CompactEncodeString(), mpMerge["bola1"]);
            Assert.Equal("bola2".CompactEncodeString(), mpMerge["bola2"]);
            Assert.Equal("o".CompactEncodeString(), mpMerge["o"]);
            Assert.Equal("of".CompactEncodeString(), mpMerge["of"]);
            Assert.Equal("zza".CompactEncodeString(), mpMerge["zza"]);
            
            Assert.Equal("cao".CompactEncodeString(), mpMerge["cao"]);
            Assert.Equal("cao1".CompactEncodeString(), mpMerge["cao1"]);
            Assert.Equal("cao2".CompactEncodeString(), mpMerge["cao2"]);
            
            var mpC = new MerklePatricia
            {
                ["oi"] = "oi".CompactEncodeString(),
                ["oi1"] = "oi1".CompactEncodeString(),
                ["oi2"] = "oi2".CompactEncodeString(),
                ["oi3"] = "oi3".CompactEncodeString(),
                ["bola"] = "bola".CompactEncodeString(),
                ["bola1"] = "bola1".CompactEncodeString(),
                ["bola2"] = "bola2".CompactEncodeString(),
                ["zza"] = "zza".CompactEncodeString(),
                ["o"] = "o".CompactEncodeString(),
                ["of"] = "of".CompactEncodeString(),
                ["o"] = "o".CompactEncodeString(),
                ["bola1"] = "bola1".CompactEncodeString(),
                ["of"] = "of".CompactEncodeString(),
                ["bola"] = "bola".CompactEncodeString(),
                ["cao2"] = "cao2".CompactEncodeString(),
                ["cao"] = "cao".CompactEncodeString(),
                ["cao1"] = "cao1".CompactEncodeString(),
                ["bola2"] = "bola2".CompactEncodeString(),
                ["oi2"] = "oi2".CompactEncodeString()
            };
            Assert.True(mpC.Validade());
            
            var mpD = new MerklePatricia
            {
                ["bola"] = "bola".CompactEncodeString(),
                ["bola1"] = "bola1".CompactEncodeString(),
                ["bola2"] = "bola2".CompactEncodeString(),
                ["cao"] = "cao".CompactEncodeString(),
                ["cao1"] = "cao1".CompactEncodeString(),
                ["cao2"] = "cao2".CompactEncodeString(),
                ["o"] = "o".CompactEncodeString(),
                ["of"] = "of".CompactEncodeString(),
                ["oi"] = "oi".CompactEncodeString(),
                ["oi1"] = "oi1".CompactEncodeString(),
                ["oi2"] = "oi2".CompactEncodeString(),
                ["oi3"] = "oi3".CompactEncodeString(),
                ["zza"] = "zza".CompactEncodeString()
            };
            Assert.True(mpD.Validade());
//            System.Console.WriteLine($"C:{mpC}");
//            System.Console.WriteLine($"merge:{mpMerge}");
            Assert.Equal(mpC, mpD);
            
            Assert.Equal(mpC, mpMerge);
        }
    }
}