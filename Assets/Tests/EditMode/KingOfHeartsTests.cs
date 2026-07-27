using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class KingOfHeartsTests
    {
        static Card C(Suit s, Rank r) => new Card(s, r);

        static IEnumerable<Card> Run(Suit suit, Rank lo, Rank hi)
        {
            for (int r = (int)lo; r <= (int)hi; r++)
                yield return new Card(suit, (Rank)r);
        }

        // Every seat holds one whole suit: South clubs, West diamonds, North hearts
        // (the king included), East spades.
        static IReadOnlyList<Card>[] SuitPerSeat()
        {
            var hands = new IReadOnlyList<Card>[4];
            for (int s = 0; s < 4; s++)
                hands[s] = Deck.Standard().Where(c => (int)c.Suit == s).ToArray();
            return hands;
        }

        // Deck dealt round-robin, so every seat can follow any lead. The king of
        // hearts lands with West.
        static IReadOnlyList<Card>[] RoundRobin()
        {
            var deck = Deck.Standard();
            var lists = new[] { new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
            for (int i = 0; i < deck.Length; i++)
                lists[i % 4].Add(deck[i]);
            return lists.Select(l => (IReadOnlyList<Card>)l.ToArray()).ToArray();
        }

        // Built so South can break hearts on trick one and then lead them: South
        // holds both red aces and the queen of hearts, West the king and jack of
        // hearts behind long diamonds, North the low hearts.
        static IReadOnlyList<Card>[] KingBehindTheAce()
        {
            var south = new List<Card> { C(Suit.Hearts, Rank.Ace), C(Suit.Hearts, Rank.Queen), C(Suit.Diamonds, Rank.Ace) };
            south.AddRange(Run(Suit.Clubs, Rank.Two, Rank.Jack));
            var west = new List<Card> { C(Suit.Hearts, Rank.King), C(Suit.Hearts, Rank.Jack) };
            west.AddRange(Run(Suit.Diamonds, Rank.Two, Rank.Queen));
            var north = Run(Suit.Hearts, Rank.Two, Rank.Ten).Concat(Run(Suit.Spades, Rank.Two, Rank.Five)).ToList();
            var east = new List<Card> { C(Suit.Diamonds, Rank.King) };
            east.AddRange(Run(Suit.Clubs, Rank.Queen, Rank.Ace));
            east.AddRange(Run(Suit.Spades, Rank.Six, Rank.Ace));
            return new IReadOnlyList<Card>[] { south.ToArray(), west.ToArray(), north.ToArray(), east.ToArray() };
        }

        static DealEngine NewDeal(IReadOnlyList<Card>[] hands, Seat leader)
        {
            return new DealEngine(new ContractCall(ContractType.KingOfHearts), hands, leader);
        }

        [Test]
        public void CannotLeadAHeartBeforeTheyAreBroken()
        {
            var deal = NewDeal(KingBehindTheAce(), Seat.South);
            Assert.AreEqual(11, deal.LegalPlays().Count);
            Assert.IsTrue(deal.LegalPlays().All(c => c.Suit != Suit.Hearts));
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Hearts, Rank.Ace)));
        }

        [Test]
        public void MayLeadHeartsUnbrokenWhenHoldingNothingElse()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.North);
            Assert.AreEqual(13, deal.LegalPlays().Count);
        }

        [Test]
        public void VoidInLedSuitMustDumpTheKing()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));

            // West is void too but doesn't hold the king, so West discards freely.
            Assert.AreEqual(13, deal.LegalPlays().Count);
            deal.Play(C(Suit.Diamonds, Rank.Two));

            // North holds the king, and nothing else in the hand is legal.
            CollectionAssert.AreEqual(new[] { C(Suit.Hearts, Rank.King) }, deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Hearts, Rank.Two)));
        }

        [Test]
        public void HeartsLedWithTheAceDownForcesTheKing()
        {
            var deal = NewDeal(KingBehindTheAce(), Seat.South);
            // North discards a heart on the diamond lead, breaking hearts; South's
            // ace holds the trick.
            deal.Play(C(Suit.Diamonds, Rank.Ace));
            deal.Play(C(Suit.Diamonds, Rank.Two));
            deal.Play(C(Suit.Hearts, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.King));
            Assert.IsTrue(deal.HeartsBroken);

            // The ace of hearts is led; West must follow and the beaten king goes.
            deal.Play(C(Suit.Hearts, Rank.Ace));
            CollectionAssert.AreEqual(new[] { C(Suit.Hearts, Rank.King) }, deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Hearts, Rank.Jack)));
        }

        [Test]
        public void NoForcedKingWhileTheAceIsStillOut()
        {
            var deal = NewDeal(KingBehindTheAce(), Seat.South);
            deal.Play(C(Suit.Diamonds, Rank.Ace));
            deal.Play(C(Suit.Diamonds, Rank.Two));
            deal.Play(C(Suit.Hearts, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.King));

            // A queen lead doesn't beat the king, so West keeps the choice.
            deal.Play(C(Suit.Hearts, Rank.Queen));
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Hearts, Rank.King), C(Suit.Hearts, Rank.Jack) },
                deal.LegalPlays());
        }

        [Test]
        public void DealEndsTheMomentTheKingIsCaptured()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.Two));
            deal.Play(C(Suit.Hearts, Rank.King)); // forced
            Assert.IsFalse(deal.IsComplete);
            var trick = deal.Play(C(Suit.Spades, Rank.Two));

            Assert.AreEqual(Seat.South, trick.Winner);
            Assert.IsTrue(deal.IsComplete);
            Assert.AreEqual(1, deal.History.Count);
            Assert.Throws<InvalidOperationException>(() => deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Clubs, Rank.Three)));
        }

        [Test]
        public void CaptorPaysThreeHundredTwenty()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.Two));
            deal.Play(C(Suit.Hearts, Rank.King));
            deal.Play(C(Suit.Spades, Rank.Two));

            var score = deal.Score();
            CollectionAssert.AreEqual(new[] { 1, 0, 0, 0 }, score.UnitsTaken);
            CollectionAssert.AreEqual(new[] { -320, 0, 0, 0 }, score.Points);
            Assert.AreEqual(-320, score.Points.Sum());
        }

        [Test]
        public void GreedyPlaythroughAlwaysEndsWithTheKingCaptured()
        {
            var deal = NewDeal(RoundRobin(), Seat.East);
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);

            var king = C(Suit.Hearts, Rank.King);
            var lastTrick = deal.History[deal.History.Count - 1];
            Assert.IsTrue(lastTrick.Plays.Any(p => p.Card == king));

            var score = deal.Score();
            Assert.AreEqual(-320, score.Points.Sum());
            Assert.AreEqual(1, score.UnitsTaken.Sum());
            Assert.AreEqual(1, score.UnitsTaken[(int)lastTrick.Winner]);
        }
    }
}
