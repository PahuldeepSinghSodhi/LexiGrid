using System;
using System.Collections.Generic;
using NUnit.Framework;
using LexiGrid.DataStructures;

namespace LexiGrid.Tests
{
    public class MinHeapTests
    {
        [Test]
        public void NewHeap_IsEmpty()
        {
            var heap = new MinHeap<int>();
            Assert.AreEqual(0, heap.Count);
            Assert.IsTrue(heap.IsEmpty);
        }

        [Test]
        public void Insert_SingleItem_CountIsOne()
        {
            var heap = new MinHeap<int>();
            heap.Insert(5);
            Assert.AreEqual(1, heap.Count);
            Assert.IsFalse(heap.IsEmpty);
        }

        [Test]
        public void Peek_ReturnsMinimum()
        {
            var heap = new MinHeap<int>();
            heap.Insert(5);
            heap.Insert(3);
            heap.Insert(8);
            Assert.AreEqual(3, heap.Peek());
        }

        [Test]
        public void Peek_EmptyHeap_ThrowsException()
        {
            var heap = new MinHeap<int>();
            Assert.Throws<InvalidOperationException>(() => heap.Peek());
        }

        [Test]
        public void ExtractMin_ReturnsItemsInOrder()
        {
            var heap = new MinHeap<int>();
            heap.Insert(5);
            heap.Insert(3);
            heap.Insert(8);
            heap.Insert(1);

            Assert.AreEqual(1, heap.ExtractMin());
            Assert.AreEqual(3, heap.ExtractMin());
            Assert.AreEqual(5, heap.ExtractMin());
            Assert.AreEqual(8, heap.ExtractMin());
        }

        [Test]
        public void ExtractMin_EmptyHeap_ThrowsException()
        {
            var heap = new MinHeap<int>();
            Assert.Throws<InvalidOperationException>(() => heap.ExtractMin());
        }

        [Test]
        public void TryInsertIfBetter_HeapNotFull_AlwaysInserts()
        {
            var heap = new MinHeap<int>();
            Assert.IsTrue(heap.TryInsertIfBetter(5, 5));
            Assert.IsTrue(heap.TryInsertIfBetter(3, 5));
            Assert.IsTrue(heap.TryInsertIfBetter(8, 5));
            Assert.AreEqual(3, heap.Count);
        }
        
        [Test]
        public void TryInsertIfBetter_HeapFull_BetterScore_Inserts()
        {
            var heap = new MinHeap<LeaderboardEntry>();
            heap.Insert(new LeaderboardEntry("A", 10));
            heap.Insert(new LeaderboardEntry("B", 20));
            heap.Insert(new LeaderboardEntry("C", 30));

            bool inserted = heap.TryInsertIfBetter(new LeaderboardEntry("D", 25), 3);
            Assert.IsTrue(inserted);
            Assert.AreEqual(3, heap.Count);
            Assert.AreEqual(20, heap.Peek().Score);
        }

        [Test]
        public void TryInsertIfBetter_HeapFull_WorseScore_DoesNotInsert()
        {
            var heap = new MinHeap<LeaderboardEntry>();
            heap.Insert(new LeaderboardEntry("A", 10));
            heap.Insert(new LeaderboardEntry("B", 20));
            heap.Insert(new LeaderboardEntry("C", 30));

            bool inserted = heap.TryInsertIfBetter(new LeaderboardEntry("D", 5), 3);
            Assert.IsFalse(inserted);
            Assert.AreEqual(3, heap.Count);
            Assert.AreEqual(10, heap.Peek().Score);
        }

        [Test]
        public void TryInsertIfBetter_HeapFull_EqualScore_DoesNotInsert()
        {
            var heap = new MinHeap<LeaderboardEntry>();
            heap.Insert(new LeaderboardEntry("A", 10));
            heap.Insert(new LeaderboardEntry("B", 20));
            heap.Insert(new LeaderboardEntry("C", 30));

            bool inserted = heap.TryInsertIfBetter(new LeaderboardEntry("D", 10), 3);
            Assert.IsFalse(inserted);
            Assert.AreEqual(3, heap.Count);
            Assert.AreEqual(10, heap.Peek().Score);
        }

        [Test]
        public void GetSortedDescending_ReturnsCorrectOrder()
        {
            var heap = new MinHeap<int>();
            heap.Insert(5);
            heap.Insert(3);
            heap.Insert(8);
            heap.Insert(1);

            var sorted = heap.GetSortedDescending();
            CollectionAssert.AreEqual(new[] { 8, 5, 3, 1 }, sorted);
        }

        [Test]
        public void GetSortedDescending_DoesNotModifyHeap()
        {
            var heap = new MinHeap<int>();
            heap.Insert(5);
            heap.Insert(3);
            
            var sorted = heap.GetSortedDescending();
            Assert.AreEqual(2, heap.Count);
            Assert.AreEqual(3, heap.Peek());
        }

        [Test]
        public void Clear_EmptiesHeap()
        {
            var heap = new MinHeap<int>();
            heap.Insert(5);
            heap.Clear();
            Assert.IsTrue(heap.IsEmpty);
            Assert.AreEqual(0, heap.Count);
        }

        [Test]
        public void LeaderboardEntry_CompareTo_OrdersByScoreAscending()
        {
            var entry100 = new LeaderboardEntry("A", 100);
            var entry200 = new LeaderboardEntry("B", 200);
            Assert.IsTrue(entry100.CompareTo(entry200) < 0);
        }

        [Test]
        public void LeaderboardWorkflow_FullSimulation()
        {
            var heap = new MinHeap<LeaderboardEntry>();
            var random = new Random(42);
            int maxSize = 10;
            
            int maxScore = 0;

            for (int i = 0; i < 20; i++)
            {
                int score = random.Next(1, 1000);
                if (score > maxScore) maxScore = score;
                heap.TryInsertIfBetter(new LeaderboardEntry("P" + i, score), maxSize);
            }

            Assert.AreEqual(maxSize, heap.Count);
            
            var sorted = heap.GetSortedDescending();
            Assert.AreEqual(maxSize, sorted.Length);
            Assert.AreEqual(maxScore, sorted[0].Score);
            
            for (int i = 0; i < sorted.Length - 1; i++)
            {
                Assert.IsTrue(sorted[i].Score >= sorted[i + 1].Score);
            }
        }
    }
}
