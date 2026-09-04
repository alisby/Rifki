using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using King.Core;
using King.UI;
using NUnit.Framework;

namespace King.Tests
{
    public sealed class RuleBehaviorTests
    {
        static Card C(Suit suit, Rank rank) => new Card(suit, rank);

        static IReadOnlyList<Card>[] MakeHands(
            IEnumerable<Card> south,
            IEnumerable<Card> west,
            IEnumerable<Card> north,
            IEnumerable<Card> east)
        {
            var lists = new[]
            {
                new List<Card>(south),
                new List<Card>(west),
                new List<Card>(north),
                new List<Card>(east)
            };

            var used = new HashSet<Card>();
            foreach (var list in lists)
                foreach (var card in list)
                    Assert.That(used.Add(card), Is.True, "Duplicate required card: " + card);

            var remaining = new Queue<Card>(
                Deck.Standard().Where(card => !used.Contains(card)));

            for (int seat = 0; seat < 4; seat++)
                while (lists[seat].Count < 13)
                    lists[seat].Add(remaining.Dequeue());

            return new IReadOnlyList<Card>[]
            {
                lists[0], lists[1], lists[2], lists[3]
            };
        }

        static IReadOnlyList<Card> RifkiHand(params Card[] hearts)
        {
            var hand = new List<Card>(hearts);
            hand.AddRange(
                Deck.Standard()
                    .Where(card => card.Suit != Suit.Hearts)
                    .Take(13 - hand.Count));
            return hand;
        }

        static bool CanBreakRifki(IReadOnlyList<Card> hand)
        {
            var method = typeof(GameBootstrap).GetMethod(
                "CanBreakRifkiHand",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null,
                "GameBootstrap.CanBreakRifkiHand bulunamadı.");

            return (bool)method.Invoke(null, new object[] { hand });
        }

        [Test]
        public void Akq_FirstLead_AllowsOnlyAceKingQueenOfTrump()
        {
            var hA = C(Suit.Hearts, Rank.Ace);
            var hK = C(Suit.Hearts, Rank.King);
            var hQ = C(Suit.Hearts, Rank.Queen);
            var hJ = C(Suit.Hearts, Rank.Jack);
            var cA = C(Suit.Clubs, Rank.Ace);

            var hands = MakeHands(
                new[] { hA, hK, hQ, hJ, cA },
                new[] { C(Suit.Clubs, Rank.Two) },
                new[] { C(Suit.Clubs, Rank.Three) },
                new[] { C(Suit.Clubs, Rank.Four) });

            var deal = new DealEngine(
                new ContractCall(ContractType.Trump, Suit.Hearts),
                hands,
                Seat.South);

            var legal = deal.LegalPlays();

            Assert.That(legal, Does.Contain(hA));
            Assert.That(legal, Does.Contain(hK));
            Assert.That(legal, Does.Contain(hQ));
            Assert.That(legal.Any(card => card.Equals(hJ)), Is.False,
                "AKQ istisnası düşük koza izin vermemeli.");
        }

        [Test]
        public void Akq_Right_ExpiresIfCallerFirstLeadsAnotherSuit()
        {
            var hA = C(Suit.Hearts, Rank.Ace);
            var hK = C(Suit.Hearts, Rank.King);
            var hQ = C(Suit.Hearts, Rank.Queen);
            var hJ = C(Suit.Hearts, Rank.Jack);
            var cA = C(Suit.Clubs, Rank.Ace);
            var c2 = C(Suit.Clubs, Rank.Two);
            var c3 = C(Suit.Clubs, Rank.Three);
            var c4 = C(Suit.Clubs, Rank.Four);

            var hands = MakeHands(
                new[] { hA, hK, hQ, hJ, cA },
                new[] { c2 },
                new[] { c3 },
                new[] { c4 });

            var deal = new DealEngine(
                new ContractCall(ContractType.Trump, Suit.Hearts),
                hands,
                Seat.South);

            deal.Play(cA);
            deal.Play(c2);
            deal.Play(c3);
            deal.Play(c4);

            Assert.That(deal.ToPlay, Is.EqualTo(Seat.South));
            Assert.That(deal.TrickNumber, Is.EqualTo(2));
            Assert.That(deal.TrumpBroken, Is.False);

            var legal = deal.LegalPlays();
            Assert.That(legal.Any(card => card.Suit == Suit.Hearts), Is.False,
                "AKQ hakkı ilk eli başka renkle açtıktan sonra devam etmemeli.");
        }

        [Test]
        public void WithoutAkq_TrumpCannotBeLedBeforeBroken()
        {
            var hA = C(Suit.Hearts, Rank.Ace);
            var hK = C(Suit.Hearts, Rank.King);
            var hJ = C(Suit.Hearts, Rank.Jack);
            var cA = C(Suit.Clubs, Rank.Ace);

            var hands = MakeHands(
                new[] { hA, hK, hJ, cA },
                new[] { C(Suit.Clubs, Rank.Two), C(Suit.Hearts, Rank.Queen) },
                new[] { C(Suit.Clubs, Rank.Three) },
                new[] { C(Suit.Clubs, Rank.Four) });

            var deal = new DealEngine(
                new ContractCall(ContractType.Trump, Suit.Hearts),
                hands,
                Seat.South);

            Assert.That(
                deal.LegalPlays().Any(card => card.Suit == Suit.Hearts),
                Is.False);
        }

        [Test]
        public void Rifki_KingOnly_CanBreak()
        {
            Assert.That(
                CanBreakRifki(RifkiHand(C(Suit.Hearts, Rank.King))),
                Is.True);
        }

        [Test]
        public void Rifki_KingAndAceOnly_CanBreak()
        {
            Assert.That(
                CanBreakRifki(RifkiHand(
                    C(Suit.Hearts, Rank.King),
                    C(Suit.Hearts, Rank.Ace))),
                Is.True);
        }

        [Test]
        public void Rifki_KingAndAnotherHeart_CannotBreak()
        {
            Assert.That(
                CanBreakRifki(RifkiHand(
                    C(Suit.Hearts, Rank.King),
                    C(Suit.Hearts, Rank.Queen))),
                Is.False);
        }

        [Test]
        public void Rifki_KingAceAndThirdHeart_CannotBreak()
        {
            Assert.That(
                CanBreakRifki(RifkiHand(
                    C(Suit.Hearts, Rank.King),
                    C(Suit.Hearts, Rank.Ace),
                    C(Suit.Hearts, Rank.Two))),
                Is.False);
        }

        [Test]
        public void Rifki_WithoutKing_CannotBreak()
        {
            Assert.That(
                CanBreakRifki(RifkiHand(C(Suit.Hearts, Rank.Ace))),
                Is.False);
        }
    }
}
