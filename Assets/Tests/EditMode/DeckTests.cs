using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class DeckTests
    {
        [Test]
        public void StandardDeckHas52UniqueCards()
        {
            var cards = Deck.Standard();
            Assert.AreEqual(52, cards.Length);
            Assert.AreEqual(52, new HashSet<Card>(cards).Count);
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                Assert.AreEqual(13, cards.Count(c => c.Suit == suit));
        }

        [Test]
        public void DealGivesFourDisjointHandsOfThirteen()
        {
            var hands = Deck.Deal(new Random(7));
            Assert.AreEqual(4, hands.Length);
            var all = new HashSet<Card>();
            foreach (var hand in hands)
            {
                Assert.AreEqual(13, hand.Length);
                foreach (var card in hand)
                    Assert.IsTrue(all.Add(card), card + " dealt twice");
            }
            Assert.AreEqual(52, all.Count);
        }

        [Test]
        public void DealtHandsAreSortedSuitThenRankDescending()
        {
            var hands = Deck.Deal(new Random(11));
            foreach (var hand in hands)
            {
                for (int i = 1; i < hand.Length; i++)
                {
                    var prev = hand[i - 1];
                    var cur = hand[i];
                    bool ordered = prev.Suit < cur.Suit
                        || (prev.Suit == cur.Suit && prev.Rank > cur.Rank);
                    Assert.IsTrue(ordered, prev + " before " + cur);
                }
            }
        }

        [Test]
        public void DealIsDeterministicForASeed()
        {
            var a = Deck.Deal(new Random(42));
            var b = Deck.Deal(new Random(42));
            for (int h = 0; h < 4; h++)
                CollectionAssert.AreEqual(a[h], b[h]);
        }

        [Test]
        public void DifferentSeedsGiveDifferentDeals()
        {
            var a = Deck.Deal(new Random(1));
            var b = Deck.Deal(new Random(2));
            bool anyDifference = false;
            for (int h = 0; h < 4 && !anyDifference; h++)
                anyDifference = !a[h].SequenceEqual(b[h]);
            Assert.IsTrue(anyDifference);
        }

        [Test]
        public void CardToStringUsesRankAndSuitChars()
        {
            Assert.AreEqual("K♥", new Card(Suit.Hearts, Rank.King).ToString());
            Assert.AreEqual("T♠", new Card(Suit.Spades, Rank.Ten).ToString());
            Assert.AreEqual("2♣", new Card(Suit.Clubs, Rank.Two).ToString());
            Assert.AreEqual("A♦", new Card(Suit.Diamonds, Rank.Ace).ToString());
        }
    }
}
