using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class NoLastTwoTests
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

        // South runs twelve solid clubs and then loses the lead: everyone keeps one
        // card back for trick thirteen, where North's ace of hearts takes it.
        static IReadOnlyList<Card>[] TwelveAndOut()
        {
            var south = Run(Suit.Clubs, Rank.Three, Rank.Ace).ToList();
            south.Add(C(Suit.Hearts, Rank.Two));
            var west = Run(Suit.Diamonds, Rank.Three, Rank.Ace).ToList();
            west.Add(C(Suit.Hearts, Rank.Three));
            var north = Run(Suit.Hearts, Rank.Four, Rank.Ace).ToList();
            north.Add(C(Suit.Clubs, Rank.Two));
            north.Add(C(Suit.Spades, Rank.Two));
            var east = Run(Suit.Spades, Rank.Three, Rank.Ace).ToList();
            east.Add(C(Suit.Diamonds, Rank.Two));
            return new IReadOnlyList<Card>[] { south.ToArray(), west.ToArray(), north.ToArray(), east.ToArray() };
        }

        static DealEngine NewDeal(IReadOnlyList<Card>[] hands, Seat leader)
        {
            return new DealEngine(new ContractCall(ContractType.NoLastTwo), hands, leader);
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
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));
            Assert.AreEqual(13, deal.LegalPlays().Count);
        }

        [Test]
        public void SamePlayerTakingBothLastTricksPaysTheFullThreeSixty()
        {
            // South's clubs take every trick, the last two included.
            var deal = NewDeal(SuitPerSeat(), Seat.South);
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);

            Assert.AreEqual(13, deal.History.Count);
            var score = deal.Score();
            CollectionAssert.AreEqual(new[] { 2, 0, 0, 0 }, score.UnitsTaken);
            CollectionAssert.AreEqual(new[] { -360, 0, 0, 0 }, score.Points);
            Assert.AreEqual(-360, score.Points.Sum());
        }

        [Test]
        public void OnlyTricksTwelveAndThirteenScore()
        {
            var deal = NewDeal(TwelveAndOut(), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ace));
            deal.Play(C(Suit.Diamonds, Rank.Three));
            deal.Play(C(Suit.Clubs, Rank.Two)); // North's only club
            deal.Play(C(Suit.Spades, Rank.Three));

            // Eleven more club tricks for South; the others shed their side suits
            // and keep one card each for the last trick.
            for (int i = 0; i < 11; i++)
            {
                deal.Play(C(Suit.Clubs, (Rank)((int)Rank.King - i)));
                deal.Play(C(Suit.Diamonds, (Rank)((int)Rank.Four + i)));
                deal.Play(i < 10 ? C(Suit.Hearts, (Rank)((int)Rank.Four + i)) : C(Suit.Spades, Rank.Two));
                deal.Play(C(Suit.Spades, (Rank)((int)Rank.Four + i)));
            }
            Assert.AreEqual(12, deal.History.Count);
            Assert.AreEqual(Seat.South, deal.History[11].Winner);

            // Trick thirteen: the heart lead goes to North's ace.
            deal.Play(C(Suit.Hearts, Rank.Two));
            deal.Play(C(Suit.Hearts, Rank.Three));
            deal.Play(C(Suit.Hearts, Rank.Ace));
            var last = deal.Play(C(Suit.Diamonds, Rank.Two));

            Assert.AreEqual(Seat.North, last.Winner);
            Assert.IsTrue(deal.IsComplete);

            // South took twelve tricks but is only charged for the twelfth.
            var score = deal.Score();
            CollectionAssert.AreEqual(new[] { 1, 0, 1, 0 }, score.UnitsTaken);
            CollectionAssert.AreEqual(new[] { -180, 0, -180, 0 }, score.Points);
            Assert.AreEqual(-360, score.Points.Sum());
        }

        [Test]
        public void GreedyPlaythroughAlwaysSumsToMinusThreeSixty()
        {
            var deal = NewDeal(RoundRobin(), Seat.West);
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);

            Assert.AreEqual(13, deal.History.Count);
            var score = deal.Score();
            Assert.AreEqual(2, score.UnitsTaken.Sum());
            Assert.AreEqual(-360, score.Points.Sum());
            for (int s = 0; s < 4; s++)
                Assert.AreEqual(score.UnitsTaken[s] * -180, score.Points[s]);
        }
    }
}
