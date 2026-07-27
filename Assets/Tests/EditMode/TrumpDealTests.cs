using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class TrumpDealTests
    {
        static Card C(Suit s, Rank r) => new Card(s, r);

        static IEnumerable<Card> Run(Suit suit, Rank lo, Rank hi)
        {
            for (int r = (int)lo; r <= (int)hi; r++)
                yield return new Card(suit, (Rank)r);
        }

        // Deck dealt round-robin, so every seat can follow any lead. South's clubs come
        // out as 2,6,T,A; West's as 3,7,J; North's 4,8,Q; East's 5,9,K.
        static IReadOnlyList<Card>[] RoundRobin()
        {
            var deck = Deck.Standard();
            var lists = new[] { new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
            for (int i = 0; i < deck.Length; i++)
                lists[i % 4].Add(deck[i]);
            return lists.Select(l => (IReadOnlyList<Card>)l.ToArray()).ToArray();
        }

        // Built for hearts-as-trump scenarios: South holds every club, East every spade,
        // West the top three hearts plus low diamonds, North the rest of both red suits.
        // On a club lead West and North are void and hold trumps; East is void in both
        // clubs and trumps.
        static IReadOnlyList<Card>[] MixedHands()
        {
            return new IReadOnlyList<Card>[]
            {
                Run(Suit.Clubs, Rank.Two, Rank.Ace).ToArray(),
                Run(Suit.Hearts, Rank.Queen, Rank.Ace).Concat(Run(Suit.Diamonds, Rank.Two, Rank.Jack)).ToArray(),
                Run(Suit.Hearts, Rank.Two, Rank.Jack).Concat(Run(Suit.Diamonds, Rank.Queen, Rank.Ace)).ToArray(),
                Run(Suit.Spades, Rank.Two, Rank.Ace).ToArray(),
            };
        }

        static DealEngine NewTrumpDeal(IReadOnlyList<Card>[] hands, Seat leader, Suit trump = Suit.Hearts)
        {
            return new DealEngine(new ContractCall(ContractType.Trump, trump), hands, leader);
        }

        [Test]
        public void VoidInLedSuitMustPlayATrump()
        {
            var deal = NewTrumpDeal(MixedHands(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));

            // West has no clubs but holds three trumps; nothing else is legal.
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Hearts, Rank.Ace), C(Suit.Hearts, Rank.King), C(Suit.Hearts, Rank.Queen) },
                deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Diamonds, Rank.Two)));
        }

        [Test]
        public void NoObligationToBeatTrumpsAlreadyPlayed()
        {
            var deal = NewTrumpDeal(MixedHands(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Hearts, Rank.Ace));

            // The ace of trumps is on the table; North is still free to throw any trump under it.
            CollectionAssert.AreEquivalent(Run(Suit.Hearts, Rank.Two, Rank.Jack).ToArray(), deal.LegalPlays());
            Assert.IsNull(deal.Play(C(Suit.Hearts, Rank.Two)));
        }

        [Test]
        public void VoidInLedSuitAndTrumpsDiscardsFreely()
        {
            var deal = NewTrumpDeal(MixedHands(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ace));
            deal.Play(C(Suit.Hearts, Rank.Queen));
            deal.Play(C(Suit.Hearts, Rank.Two));

            // East has neither clubs nor hearts, so all thirteen spades are open.
            Assert.AreEqual(13, deal.LegalPlays().Count);
            Assert.IsTrue(deal.LegalPlays().All(c => c.Suit == Suit.Spades));
        }

        [Test]
        public void TrumpBeatsTheLedSuitAndHighestTrumpWins()
        {
            var deal = NewTrumpDeal(MixedHands(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ace));
            deal.Play(C(Suit.Hearts, Rank.Queen));
            deal.Play(C(Suit.Hearts, Rank.Two));
            var trick = deal.Play(C(Suit.Spades, Rank.Ace));

            // The led ace of clubs and the discarded ace of spades both lose to the
            // queen of trumps, which outranks the two of trumps.
            Assert.AreEqual(Seat.West, trick.Winner);
            Assert.AreEqual(Seat.West, deal.ToPlay);
        }

        [Test]
        public void MustStillFollowSuitEvenWhileHoldingTrumps()
        {
            var deal = NewTrumpDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));

            // West holds hearts, but has clubs, so following suit still rules.
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Clubs, Rank.Three), C(Suit.Clubs, Rank.Seven), C(Suit.Clubs, Rank.Jack) },
                deal.LegalPlays());
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Hearts, Rank.Five)));
        }

        [Test]
        public void HighestOfLedSuitWinsWhenNobodyTrumps()
        {
            var deal = NewTrumpDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Clubs, Rank.Three));
            deal.Play(C(Suit.Clubs, Rank.Four));
            var trick = deal.Play(C(Suit.Clubs, Rank.Five));
            Assert.AreEqual(Seat.East, trick.Winner);
        }

        [Test]
        public void LeadingTrumpsIsAlwaysAllowed()
        {
            var deal = NewTrumpDeal(RoundRobin(), Seat.South);
            Assert.AreEqual(13, deal.LegalPlays().Count);
            Assert.IsNull(deal.Play(C(Suit.Hearts, Rank.Four)));

            // A trump lead has to be followed with trumps like any other suit.
            Assert.IsTrue(deal.LegalPlays().All(c => c.Suit == Suit.Hearts));
        }

        [Test]
        public void ScoringPaysFiftyPerTrickAndSumsToTheDealTotal()
        {
            var deal = NewTrumpDeal(RoundRobin(), Seat.North, Suit.Spades);
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);

            Assert.AreEqual(13, deal.History.Count);
            var score = deal.Score();
            Assert.AreEqual(650, score.Points.Sum());
            Assert.AreEqual(13, score.UnitsTaken.Sum());
            for (int s = 0; s < 4; s++)
            {
                Assert.AreEqual(deal.History.Count(t => t.Winner == (Seat)s), score.UnitsTaken[s]);
                Assert.AreEqual(50 * score.UnitsTaken[s], score.Points[s]);
            }

            // Recompute every winner independently: highest trump if any fell, else
            // highest card of the led suit.
            foreach (var trick in deal.History)
            {
                var led = trick.Plays[0].Card.Suit;
                var trumps = trick.Plays.Where(p => p.Card.Suit == Suit.Spades).ToList();
                var contenders = trumps.Count > 0 ? trumps : trick.Plays.Where(p => p.Card.Suit == led).ToList();
                Assert.AreEqual(contenders.OrderByDescending(p => p.Card.Rank).First().Seat, trick.Winner);
            }
        }
    }
}
