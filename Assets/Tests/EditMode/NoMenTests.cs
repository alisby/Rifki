using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class NoMenTests
    {
        static Card C(Suit s, Rank r) => new Card(s, r);

        static IEnumerable<Card> Run(Suit suit, Rank lo, Rank hi)
        {
            for (int r = (int)lo; r <= (int)hi; r++)
                yield return new Card(suit, (Rank)r);
        }

        // Every seat holds one whole suit: South clubs, West diamonds, North hearts, East spades.
        static IReadOnlyList<Card>[] SuitPerSeat()
        {
            var hands = new IReadOnlyList<Card>[4];
            for (int s = 0; s < 4; s++)
                hands[s] = Deck.Standard().Where(c => (int)c.Suit == s).ToArray();
            return hands;
        }

        // Deck dealt round-robin, so every seat can follow any lead.
        static IReadOnlyList<Card>[] RoundRobin()
        {
            var deck = Deck.Standard();
            var lists = new[] { new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
            for (int i = 0; i < deck.Length; i++)
                lists[i % 4].Add(deck[i]);
            return lists.Select(l => (IReadOnlyList<Card>)l.ToArray()).ToArray();
        }

        // West sits with both the king and jack of clubs behind South's ace, so one
        // forced dump has to pick between them.
        static IReadOnlyList<Card>[] BothMenBehindTheAce()
        {
            var south = new List<Card> { C(Suit.Clubs, Rank.Ace), C(Suit.Clubs, Rank.Queen) };
            south.AddRange(Run(Suit.Diamonds, Rank.Two, Rank.Queen));
            var west = new List<Card> { C(Suit.Clubs, Rank.King), C(Suit.Clubs, Rank.Jack) };
            west.AddRange(Run(Suit.Spades, Rank.Two, Rank.Queen));
            var north = Run(Suit.Clubs, Rank.Two, Rank.Ten).Concat(Run(Suit.Hearts, Rank.Two, Rank.Five)).ToList();
            var east = new List<Card> { C(Suit.Diamonds, Rank.King), C(Suit.Diamonds, Rank.Ace), C(Suit.Spades, Rank.King), C(Suit.Spades, Rank.Ace) };
            east.AddRange(Run(Suit.Hearts, Rank.Six, Rank.Ace));
            return new IReadOnlyList<Card>[] { south.ToArray(), west.ToArray(), north.ToArray(), east.ToArray() };
        }

        static DealEngine NewDeal(IReadOnlyList<Card>[] hands, Seat leader)
        {
            return new DealEngine(new ContractCall(ContractType.NoMen), hands, leader);
        }

        [Test]
        public void VoidInLedSuitMustDumpAMan()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));

            // West is void and holds two men; either goes, nothing else does.
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Diamonds, Rank.King), C(Suit.Diamonds, Rank.Jack) },
                deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Diamonds, Rank.Two)));
        }

        [Test]
        public void FollowerMustDropAManTheTableBeats()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ace));

            // West's jack of clubs is beaten by the ace; the jack goes.
            CollectionAssert.AreEqual(new[] { C(Suit.Clubs, Rank.Jack) }, deal.LegalPlays());
            deal.Play(C(Suit.Clubs, Rank.Jack));

            // North holds no club man and follows freely; the queen is safe here.
            Assert.AreEqual(3, deal.LegalPlays().Count);
            deal.Play(C(Suit.Clubs, Rank.Four));

            // East's king of clubs is beaten too.
            CollectionAssert.AreEqual(new[] { C(Suit.Clubs, Rank.King) }, deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Clubs, Rank.Five)));
        }

        [Test]
        public void NoForcedManWhileItStillBeatsTheTable()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ten));

            // The ten doesn't beat West's jack, so West keeps the choice.
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Clubs, Rank.Three), C(Suit.Clubs, Rank.Seven), C(Suit.Clubs, Rank.Jack) },
                deal.LegalPlays());
        }

        [Test]
        public void BothMenBeatenLeavesTheChoiceBetweenThem()
        {
            var deal = NewDeal(BothMenBehindTheAce(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ace));

            // King and jack are both under the ace; West picks which one to lose.
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Clubs, Rank.King), C(Suit.Clubs, Rank.Jack) },
                deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Spades, Rank.Two)));
        }

        // Four scripted tricks that hoover up all eight men: the off-suit kings and
        // jacks are dumped on South's low clubs, then South cashes its own two.
        static DealEngine PlayOutAllMen()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.King));
            deal.Play(C(Suit.Hearts, Rank.King));
            deal.Play(C(Suit.Spades, Rank.King));

            deal.Play(C(Suit.Clubs, Rank.Three));
            deal.Play(C(Suit.Diamonds, Rank.Jack));
            deal.Play(C(Suit.Hearts, Rank.Jack));
            deal.Play(C(Suit.Spades, Rank.Jack));

            deal.Play(C(Suit.Clubs, Rank.King));
            deal.Play(C(Suit.Diamonds, Rank.Two));
            deal.Play(C(Suit.Hearts, Rank.Two));
            deal.Play(C(Suit.Spades, Rank.Two));

            deal.Play(C(Suit.Clubs, Rank.Jack));
            deal.Play(C(Suit.Diamonds, Rank.Three));
            deal.Play(C(Suit.Hearts, Rank.Three));
            deal.Play(C(Suit.Spades, Rank.Three));
            return deal;
        }

        [Test]
        public void DealEndsEarlyOnceAllEightMenAreCaptured()
        {
            var deal = PlayOutAllMen();

            Assert.IsTrue(deal.IsComplete);
            Assert.AreEqual(4, deal.History.Count);
            Assert.Throws<InvalidOperationException>(() => deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Clubs, Rank.Four)));

            var men = deal.History.SelectMany(t => t.Plays)
                                  .Count(p => p.Card.Rank == Rank.King || p.Card.Rank == Rank.Jack);
            Assert.AreEqual(8, men);
        }

        [Test]
        public void ScoringChargesSixtyPerMan()
        {
            var deal = PlayOutAllMen();
            var score = deal.Score();

            CollectionAssert.AreEqual(new[] { 8, 0, 0, 0 }, score.UnitsTaken);
            CollectionAssert.AreEqual(new[] { -480, 0, 0, 0 }, score.Points);
            Assert.AreEqual(-480, score.Points.Sum());
        }

        [Test]
        public void GreedyPlaythroughAlwaysSumsToMinusFourEighty()
        {
            var deal = NewDeal(RoundRobin(), Seat.West);
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);

            var score = deal.Score();
            Assert.AreEqual(8, score.UnitsTaken.Sum());
            Assert.AreEqual(-480, score.Points.Sum());
            for (int s = 0; s < 4; s++)
                Assert.AreEqual(score.UnitsTaken[s] * -60, score.Points[s]);
        }
    }
}
