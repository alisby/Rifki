using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class NoHeartsTests
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

        // Hearts split 4-4-4-1 so they can all fall inside five tricks. South holds
        // K,Q,J,T of hearts plus low clubs; West A,9,8,7 plus low diamonds; North
        // 6,5,4,3 plus low spades; East just the 2 of hearts plus every J,Q,K,A
        // outside hearts.
        static IReadOnlyList<Card>[] SplitHearts()
        {
            var south = new List<Card> { C(Suit.Hearts, Rank.King), C(Suit.Hearts, Rank.Queen), C(Suit.Hearts, Rank.Jack), C(Suit.Hearts, Rank.Ten) };
            south.AddRange(Run(Suit.Clubs, Rank.Two, Rank.Ten));
            var west = new List<Card> { C(Suit.Hearts, Rank.Ace), C(Suit.Hearts, Rank.Nine), C(Suit.Hearts, Rank.Eight), C(Suit.Hearts, Rank.Seven) };
            west.AddRange(Run(Suit.Diamonds, Rank.Two, Rank.Ten));
            var north = new List<Card> { C(Suit.Hearts, Rank.Six), C(Suit.Hearts, Rank.Five), C(Suit.Hearts, Rank.Four), C(Suit.Hearts, Rank.Three) };
            north.AddRange(Run(Suit.Spades, Rank.Two, Rank.Ten));
            var east = new List<Card> { C(Suit.Hearts, Rank.Two) };
            foreach (var suit in new[] { Suit.Clubs, Suit.Diamonds, Suit.Spades })
                east.AddRange(Run(suit, Rank.Jack, Rank.Ace));
            return new IReadOnlyList<Card>[] { south.ToArray(), west.ToArray(), north.ToArray(), east.ToArray() };
        }

        static DealEngine NewDeal(IReadOnlyList<Card>[] hands, Seat leader)
        {
            return new DealEngine(new ContractCall(ContractType.NoHearts), hands, leader);
        }

        static void Script(DealEngine deal, params Card[] cards)
        {
            foreach (var card in cards)
                deal.Play(card);
        }

        [Test]
        public void CannotLeadAHeartBeforeTheyAreBroken()
        {
            var deal = NewDeal(SplitHearts(), Seat.South);
            Assert.IsTrue(deal.LegalPlays().All(c => c.Suit == Suit.Clubs));
            Assert.AreEqual(9, deal.LegalPlays().Count);
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Hearts, Rank.King)));
        }

        [Test]
        public void MayLeadHeartsOnceBroken()
        {
            var deal = NewDeal(SplitHearts(), Seat.South);
            // West and North dump hearts on the club lead, breaking them; East's jack
            // takes the trick.
            Script(deal,
                C(Suit.Clubs, Rank.Ten), C(Suit.Hearts, Rank.Nine), C(Suit.Hearts, Rank.Three), C(Suit.Clubs, Rank.Jack));

            Assert.IsTrue(deal.HeartsBroken);
            Assert.AreEqual(Seat.East, deal.ToPlay);
            // Whole remaining hand is open, the heart included.
            CollectionAssert.Contains(deal.LegalPlays(), C(Suit.Hearts, Rank.Two));
            Assert.AreEqual(12, deal.LegalPlays().Count);
        }

        [Test]
        public void MayLeadHeartsUnbrokenWhenHoldingNothingElse()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.North);
            Assert.IsFalse(deal.HeartsBroken);
            Assert.AreEqual(13, deal.LegalPlays().Count);
            Assert.IsNull(deal.Play(C(Suit.Hearts, Rank.Two)));
        }

        [Test]
        public void VoidInLedSuitMustDumpAHeartIfHeld()
        {
            var deal = NewDeal(SplitHearts(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ten));

            // West has no clubs and four hearts; only the hearts are playable.
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Hearts, Rank.Ace), C(Suit.Hearts, Rank.Nine), C(Suit.Hearts, Rank.Eight), C(Suit.Hearts, Rank.Seven) },
                deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Diamonds, Rank.Two)));
        }

        [Test]
        public void VoidWithoutHeartsDiscardsFreely()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            Assert.AreEqual(13, deal.LegalPlays().Count); // all of West's diamonds
        }

        [Test]
        public void FollowerMustDropAHeartTheTableAlreadyBeats()
        {
            var deal = NewDeal(SplitHearts(), Seat.South);
            Script(deal,
                C(Suit.Clubs, Rank.Ten), C(Suit.Hearts, Rank.Nine), C(Suit.Hearts, Rank.Three), C(Suit.Clubs, Rank.Jack));

            // East leads the two of hearts; South's hearts all beat it, so South
            // follows freely.
            deal.Play(C(Suit.Hearts, Rank.Two));
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Hearts, Rank.King), C(Suit.Hearts, Rank.Queen), C(Suit.Hearts, Rank.Jack), C(Suit.Hearts, Rank.Ten) },
                deal.LegalPlays());

            // The king goes down; West's 8 and 7 are now beaten, so one of them has
            // to go and the ace can't be played over it.
            deal.Play(C(Suit.Hearts, Rank.King));
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Hearts, Rank.Eight), C(Suit.Hearts, Rank.Seven) },
                deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Hearts, Rank.Ace)));
        }

        // Plays the split-hearts deal to its early end: all thirteen hearts are gone
        // after five tricks.
        static DealEngine PlayOutSplitHearts()
        {
            var deal = NewDeal(SplitHearts(), Seat.South);
            Script(deal,
                C(Suit.Clubs, Rank.Ten), C(Suit.Hearts, Rank.Nine), C(Suit.Hearts, Rank.Three), C(Suit.Clubs, Rank.Jack),
                C(Suit.Hearts, Rank.Two), C(Suit.Hearts, Rank.King), C(Suit.Hearts, Rank.Seven), C(Suit.Hearts, Rank.Four),
                C(Suit.Hearts, Rank.Queen), C(Suit.Hearts, Rank.Eight), C(Suit.Hearts, Rank.Five), C(Suit.Diamonds, Rank.Jack),
                C(Suit.Hearts, Rank.Jack), C(Suit.Hearts, Rank.Ace), C(Suit.Hearts, Rank.Six), C(Suit.Diamonds, Rank.Queen),
                C(Suit.Diamonds, Rank.Two), C(Suit.Spades, Rank.Two), C(Suit.Diamonds, Rank.King), C(Suit.Hearts, Rank.Ten));
            return deal;
        }

        [Test]
        public void DealEndsEarlyOnceEveryHeartIsCaptured()
        {
            var deal = PlayOutSplitHearts();

            Assert.IsTrue(deal.IsComplete);
            Assert.AreEqual(5, deal.History.Count);
            Assert.AreEqual(5, deal.TrickNumber);
            Assert.Throws<InvalidOperationException>(() => deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Clubs, Rank.Two)));

            // All thirteen hearts really are in the history.
            var hearts = deal.History.SelectMany(t => t.Plays)
                                     .Count(p => p.Card.Suit == Suit.Hearts);
            Assert.AreEqual(13, hearts);
        }

        [Test]
        public void ScoringChargesThirtyPerCapturedHeart()
        {
            var deal = PlayOutSplitHearts();
            var score = deal.Score();

            // East took tricks 1 and 5 (three hearts), South tricks 2 and 3 (seven),
            // West trick 4 (three).
            CollectionAssert.AreEqual(new[] { 7, 3, 0, 3 }, score.UnitsTaken);
            CollectionAssert.AreEqual(new[] { -210, -90, 0, -90 }, score.Points);
            Assert.AreEqual(-390, score.Points.Sum());
        }

        [Test]
        public void FullDealScoringSumsToTheDealTotal()
        {
            // South leads a club every trick and wins them all; North is forced to
            // dump a heart on each, so South ends up owning all thirteen.
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);

            Assert.AreEqual(13, deal.History.Count);
            var score = deal.Score();
            CollectionAssert.AreEqual(new[] { 13, 0, 0, 0 }, score.UnitsTaken);
            CollectionAssert.AreEqual(new[] { -390, 0, 0, 0 }, score.Points);
            Assert.AreEqual(-390, score.Points.Sum());
        }
    }
}
