using System;
using System.Collections.Generic;
using System.Linq;
using King.AI;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class HeuristicPlayTests
    {
        static Card C(Suit s, Rank r) => new Card(s, r);

        static List<Card> Suited(Suit s) =>
            Deck.Standard().Where(c => c.Suit == s).ToList();

        // Trade one card between two hands so voids and holdings line up the
        // way a scenario needs them.
        static void Swap(List<Card> a, List<Card> b, Card fromA, Card fromB)
        {
            Assert.IsTrue(a.Remove(fromA));
            Assert.IsTrue(b.Remove(fromB));
            a.Add(fromB);
            b.Add(fromA);
        }

        static IReadOnlyList<Card>[] Hands(List<Card> s, List<Card> w, List<Card> n, List<Card> e) =>
            new IReadOnlyList<Card>[] { s.ToArray(), w.ToArray(), n.ToArray(), e.ToArray() };

        static IReadOnlyList<Card>[] RoundRobin()
        {
            var deck = Deck.Standard();
            var lists = new[] { new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
            for (int i = 0; i < deck.Length; i++)
                lists[i % 4].Add(deck[i]);
            return lists.Select(l => (IReadOnlyList<Card>)l.ToArray()).ToArray();
        }

        // The big one: whole sessions of four heuristic agents, hundreds of
        // seeds. Every card they offer must be legal at that moment, and the
        // finished sheets must balance to zero like any correct session.
        [Test]
        public void FiveHundredSessionsStayLegalAndBalance()
        {
            for (int seed = 0; seed < 500; seed++)
            {
                var session = new Session(seed);
                var handRng = new Random(seed * 613 + 11);
                var agents = new IPlayerAgent[4];
                for (int s = 0; s < 4; s++)
                    agents[s] = new HeuristicAgent(seed * 4 + s);

                while (!session.IsComplete)
                {
                    var call = agents[(int)session.Caller]
                        .ChooseContract(session, Deck.Deal(handRng)[0], session.AvailableContracts());
                    var deal = session.StartDeal(call);
                    while (!deal.IsComplete)
                    {
                        var legal = deal.LegalPlays();
                        var pick = agents[(int)deal.ToPlay].ChooseCard(deal, deal.ToPlay);
                        CollectionAssert.Contains(legal, pick);
                        deal.Play(pick);
                    }
                    session.FinishDeal();
                }

                Assert.AreEqual(20, session.Sheet.Count, "seed " + seed);
                Assert.AreEqual(0, session.Totals.Sum(), "seed " + seed);
            }
        }

        [Test]
        public void DucksUnderAHigherCardInNoTricks()
        {
            var south = Suited(Suit.Clubs);
            var west = Suited(Suit.Diamonds);
            Swap(south, west, C(Suit.Clubs, Rank.King), C(Suit.Diamonds, Rank.Three));
            Swap(south, west, C(Suit.Clubs, Rank.Three), C(Suit.Diamonds, Rank.Two));

            var deal = new DealEngine(new ContractCall(ContractType.NoTricks),
                Hands(south, west, Suited(Suit.Hearts), Suited(Suit.Spades)), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ace));

            // West holds the king and the three of clubs; both lose to the ace,
            // so the duck should shed the king while it's safe to do so.
            CollectionAssert.AreEquivalent(
                new[] { C(Suit.Clubs, Rank.King), C(Suit.Clubs, Rank.Three) }, deal.LegalPlays());
            var pick = new HeuristicAgent(7).ChooseCard(deal, Seat.West);
            Assert.AreEqual(C(Suit.Clubs, Rank.King), pick);
        }

        [Test]
        public void DumpsAQueenWhenVoidInNoQueens()
        {
            var west = Suited(Suit.Diamonds);
            var east = Suited(Suit.Spades);
            Swap(west, east, C(Suit.Diamonds, Rank.Two), C(Suit.Spades, Rank.Queen));

            var deal = new DealEngine(new ContractCall(ContractType.NoQueens),
                Hands(Suited(Suit.Clubs), west, Suited(Suit.Hearts), east), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));

            // West is void in clubs holding two queens; whichever it picks, a
            // queen goes.
            Assert.AreEqual(2, deal.LegalPlays().Count);
            var pick = new HeuristicAgent(7).ChooseCard(deal, Seat.West);
            Assert.AreEqual(Rank.Queen, pick.Rank);
        }

        [Test]
        public void DiscardsTheAceOfHeartsWhenVoidInKingOfHearts()
        {
            var west = Suited(Suit.Hearts);
            var east = Suited(Suit.Spades);
            Swap(west, east, C(Suit.Hearts, Rank.King), C(Suit.Spades, Rank.Ace));

            var deal = new DealEngine(new ContractCall(ContractType.KingOfHearts),
                Hands(Suited(Suit.Clubs), west, Suited(Suit.Diamonds), east), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Two));

            // West may discard anything. The ace of hearts is the card most
            // likely to catch the king later, so it should go first — ahead of
            // the equally high ace of spades.
            Assert.AreEqual(13, deal.LegalPlays().Count);
            var pick = new HeuristicAgent(7).ChooseCard(deal, Seat.West);
            Assert.AreEqual(C(Suit.Hearts, Rank.Ace), pick);
        }

        [Test]
        public void NoLastTwoBurnsWinnersWhileTricksAreFree()
        {
            var deal = new DealEngine(new ContractCall(ContractType.NoLastTwo),
                Hands(Suited(Suit.Clubs), Suited(Suit.Diamonds), Suited(Suit.Hearts), Suited(Suit.Spades)),
                Seat.South);
            var agent = new HeuristicAgent(7);

            // Trick one costs nothing, so the biggest cards should go now.
            var lead = agent.ChooseCard(deal, Seat.South);
            Assert.AreEqual(C(Suit.Clubs, Rank.Ace), lead);
            deal.Play(lead);
            Assert.AreEqual(C(Suit.Diamonds, Rank.Ace), agent.ChooseCard(deal, Seat.West));
        }

        [Test]
        public void NoLastTwoDucksTheTwelfthTrick()
        {
            var deal = new DealEngine(new ContractCall(ContractType.NoLastTwo), RoundRobin(), Seat.South);
            var agent = new HeuristicAgent(9);

            // Any fixed policy gets us to the endgame; the agent only takes
            // over once the tricks start costing points.
            while (deal.TrickNumber < 12)
                deal.Play(deal.LegalPlays()[0]);

            deal.Play(agent.ChooseCard(deal, deal.ToPlay));
            for (int follower = 0; follower < 3; follower++)
            {
                var legal = deal.LegalPlays();
                var led = deal.CurrentTrick[0].Card.Suit;
                int top = deal.CurrentTrick.Where(p => p.Card.Suit == led).Max(p => (int)p.Card.Rank);
                var pick = agent.ChooseCard(deal, deal.ToPlay);

                // Whenever a losing card is available, the agent must play one.
                if (legal.Any(c => c.Suit != led || (int)c.Rank < top))
                    Assert.IsTrue(pick.Suit != led || (int)pick.Rank < top, pick + " should stay under the trick");
                deal.Play(pick);
            }
            Assert.AreEqual(13, deal.TrickNumber);
        }

        [Test]
        public void StrongTrumpHolderLeadsTheBossTrump()
        {
            var south = new List<Card>
            {
                C(Suit.Spades, Rank.Ace), C(Suit.Spades, Rank.King), C(Suit.Spades, Rank.Queen),
                C(Suit.Spades, Rank.Jack), C(Suit.Spades, Rank.Ten), C(Suit.Spades, Rank.Nine),
                C(Suit.Spades, Rank.Eight), C(Suit.Spades, Rank.Seven),
                C(Suit.Clubs, Rank.Five), C(Suit.Clubs, Rank.Four), C(Suit.Clubs, Rank.Three),
                C(Suit.Clubs, Rank.Two), C(Suit.Diamonds, Rank.Two),
            };
            var west = Suited(Suit.Hearts);
            var east = new List<Card>
            {
                C(Suit.Spades, Rank.Six), C(Suit.Spades, Rank.Five), C(Suit.Spades, Rank.Four),
                C(Suit.Spades, Rank.Three), C(Suit.Spades, Rank.Two),
                C(Suit.Clubs, Rank.Ace), C(Suit.Clubs, Rank.King), C(Suit.Clubs, Rank.Queen),
                C(Suit.Clubs, Rank.Jack), C(Suit.Clubs, Rank.Ten), C(Suit.Clubs, Rank.Nine),
                C(Suit.Clubs, Rank.Eight), C(Suit.Clubs, Rank.Seven),
            };
            var north = Deck.Standard()
                .Where(c => !south.Contains(c) && !west.Contains(c) && !east.Contains(c))
                .ToList();

            var deal = new DealEngine(new ContractCall(ContractType.Trump, Suit.Spades),
                Hands(south, west, north, east), Seat.South);

            // Five trumps are out against South's eight and the ace is South's:
            // draw from the top.
            var pick = new HeuristicAgent(7).ChooseCard(deal, Seat.South);
            Assert.AreEqual(C(Suit.Spades, Rank.Ace), pick);
        }

        [Test]
        public void WinsAsCheaplyAsPossibleInTrump()
        {
            var south = Suited(Suit.Clubs);
            var west = Suited(Suit.Diamonds);
            Swap(south, west, C(Suit.Clubs, Rank.Queen), C(Suit.Diamonds, Rank.Three));
            Swap(south, west, C(Suit.Clubs, Rank.Jack), C(Suit.Diamonds, Rank.Two));

            var deal = new DealEngine(new ContractCall(ContractType.Trump, Suit.Spades),
                Hands(south, west, Suited(Suit.Hearts), Suited(Suit.Spades)), Seat.South);
            deal.Play(C(Suit.Clubs, Rank.Ten));

            // West's queen and jack both beat the ten; the jack is enough.
            var pick = new HeuristicAgent(7).ChooseCard(deal, Seat.West);
            Assert.AreEqual(C(Suit.Clubs, Rank.Jack), pick);
        }
    }
}
