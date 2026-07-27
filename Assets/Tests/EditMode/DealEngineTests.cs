using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class DealEngineTests
    {
        // Every seat holds one whole suit: South clubs, West diamonds, North hearts, East spades.
        static IReadOnlyList<Card>[] SuitPerSeat()
        {
            var hands = new IReadOnlyList<Card>[4];
            for (int s = 0; s < 4; s++)
                hands[s] = Deck.Standard().Where(c => (int)c.Suit == s).ToArray();
            return hands;
        }

        // Deck dealt round-robin, so every seat holds three or four cards of every suit.
        // South's clubs come out as 2,6,T,A; West's as 3,7,J; North's 4,8,Q; East's 5,9,K.
        static IReadOnlyList<Card>[] RoundRobin()
        {
            var deck = Deck.Standard();
            var lists = new[] { new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
            for (int i = 0; i < deck.Length; i++)
                lists[i % 4].Add(deck[i]);
            return lists.Select(l => (IReadOnlyList<Card>)l.ToArray()).ToArray();
        }

        static DealEngine NewDeal(IReadOnlyList<Card>[] hands, Seat leader, ContractType type = ContractType.NoTricks)
        {
            return new DealEngine(new ContractCall(type), hands, leader);
        }

        static Card C(Suit s, Rank r) => new Card(s, r);

        [Test]
        public void ConstructorRejectsMalformedHands()
        {
            var good = SuitPerSeat();
            Assert.Throws<ArgumentNullException>(() => NewDeal(null, Seat.South));
            Assert.Throws<ArgumentException>(() => NewDeal(good.Take(3).ToArray(), Seat.South));

            var shortHand = (IReadOnlyList<Card>[])good.Clone();
            shortHand[2] = good[2].Take(12).ToArray();
            Assert.Throws<ArgumentException>(() => NewDeal(shortHand, Seat.South));

            var duplicated = (IReadOnlyList<Card>[])good.Clone();
            var west = good[1].ToArray();
            west[0] = good[0][0]; // a club now sits in both South's and West's hand
            duplicated[1] = west;
            Assert.Throws<ArgumentException>(() => NewDeal(duplicated, Seat.South));
        }

        [Test]
        public void CallerLeadsAndPlayRotatesClockwise()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            Assert.AreEqual(Seat.South, deal.ToPlay);
            Assert.AreEqual(1, deal.TrickNumber);

            Assert.IsNull(deal.Play(C(Suit.Clubs, Rank.Two)));
            Assert.AreEqual(Seat.West, deal.ToPlay);
            Assert.IsNull(deal.Play(C(Suit.Clubs, Rank.Three)));
            Assert.AreEqual(Seat.North, deal.ToPlay);
            Assert.IsNull(deal.Play(C(Suit.Clubs, Rank.Four)));
            Assert.AreEqual(Seat.East, deal.ToPlay);

            var trick = deal.Play(C(Suit.Clubs, Rank.Five));
            Assert.IsNotNull(trick);
            Assert.AreEqual(Seat.East, trick.Winner);
            Assert.AreEqual(Seat.East, deal.ToPlay); // winner leads the next trick
            Assert.AreEqual(2, deal.TrickNumber);
        }

        [Test]
        public void RotationWrapsFromEastToSouth()
        {
            var deal = NewDeal(RoundRobin(), Seat.East);
            Assert.AreEqual(Seat.East, deal.ToPlay);
            deal.Play(C(Suit.Clubs, Rank.Five));
            Assert.AreEqual(Seat.South, deal.ToPlay);
        }

        [Test]
        public void HighestOfLedSuitWinsTheTrick()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Six));
            deal.Play(C(Suit.Clubs, Rank.Jack));
            deal.Play(C(Suit.Clubs, Rank.Four));
            var trick = deal.Play(C(Suit.Clubs, Rank.King));

            Assert.AreEqual(Seat.East, trick.Winner);
            Assert.AreEqual(Seat.South, trick.Leader);
            Assert.AreEqual(1, trick.Number);
            Assert.AreEqual(4, trick.Plays.Count);
            Assert.AreEqual((Seat.West, C(Suit.Clubs, Rank.Jack)), trick.Plays[1]);
        }

        [Test]
        public void DiscardsNeverWinTheTrick()
        {
            // Only South can follow the club lead; the three aces thrown on it don't count.
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.Ace));
            deal.Play(C(Suit.Hearts, Rank.Ace));
            var trick = deal.Play(C(Suit.Spades, Rank.Ace));
            Assert.AreEqual(Seat.South, trick.Winner);
        }

        [Test]
        public void MustFollowSuitWhenAble()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));

            var legal = deal.LegalPlays();
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Clubs, Rank.Three), C(Suit.Clubs, Rank.Seven), C(Suit.Clubs, Rank.Jack) },
                legal);

            // West holds diamonds but has clubs, so a diamond is illegal.
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Diamonds, Rank.Two)));
            // A card West doesn't even hold is illegal too.
            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Clubs, Rank.Four)));

            // The failed attempts changed nothing.
            Assert.AreEqual(Seat.West, deal.ToPlay);
            Assert.AreEqual(13, deal.HandOf(Seat.West).Count);
        }

        [Test]
        public void VoidInLedSuitMayPlayAnything()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Nine));
            Assert.AreEqual(13, deal.LegalPlays().Count); // all of West's diamonds
        }

        [Test]
        public void CurrentTrickAndHandsTrackThePlays()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            Assert.AreEqual(0, deal.CurrentTrick.Count);

            deal.Play(C(Suit.Clubs, Rank.Ace));
            deal.Play(C(Suit.Clubs, Rank.Three));
            Assert.AreEqual(2, deal.CurrentTrick.Count);
            Assert.AreEqual((Seat.South, C(Suit.Clubs, Rank.Ace)), deal.CurrentTrick[0]);
            Assert.AreEqual(0, deal.History.Count);

            var south = deal.HandOf(Seat.South);
            Assert.AreEqual(12, south.Count);
            CollectionAssert.DoesNotContain(south.ToArray(), C(Suit.Clubs, Rank.Ace));

            deal.Play(C(Suit.Clubs, Rank.Four));
            var trick = deal.Play(C(Suit.Clubs, Rank.Five));
            Assert.AreEqual(Seat.South, trick.Winner); // the ace led and held
            Assert.AreEqual(0, deal.CurrentTrick.Count);
            Assert.AreEqual(1, deal.History.Count);
            Assert.AreSame(trick, deal.History[0]);
        }

        [Test]
        public void HeartDiscardOnAnotherSuitBreaksHearts()
        {
            var deal = NewDeal(SuitPerSeat(), Seat.South, ContractType.NoHearts);
            Assert.IsFalse(deal.HeartsBroken);
            deal.Play(C(Suit.Clubs, Rank.Two));
            deal.Play(C(Suit.Diamonds, Rank.Two));
            Assert.IsFalse(deal.HeartsBroken);
            deal.Play(C(Suit.Hearts, Rank.Two)); // North is void in clubs and dumps a heart
            Assert.IsTrue(deal.HeartsBroken);
        }

        [Test]
        public void ScoreThrowsWhileTheDealIsInProgress()
        {
            var deal = NewDeal(RoundRobin(), Seat.South);
            Assert.Throws<InvalidOperationException>(() => deal.Score());
        }

        [Test]
        public void FullPlaythroughRunsThirteenTricksAndEmptiesEveryHand()
        {
            var deal = NewDeal(RoundRobin(), Seat.West);

            while (!deal.IsComplete)
            {
                var before = deal.ToPlay;
                var trick = deal.Play(deal.LegalPlays()[0]);
                if (trick == null)
                    Assert.AreEqual((Seat)(((int)before + 1) % 4), deal.ToPlay);
            }

            Assert.AreEqual(13, deal.History.Count);
            Assert.AreEqual(13, deal.TrickNumber);
            for (int s = 0; s < 4; s++)
                Assert.AreEqual(0, deal.HandOf((Seat)s).Count);

            // Every card was played exactly once.
            var played = deal.History.SelectMany(t => t.Plays.Select(p => p.Card)).ToList();
            Assert.AreEqual(52, played.Count);
            Assert.AreEqual(52, new HashSet<Card>(played).Count);

            // Each winner led the next trick, and each trick numbers itself in order.
            for (int i = 0; i < 13; i++)
            {
                var trick = deal.History[i];
                Assert.AreEqual(i + 1, trick.Number);
                Assert.AreEqual(trick.Leader, trick.Plays[0].Seat);
                if (i > 0)
                    Assert.AreEqual(deal.History[i - 1].Winner, trick.Leader);

                // Recompute the winner independently: highest rank among led-suit plays.
                var led = trick.Plays[0].Card.Suit;
                var expected = trick.Plays.Where(p => p.Card.Suit == led)
                                          .OrderByDescending(p => p.Card.Rank)
                                          .First().Seat;
                Assert.AreEqual(expected, trick.Winner);
            }

            Assert.Throws<InvalidOperationException>(() => deal.Play(C(Suit.Clubs, Rank.Two)));
            Assert.Throws<InvalidOperationException>(() => deal.LegalPlays());
        }
    }
}
