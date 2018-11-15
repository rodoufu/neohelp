namespace com.github.neoresearch.NeoDataStructureTest
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using NeoDataStructure;
    using Xunit;

    public class MerklePatriciaParallelTest
    {
        private MerklePatricia mpA = new MerklePatricia();
        private MerklePatricia mpB = new MerklePatricia();
        private MerklePatricia mpC = new MerklePatricia();
        private MerklePatricia mpD = new MerklePatricia();
        private List<string> listaA = new List<string>();
        private List<string> listaB = new List<string>();
        private List<string> listaC = new List<string>();
        private List<string> listaD = new List<string>();

        [Fact]
        public void ParallelThreds()
        {
            var amount = 10000;
            var prefixSize = 20;
            var size = 40;
            var prefixA = StringTools.RandomString(prefixSize);
            var prefixB = StringTools.RandomString(prefixSize);
            var prefixC = StringTools.RandomString(prefixSize);
            var prefixD = StringTools.RandomString(prefixSize);
            for (var i = 0; i < amount; i++)
            {
                listaA.Add(prefixA + StringTools.RandomString(size));
                listaB.Add(prefixB + StringTools.RandomString(size));
                listaC.Add(prefixC + StringTools.RandomString(size));
                listaD.Add(prefixD + StringTools.RandomString(size));
            }

            var threadA = new Thread(RunThreadA);
            var threadB = new Thread(RunThreadB);
            var threadC = new Thread(RunThreadC);
            var threadD = new Thread(RunThreadD);
            var timeThreds = Stopwatch.StartNew();
            var timeThredsAndMerge = Stopwatch.StartNew();
            threadA.Start();
            threadB.Start();
            threadC.Start();
            threadD.Start();

            System.Console.WriteLine("Começou");

            threadA.Join();
            threadB.Join();
            threadC.Join();
            threadD.Join();

            timeThreds.Stop();
            var mpMerged = mpA.Merge(mpB);
            mpMerged = mpMerged.Merge(mpC);
            mpMerged = mpMerged.Merge(mpD);
            timeThredsAndMerge.Stop();

            var timeNoThred = Stopwatch.StartNew();
            var mpNoThread = new MerklePatricia();
            RunThread(mpNoThread, listaA);
            RunThread(mpNoThread, listaB);
            RunThread(mpNoThread, listaC);
            RunThread(mpNoThread, listaD);
            timeNoThred.Stop();

            System.Console.WriteLine($"time using threds: {timeThreds.ElapsedMilliseconds}ms");
            System.Console.WriteLine(
                $"time using threds: {timeThredsAndMerge.ElapsedMilliseconds}ms (with merge time)");
            System.Console.WriteLine($"time using no thred: {timeNoThred.ElapsedMilliseconds}ms");
            System.Console.WriteLine(
                $"{timeThreds.ElapsedMilliseconds},{timeThredsAndMerge.ElapsedMilliseconds},{timeNoThred.ElapsedMilliseconds}");
            System.Console.WriteLine(
                $"{timeNoThred.ElapsedMilliseconds}/{timeThredsAndMerge.ElapsedMilliseconds}={(double) timeNoThred.ElapsedMilliseconds / timeThredsAndMerge.ElapsedMilliseconds}");

            Assert.Equal(mpMerged, mpNoThread);
        }

        private void RunThread(MerklePatricia mp, IEnumerable<string> lista)
        {
            foreach (var it in lista)
            {
                mp[it] = it;
            }
        }

        private void RunThreadA() => RunThread(mpA, listaA);
        private void RunThreadB() => RunThread(mpB, listaB);
        private void RunThreadC() => RunThread(mpC, listaC);
        private void RunThreadD() => RunThread(mpD, listaD);
    }
}