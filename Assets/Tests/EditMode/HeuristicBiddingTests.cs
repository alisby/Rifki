using System;
using System.Collections.Generic;
using System.Linq;
using King.AI;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class HeuristicBiddingTests
    {
        static Card C(Suit s, Rank r) => new Card(s, r);

        static IReadOnlyList<Card> Hand(params Card[] cards)
        {
            Assert.AreEqual(13, cards.Length, "test hand must hold 13 cards");
            return cards;
        }

        // A fresh session's first caller has every contract open, which is the
        // natural backdrop for the hand-shape tests.
        static ContractCall Choose(IReadOnlyList<Card> hand)
        {
            var session = new Session(1);
            return new HeuristicAgent(1).ChooseContract(session, hand, session.AvailableContracts());
        }

        [Test]
        public void LongStrongSuitCallsTrumpInThatSuit()
        {
            var call = Choose(Hand(
                C(Suit.Spades, Rank.Ace), C(Suit.Spades, Rank.King), C(Suit.Spades, Rank.Queen),
                C(Suit.Spades, Rank.Jack), C(Suit.Spades, Rank.Ten), C(Suit.Spades, Rank.Nine),
                C(Suit.Spades, Rank.Eight),
                C(Suit.Clubs, Rank.Five), C(Suit.Clubs, Rank.Four), C(Suit.Clubs, Rank.Three),
                C(Suit.Clubs, Rank.Two),
                C(Suit.Diamonds, Rank.Three), C(Suit.Diamonds, Rank.Two)));

            Assert.AreEqual(ContractType.Trump, call.Type);
            Assert.AreEqual(Suit.Spades, call.TrumpSuit);
        }

        [Test]
        public void QueenlessHandCallsNoQueens()
        {
            // No queens at all, but enough kings and jacks scattered around to
            // make every other penalty and a trump call look worse.
            var call = Choose(Hand(
                C(Suit.Hearts, Rank.Ace), C(Suit.Hearts, Rank.King),
                C(Suit.Clubs, Rank.King), C(Suit.Clubs, Rank.Jack), C(Suit.Clubs, Rank.Five),
                C(Suit.Clubs, Rank.Four),
                C(Suit.Diamonds, Rank.Jack), C(Suit.Diamonds, Rank.Five), C(Suit.Diamonds, Rank.Four),
                C(Suit.Diamonds, Rank.Three),
                C(Suit.Spades, Rank.Jack), C(Suit.Spades, Rank.Eight), C(Suit.Spades, Rank.Two)));

            Assert.AreEqual(ContractType.NoQueens, call.Type);
            Assert.IsNull(call.TrumpSuit);
        }

        [Test]
        public void HeartlessHandCallsNoHearts()
        {
            var call = Choose(Hand(
                C(Suit.Clubs, Rank.Queen), C(Suit.Clubs, Rank.Seven), C(Suit.Clubs, Rank.Four),
                C(Suit.Clubs, Rank.Two),
                C(Suit.Diamonds, Rank.Queen), C(Suit.Diamonds, Rank.Eight), C(Suit.Diamonds, Rank.Five),
                C(Suit.Diamonds, Rank.Three),
                C(Suit.Spades, Rank.King), C(Suit.Spades, Rank.Queen), C(Suit.Spades, Rank.Jack),
                C(Suit.Spades, Rank.Six), C(Suit.Spades, Rank.Two)));

            Assert.AreEqual(ContractType.NoHearts, call.Type);
        }

        [Test]
        public void WeakFlatHandCallsNoTricks()
        {
            var call = Choose(Hand(
                C(Suit.Clubs, Rank.Nine), C(Suit.Clubs, Rank.Six), C(Suit.Clubs, Rank.Three),
                C(Suit.Clubs, Rank.Two),
                C(Suit.Diamonds, Rank.Eight), C(Suit.Diamonds, Rank.Five), C(Suit.Diamonds, Rank.Four),
                C(Suit.Diamonds, Rank.Two),
                C(Suit.Hearts, Rank.Seven), C(Suit.Hearts, Rank.Four), C(Suit.Hearts, Rank.Two),
                C(Suit.Spades, Rank.Five), C(Suit.Spades, Rank.Three)));

            Assert.AreEqual(ContractType.NoTricks, call.Type);
        }

        [Test]
        public void BareKingOfHeartsStaysAwayFromThatContract()
        {
            var call = Choose(Hand(
                C(Suit.Hearts, Rank.King),
                C(Suit.Clubs, Rank.Eight), C(Suit.Clubs, Rank.Seven), C(Suit.Clubs, Rank.Five),
                C(Suit.Clubs, Rank.Four), C(Suit.Clubs, Rank.Three), C(Suit.Clubs, Rank.Two),
                C(Suit.Diamonds, Rank.Eight), C(Suit.Diamonds, Rank.Six), C(Suit.Diamonds, Rank.Four),
                C(Suit.Diamonds, Rank.Two),
                C(Suit.Spades, Rank.Four), C(Suit.Spades, Rank.Three)));

            Assert.AreNotEqual(ContractType.KingOfHearts, call.Type);
            Assert.AreEqual(ContractType.NoQueens, call.Type);
        }

        [Test]
        public void QueenHeavyHandAvoidsNoQueens()
        {
            var call = Choose(Hand(
                C(Suit.Clubs, Rank.Queen), C(Suit.Clubs, Rank.Three), C(Suit.Clubs, Rank.Two),
                C(Suit.Diamonds, Rank.Queen), C(Suit.Diamonds, Rank.Four), C(Suit.Diamonds, Rank.Two),
                C(Suit.Hearts, Rank.Queen), C(Suit.Hearts, Rank.Five), C(Suit.Hearts, Rank.Three),
                C(Suit.Hearts, Rank.Two),
                C(Suit.Spades, Rank.Queen), C(Suit.Spades, Rank.Four), C(Suit.Spades, Rank.Two)));

            Assert.AreNotEqual(ContractType.NoQueens, call.Type);
        }

        [Test]
        public void ForcedTrumpNamesASuitEvenOnAJunkHand()
        {
            var session = new Session(3);
            var agent = new HeuristicAgent(3);
            var hand = Hand(
                C(Suit.Diamonds, Rank.King), C(Suit.Diamonds, Rank.Nine), C(Suit.Diamonds, Rank.Seven),
                C(Suit.Diamonds, Rank.Five), C(Suit.Diamonds, Rank.Three),
                C(Suit.Clubs, Rank.Six), C(Suit.Clubs, Rank.Four), C(Suit.Clubs, Rank.Two),
                C(Suit.Hearts, Rank.Six), C(Suit.Hearts, Rank.Three),
                C(Suit.Spades, Rank.Seven), C(Suit.Spades, Rank.Five), C(Suit.Spades, Rank.Two));

            var call = agent.ChooseContract(session, hand, new[] { ContractType.Trump });
            Assert.AreEqual(ContractType.Trump, call.Type);
            Assert.AreEqual(Suit.Diamonds, call.TrumpSuit);
        }

        [Test]
        public void TrumpWorthyHandRespectsAPenaltyOnlyMenu()
        {
            var session = new Session(4);
            var agent = new HeuristicAgent(4);
            var hand = Hand(
                C(Suit.Spades, Rank.Ace), C(Suit.Spades, Rank.King), C(Suit.Spades, Rank.Queen),
                C(Suit.Spades, Rank.Jack), C(Suit.Spades, Rank.Ten), C(Suit.Spades, Rank.Nine),
                C(Suit.Spades, Rank.Eight),
                C(Suit.Clubs, Rank.Five), C(Suit.Clubs, Rank.Four), C(Suit.Clubs, Rank.Three),
                C(Suit.Clubs, Rank.Two),
                C(Suit.Diamonds, Rank.Three), C(Suit.Diamonds, Rank.Two));

            var menu = new[] { ContractType.NoHearts, ContractType.NoLastTwo };
            var call = agent.ChooseContract(session, hand, menu);
            CollectionAssert.Contains(menu, call.Type);
            Assert.IsNull(call.TrumpSuit);
        }

        // Full sessions with rotating callers and shrinking quotas: whatever
        // hand the agent is shown, its call must come off the current menu.
        [Test]
        public void NeverCallsAnUnavailableContractAcrossManySessions()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                var session = new Session(seed);
                var handRng = new Random(seed * 31 + 7);
                var agents = new IPlayerAgent[4];
                for (int s = 0; s < 4; s++)
                    agents[s] = new HeuristicAgent(seed * 4 + s);

                while (!session.IsComplete)
                {
                    var available = session.AvailableContracts();
                    var hand = Deck.Deal(handRng)[0];
                    var call = agents[(int)session.Caller].ChooseContract(session, hand, available);

                    CollectionAssert.Contains(available, call.Type, "seed " + seed + " deal " + session.DealNumber);
                    Assert.AreEqual(call.Type == ContractType.Trump, call.TrumpSuit != null);

                    var deal = session.StartDeal(call);
                    while (!deal.IsComplete)
                        deal.Play(agents[(int)deal.ToPlay].ChooseCard(deal, deal.ToPlay));
                    session.FinishDeal();
                }
                Assert.AreEqual(0, session.Totals.Sum(), "seed " + seed);
            }
        }

        [Test]
        public void SameSeedsReplayTheSameSession()
        {
            var first = PlaySession(11);
            var second = PlaySession(11);

            Assert.AreEqual(first.Sheet.Count, second.Sheet.Count);
            for (int i = 0; i < first.Sheet.Count; i++)
            {
                Assert.AreEqual(first.Sheet[i].Contract.Type, second.Sheet[i].Contract.Type, "deal " + (i + 1));
                Assert.AreEqual(first.Sheet[i].Contract.TrumpSuit, second.Sheet[i].Contract.TrumpSuit, "deal " + (i + 1));
                CollectionAssert.AreEqual(first.Sheet[i].Points, second.Sheet[i].Points, "deal " + (i + 1));
            }
        }

        static Session PlaySession(int seed)
        {
            var session = new Session(seed);
            var handRng = new Random(seed + 1);
            var agents = new IPlayerAgent[4];
            for (int s = 0; s < 4; s++)
                agents[s] = new HeuristicAgent(seed * 4 + s);

            while (!session.IsComplete)
            {
                var call = agents[(int)session.Caller]
                    .ChooseContract(session, Deck.Deal(handRng)[0], session.AvailableContracts());
                var deal = session.StartDeal(call);
                while (!deal.IsComplete)
                    deal.Play(agents[(int)deal.ToPlay].ChooseCard(deal, deal.ToPlay));
                session.FinishDeal();
            }
            return session;
        }
    }
}
