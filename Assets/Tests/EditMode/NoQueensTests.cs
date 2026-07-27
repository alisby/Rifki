using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class NoQueensTests
    {
        static Card C(Suit s, Rank r) => new Card(s, r);

        // Every seat holds one whole suit: South clubs, West diamonds, North hearts, East spades.
        static IReadOnlyList<Card>[] SuitPerSeat()
        {
            var hands = new IReadOnlyList<Card>[4];
            for (int s = 0; s < 4; s++)
                hands[s] = Deck.Standard().Where(c => (int)c.Suit == s).ToArray();
            return hands;
        }

        // Deck dealt round-robin, so every seat can follow any lead. South ends up
        // with the queen of hearts, West the queen of spades, North the queen of
        // clubs, East the queen of diamonds.
        static IReadOnlyList<Card>[] RoundRobin()
        {
            var deck = Deck.Standard();
            var lists = new[] { new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
            for (int i = 0; i < deck.Length; i++)
                lists[i % 4].Add(deck[i]);
            return lists.Select(l => (IReadOnlyList<Card>)l.ToArray()).ToArray();
        }

        static DealEngine NewDeal(IReadOnlyList<Card>[] hands, Seat leader)
        {
            return new DealEngine(new ContractCall(ContractType.NoQueens), hands, leader);
        }

        [Test]
        public void HeartsMayBeLedFromTheStart()
        {
            // The hearts lead restriction belongs to the hearts contracts only.
            var deal = NewDeal(RoundRobin(), Seat.South);
            Assert.AreEqual(13, deal.LegalPlays().Count);
            Assert.IsTrue(deal.LegalPlays().Any(c => c.Suit == Suit.Hearts));
        }

        [Test]
        public void VoidInLedSuitMustDumpAQueen()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));

            // West has no clubs and holds the queen of diamonds; it has to go.
            CollectionAssert.AreEqual(new[] { C(Suit.Diamonds, Rank.Queen) }, deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Diamonds, Rank.Two)));
        }

        [Test]
        public void VoidWithoutAQueenDiscardsFreely()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.Queen));
            deal.Play(C(Suit.Hearts, Rank.Queen));
            deal.Play(C(Suit.Spades, Rank.Queen));

            // Second trick: West's queen is already gone, so any diamond will do.
            deal.Play(C(Suit.Clubs, Rank.Three));
            Assert.AreEqual(12, deal.LegalPlays().Count);
        }

        [Test]
        public void FollowerMustDropAQueenTheTableBeats()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ace));

            // West holds no club queen and follows freely.
            Assert.AreEqual(3, deal.LegalPlays().Count);
            deal.Play(C(Suit.Clubs, Rank.Three));

            // North's queen of clubs is beaten by the ace on the table, so it goes.
            CollectionAssert.AreEqual(new[] { C(Suit.Clubs, Rank.Queen) }, deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Clubs, Rank.Four)));
        }

        [Test]
        public void NoForcedQueenWhileItStillBeatsTheTable()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ten));
            deal.Play(C(Suit.Clubs, Rank.Jack));

            // The jack is the best club down and North's queen beats it, so North
            // keeps the whole club holding as options.
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Clubs, Rank.Four), C(Suit.Clubs, Rank.Eight), C(Suit.Clubs, Rank.Queen) },
                deal.LegalPlays());
        }

        [Test]
        public void DealEndsEarlyOnceAllFourQueensAreCaptured()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            // Three queens fall on the first club, forced.
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.Queen));
            deal.Play(C(Suit.Hearts, Rank.Queen));
            deal.Play(C(Suit.Spades, Rank.Queen));
            Assert.IsFalse(deal.IsComplete);

            // South leads the last queen and takes it home.
            deal.Play(C(Suit.Clubs, Rank.Queen));
            deal.Play(C(Suit.Diamonds, Rank.Two));
            deal.Play(C(Suit.Hearts, Rank.Two));
            var trick = deal.Play(C(Suit.Spades, Rank.Two));

            Assert.AreEqual(Seat.South, trick.Winner);
            Assert.IsTrue(deal.IsComplete);
            Assert.AreEqual(2, deal.History.Count);
            Assert.Throws<InvalidOperationException>(() => deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Clubs, Rank.Three)));
        }

        [Test]
        public void ScoringChargesOneHundredPerQueen()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.Queen));
            deal.Play(C(Suit.Hearts, Rank.Queen));
            deal.Play(C(Suit.Spades, Rank.Queen));
            deal.Play(C(Suit.Clubs, Rank.Queen));
            deal.Play(C(Suit.Diamonds, Rank.Two));
            deal.Play(C(Suit.Hearts, Rank.Two));
            deal.Play(C(Suit.Spades, Rank.Two));

            var score = deal.Score();
            CollectionAssert.AreEqual(new[] { 4, 0, 0, 0 }, score.UnitsTaken);
            CollectionAssert.AreEqual(new[] { -400, 0, 0, 0 }, score.Points);
            Assert.AreEqual(-400, score.Points.Sum());
        }

        [Test]
        public void GreedyPlaythroughAlwaysSumsToMinusFourHundred()
        {
            var deal = NewDeal(RoundRobin(), Seat.East);
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);

            var score = deal.Score();
            Assert.AreEqual(4, score.UnitsTaken.Sum());
            Assert.AreEqual(-400, score.Points.Sum());
            for (int s = 0; s < 4; s++)
                Assert.AreEqual(score.UnitsTaken[s] * -100, score.Points[s]);
        }
    }
}
