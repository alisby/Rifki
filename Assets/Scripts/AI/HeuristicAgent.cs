using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;

namespace King.AI
{
    // The computer opponent. No search, no simulation: it sizes up the hand
    // with a handful of fixed weights and takes the call that looks cheapest,
    // or trump when a long strong suit can carry the deal. All weights are
    // constants and ties break the same way every time, so a given seed always
    // produces the same decisions.
    public sealed class HeuristicAgent : IPlayerAgent
    {
        // A trump deal pays +650 while penalty costs here are small integers,
        // so trump wins whenever the hand clears this bar.
        const int TrumpThreshold = 18;

        readonly Random rng;

        public HeuristicAgent(int seed)
        {
            rng = new Random(seed);
        }

        public ContractCall ChooseContract(Session session, IReadOnlyList<Card> hand, IReadOnlyList<ContractType> available)
        {
            bool trumpOpen = available.Contains(ContractType.Trump);
            var penalties = available.Where(t => t != ContractType.Trump).ToList();

            if (penalties.Count == 0)
                return TrumpCall(hand);
            if (!trumpOpen || TrumpAppeal(hand) < TrumpThreshold)
                return new ContractCall(CheapestPenalty(hand, penalties));
            return TrumpCall(hand);
        }

        // Placeholder until the play heuristics land: any legal card.
        public Card ChooseCard(DealEngine deal, Seat seat)
        {
            var legal = deal.LegalPlays();
            return legal[rng.Next(legal.Count)];
        }

        static ContractCall TrumpCall(IReadOnlyList<Card> hand)
        {
            return new ContractCall(ContractType.Trump, BestTrumpSuit(hand));
        }

        static Suit BestTrumpSuit(IReadOnlyList<Card> hand)
        {
            var best = Suit.Clubs;
            int bestScore = -1;
            for (int s = 0; s < 4; s++)
            {
                int score = SuitScore(hand, (Suit)s);
                if (score > bestScore)
                {
                    best = (Suit)s;
                    bestScore = score;
                }
            }
            return best;
        }

        // Two points per card of the suit plus honor weight: length matters as
        // much as top cards when the suit is going to be trump.
        static int SuitScore(IReadOnlyList<Card> hand, Suit suit)
        {
            int score = 0;
            foreach (var c in hand)
                if (c.Suit == suit)
                    score += 2 + HonorPoints(c.Rank);
            return score;
        }

        static int HonorPoints(Rank r)
        {
            switch (r)
            {
                case Rank.Ace: return 4;
                case Rank.King: return 3;
                case Rank.Queen: return 2;
                case Rank.Jack: return 1;
                default: return 0;
            }
        }

        // Best suit's score plus outside aces and kings, which cash tricks
        // once trumps are drawn.
        static int TrumpAppeal(IReadOnlyList<Card> hand)
        {
            var suit = BestTrumpSuit(hand);
            int appeal = SuitScore(hand, suit);
            foreach (var c in hand)
                if (c.Suit != suit)
                    appeal += c.Rank == Rank.Ace ? 2 : c.Rank == Rank.King ? 1 : 0;
            return appeal;
        }

        // First cheapest wins ties, and `penalties` arrives in declaration
        // order, so a flat hand settles on the earliest contract type.
        static ContractType CheapestPenalty(IReadOnlyList<Card> hand, List<ContractType> penalties)
        {
            var best = penalties[0];
            int bestCost = PenaltyCost(hand, best);
            for (int i = 1; i < penalties.Count; i++)
            {
                int cost = PenaltyCost(hand, penalties[i]);
                if (cost < bestCost)
                {
                    best = penalties[i];
                    bestCost = cost;
                }
            }
            return best;
        }

        // Rough expected pain of calling each penalty with this hand. The
        // scales only have to be comparable with each other, not with points.
        static int PenaltyCost(IReadOnlyList<Card> hand, ContractType type)
        {
            switch (type)
            {
                case ContractType.NoTricks:
                    // raw trick-taking power is the whole problem here
                    return hand.Sum(c => TrickPower(c.Rank));

                case ContractType.NoHearts:
                    // high hearts win heart tricks; even small ones clog the hand
                    return hand.Where(c => c.Suit == Suit.Hearts)
                               .Sum(c => c.Rank >= Rank.Ten ? (int)c.Rank - 8 : 1);

                case ContractType.NoQueens:
                    return 5 * hand.Count(c => c.Rank == Rank.Queen)
                         + hand.Count(c => c.Rank >= Rank.King) / 2;

                case ContractType.NoMen:
                    return 4 * hand.Count(c => c.Rank == Rank.King || c.Rank == Rank.Jack)
                         + hand.Count(c => c.Rank == Rank.Ace);

                case ContractType.KingOfHearts:
                {
                    var hearts = hand.Where(c => c.Suit == Suit.Hearts).ToList();
                    if (hearts.Any(c => c.Rank == Rank.King))
                    {
                        // small hearts under the king let it duck; bare it's a bomb
                        int guards = hearts.Count(c => c.Rank < Rank.King);
                        return 9 - 3 * Math.Min(2, guards);
                    }
                    // without the king the main danger is catching it with the ace
                    return hearts.Any(c => c.Rank == Rank.Ace) ? 3 : 1;
                }

                case ContractType.NoLastTwo:
                {
                    // high cards and a long suit are what keep you on lead at the end
                    int longest = 0;
                    for (int s = 0; s < 4; s++)
                        longest = Math.Max(longest, hand.Count(c => (int)c.Suit == s));
                    return hand.Sum(c => c.Rank == Rank.Ace ? 2 : c.Rank == Rank.King ? 1 : 0)
                         + Math.Max(0, longest - 4);
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        static int TrickPower(Rank r)
        {
            switch (r)
            {
                case Rank.Ace: return 3;
                case Rank.King: return 2;
                case Rank.Queen: return 1;
                default: return 0;
            }
        }
    }
}
