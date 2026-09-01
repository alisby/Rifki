using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class UserRuleTests
    {
        static Card C(Suit s, Rank r) => new Card(s, r);

        static void PlayOut(DealEngine deal)
        {
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);
        }

        static void RunDeal(Session session, ContractCall call)
        {
            PlayOut(session.StartDeal(call));
            session.FinishDeal();
        }

        static IEnumerable<Card> Run(Suit suit, Rank lo, Rank hi)
        {
            for (int r = (int)lo; r <= (int)hi; r++)
                yield return new Card(suit, (Rank)r);
        }

        static IReadOnlyList<Card>[] RoundRobin()
        {
            var deck = Deck.Standard();
            var lists = new[]
            {
                new List<Card>(),
                new List<Card>(),
                new List<Card>(),
                new List<Card>()
            };

            for (int i = 0; i < deck.Length; i++)
                lists[i % 4].Add(deck[i]);

            return lists
                .Select(x => (IReadOnlyList<Card>)x.ToArray())
                .ToArray();
        }

        static IReadOnlyList<Card>[] CallerHasTopThreeTrumps()
        {
            var deck = Deck.Standard().ToList();

            var south = new List<Card>
            {
                C(Suit.Hearts, Rank.Ace),
                C(Suit.Hearts, Rank.King),
                C(Suit.Hearts, Rank.Queen)
            };

            foreach (var c in south)
                deck.Remove(c);

            var sideCards = deck
                .Where(c => c.Suit != Suit.Hearts)
                .Take(10)
                .ToList();

            south.AddRange(sideCards);

            foreach (var c in sideCards)
                deck.Remove(c);

            return new IReadOnlyList<Card>[]
            {
                south.ToArray(),
                deck.Take(13).ToArray(),
                deck.Skip(13).Take(13).ToArray(),
                deck.Skip(26).Take(13).ToArray()
            };
        }

        static IReadOnlyList<Card>[] ForcedTrumpHands()
        {
            return new IReadOnlyList<Card>[]
            {
                Run(Suit.Clubs, Rank.Two, Rank.Ace).ToArray(),

                Run(Suit.Hearts, Rank.Queen, Rank.Ace)
                    .Concat(Run(Suit.Diamonds, Rank.Two, Rank.Jack))
                    .ToArray(),

                Run(Suit.Hearts, Rank.Two, Rank.Jack)
                    .Concat(Run(Suit.Diamonds, Rank.Queen, Rank.Ace))
                    .ToArray(),

                Run(Suit.Spades, Rank.Two, Rank.Ace).ToArray()
            };
        }

        [Test]
        public void FirstFourDealsCannotBeTrump()
        {
            var session = new Session(123);

            var firstFour = new[]
            {
                ContractType.NoTricks,
                ContractType.NoHearts,
                ContractType.NoQueens,
                ContractType.NoMen
            };

            for (int i = 0; i < 4; i++)
            {
                var available = session.AvailableContracts();

                CollectionAssert.DoesNotContain(
                    available,
                    ContractType.Trump,
                    "deal " + session.DealNumber);

                Assert.Throws<InvalidOperationException>(() =>
                    session.StartDeal(
                        new ContractCall(ContractType.Trump, Suit.Clubs)));

                RunDeal(session, new ContractCall(firstFour[i]));
            }

            Assert.AreEqual(5, session.DealNumber);
            CollectionAssert.Contains(
                session.AvailableContracts(),
                ContractType.Trump);
        }

        [Test]
        public void TrumpCannotBeLedBeforeBrokenWithoutAceKingQueen()
        {
            var deal = new DealEngine(
                new ContractCall(ContractType.Trump, Suit.Hearts),
                RoundRobin(),
                Seat.South);

            Assert.IsFalse(deal.TrumpBroken);

            Assert.IsFalse(
                deal.LegalPlays().Any(c => c.Suit == Suit.Hearts));
        }

        [Test]
        public void CallerWithAceKingQueenMayLeadTrumpImmediately()
        {
            var deal = new DealEngine(
                new ContractCall(ContractType.Trump, Suit.Hearts),
                CallerHasTopThreeTrumps(),
                Seat.South);

            Assert.IsFalse(deal.TrumpBroken);

            Assert.IsTrue(
                deal.LegalPlays().Any(c => c.Suit == Suit.Hearts));

            Assert.DoesNotThrow(() =>
                deal.Play(C(Suit.Hearts, Rank.Ace)));
        }

        [Test]
        public void ForcedTrumpBreaksTrumpForFollowingLeads()
        {
            var deal = new DealEngine(
                new ContractCall(ContractType.Trump, Suit.Hearts),
                ForcedTrumpHands(),
                Seat.South);

            deal.Play(C(Suit.Clubs, Rank.Ace));
            deal.Play(C(Suit.Hearts, Rank.Queen));
            deal.Play(C(Suit.Hearts, Rank.Two));
            deal.Play(C(Suit.Spades, Rank.Ace));

            Assert.IsTrue(deal.TrumpBroken);
            Assert.AreEqual(Seat.West, deal.ToPlay);

            Assert.IsTrue(
                deal.LegalPlays().Any(c => c.Suit == Suit.Hearts));
        }
    }
}
