using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class NoTricksTests
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

        // Deck dealt round-robin, so every seat can follow any lead.
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
            return new DealEngine(new ContractCall(ContractType.NoTricks), hands, leader);
        }

        [Test]
        public void HeartsMayBeLedFromTheStart()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            Assert.AreEqual(13, deal.LegalPlays().Count);
            Assert.IsTrue(deal.LegalPlays().Any(c => c.Suit == Suit.Hearts));
        }

        [Test]
        public void VoidHandsDiscardFreely()
        {
            // West holds queens, kings and jacks, but there is no dumping duty in
            // this contract; the whole hand is open.
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            Assert.AreEqual(13, deal.LegalPlays().Count);
        }

        [Test]
        public void FollowersAreNeverForcedToAParticularCard()
        {
            // The ace is down and West holds the club jack, yet any club is fine.
            var deal = NewDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ace));
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Clubs, Rank.Three), C(Suit.Clubs, Rank.Seven), C(Suit.Clubs, Rank.Jack) },
                deal.LegalPlays());
        }

        [Test]
        public void HighestCardOfTheLedSuitTakesTheTrick()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Six));
            deal.Play(C(Suit.Clubs, Rank.Seven));
            deal.Play(C(Suit.Clubs, Rank.Eight));
            var trick = deal.Play(C(Suit.Clubs, Rank.Nine));

            Assert.AreEqual(Seat.East, trick.Winner);
            Assert.AreEqual(Seat.East, deal.ToPlay);
        }

        [Test]
        public void EveryTrickChargesItsWinnerFifty()
        {
            // South's solid clubs win all thirteen tricks; nothing ends this deal early.
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);

            Assert.AreEqual(13, deal.History.Count);
            var score = deal.Score();
            CollectionAssert.AreEqual(new[] { 13, 0, 0, 0 }, score.UnitsTaken);
            CollectionAssert.AreEqual(new[] { -650, 0, 0, 0 }, score.Points);
            Assert.AreEqual(-650, score.Points.Sum());
        }

        [Test]
        public void GreedyPlaythroughAlwaysSumsToMinusSixFifty()
        {
            var deal = NewDeal(RoundRobin(), Seat.North);
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);

            Assert.AreEqual(13, deal.History.Count);
            var score = deal.Score();
            Assert.AreEqual(13, score.UnitsTaken.Sum());
            Assert.AreEqual(-650, score.Points.Sum());
            for (int s = 0; s < 4; s++)
                Assert.AreEqual(score.UnitsTaken[s] * -50, score.Points[s]);
        }
    }
}
