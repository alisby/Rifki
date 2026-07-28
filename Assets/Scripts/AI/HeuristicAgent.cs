using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;

namespace King.AI
{
    // The computer opponent. No search, no simulation: bidding sizes up the
    // hand with a handful of fixed weights and takes the call that looks
    // cheapest (or trump when a long strong suit can carry the deal), and card
    // play is plain table sense — duck and shed danger in the penalty deals,
    // draw trumps and win cheaply in the trump ones. All weights are constants
    // and the one rng tie-break it does use is seeded, so a given seed always
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

        public Card ChooseCard(DealEngine deal, Seat seat)
        {
            var legal = deal.LegalPlays();
            if (legal.Count == 1)
                return legal[0];
            return deal.Contract.Type == ContractType.Trump
                ? ChooseTrumpCard(deal, seat, legal)
                : ChoosePenaltyCard(deal, legal);
        }

        Card ChooseTrumpCard(DealEngine deal, Seat seat, IReadOnlyList<Card> legal)
        {
            var trump = deal.Contract.TrumpSuit.Value;
            if (deal.CurrentTrick.Count == 0)
                return TrumpLead(deal, seat, legal, trump);

            var winners = legal.Where(c => WinsAsItStands(deal, c)).ToList();
            if (winners.Count > 0)
                return Lowest(winners);   // take the trick as cheaply as possible
            return Lowest(legal);         // can't have it, so keep the good cards back
        }

        static Card TrumpLead(DealEngine deal, Seat seat, IReadOnlyList<Card> legal, Suit trump)
        {
            var mine = legal.Where(c => c.Suit == trump).ToList();

            // Draw trumps while the top one out is ours and someone still has any.
            if (mine.Count > 0 && OutstandingTrumps(deal, seat, trump) > 0 && HoldsBossTrump(deal, mine, trump))
                return Highest(mine);

            // Otherwise work on the longest side suit from the bottom, saving winners.
            var side = legal.Where(c => c.Suit != trump).ToList();
            if (side.Count == 0)
                return Highest(mine);
            var suit = LongestSuit(side);
            return Lowest(side.Where(c => c.Suit == suit).ToList());
        }

        Card ChoosePenaltyCard(DealEngine deal, IReadOnlyList<Card> legal)
        {
            // Only tricks 12 and 13 score in no last two, so the early game is
            // for burning the cards that would otherwise win them.
            if (deal.Contract.Type == ContractType.NoLastTwo && deal.TrickNumber <= 11)
                return Highest(legal);

            if (deal.CurrentTrick.Count == 0)
                return Lowest(legal);   // small cards don't win tricks

            // Duck whenever the trick can be lost, shedding the most dangerous
            // card that still stays under. A king of hearts deal only lasts
            // while the king is uncaptured, so staying under the trick the
            // whole way through is exactly "don't win while the king is live".
            var ducks = legal.Where(c => !WinsAsItStands(deal, c)).ToList();
            if (ducks.Count > 0)
                return MostDangerous(deal.Contract.Type, ducks);

            // Every legal card currently wins. Last to speak, spend the biggest
            // card since the trick is ours anyway; earlier, play the smallest
            // winner and hope somebody behind covers it.
            return deal.CurrentTrick.Count == 3 ? Highest(legal) : Lowest(legal);
        }

        // Highest-scoring card to get rid of; equal danger is settled by the rng.
        Card MostDangerous(ContractType type, List<Card> cards)
        {
            int bestScore = -1;
            var tied = new List<Card>();
            foreach (var c in cards)
            {
                int score = DangerScore(type, c);
                if (score > bestScore)
                {
                    bestScore = score;
                    tied.Clear();
                }
                if (score == bestScore)
                    tied.Add(c);
            }
            return tied[rng.Next(tied.Count)];
        }

        // High cards are always a little dangerous; the contract's own penalty
        // cards much more so. In king of hearts the ace matters too, because
        // it is the card most likely to catch the king.
        static int DangerScore(ContractType type, Card c)
        {
            int score = (int)c.Rank;
            switch (type)
            {
                case ContractType.NoHearts:
                    if (c.Suit == Suit.Hearts) score += 20;
                    break;
                case ContractType.NoQueens:
                    if (c.Rank == Rank.Queen) score += 20;
                    break;
                case ContractType.NoMen:
                    if (c.Rank == Rank.King || c.Rank == Rank.Jack) score += 20;
                    break;
                case ContractType.KingOfHearts:
                    if (c.Suit == Suit.Hearts && c.Rank == Rank.King) score += 40;
                    else if (c.Suit == Suit.Hearts && c.Rank == Rank.Ace) score += 20;
                    break;
            }
            return score;
        }

        static bool WinsAsItStands(DealEngine deal, Card card)
        {
            var trick = deal.CurrentTrick;
            var trump = deal.Contract.TrumpSuit;
            var best = trick[0].Card;
            for (int i = 1; i < trick.Count; i++)
                if (Beats(trick[i].Card, best, trump))
                    best = trick[i].Card;
            return Beats(card, best, trump);
        }

        // Same ordering the engine applies: trumps over everything, otherwise
        // only a higher card of the same suit as the current best (which is
        // always the led suit) takes over.
        static bool Beats(Card candidate, Card best, Suit? trump)
        {
            if (trump != null)
            {
                bool candidateTrumps = candidate.Suit == trump.Value;
                bool bestTrumps = best.Suit == trump.Value;
                if (candidateTrumps != bestTrumps)
                    return candidateTrumps;
                if (candidateTrumps)
                    return candidate.Rank > best.Rank;
            }
            return candidate.Suit == best.Suit && candidate.Rank > best.Rank;
        }

        static int OutstandingTrumps(DealEngine deal, Seat seat, Suit trump)
        {
            int gone = 0;
            foreach (var trick in deal.History)
                foreach (var play in trick.Plays)
                    if (play.Card.Suit == trump)
                        gone++;
            int mine = 0;
            foreach (var c in deal.HandOf(seat))
                if (c.Suit == trump)
                    mine++;
            return 13 - gone - mine;
        }

        // Does this hand hold the highest trump still in play?
        static bool HoldsBossTrump(DealEngine deal, List<Card> mine, Suit trump)
        {
            var played = new HashSet<Card>();
            foreach (var trick in deal.History)
                foreach (var play in trick.Plays)
                    played.Add(play.Card);

            for (int r = (int)Rank.Ace; r >= (int)Rank.Two; r--)
            {
                var card = new Card(trump, (Rank)r);
                if (played.Contains(card))
                    continue;
                return mine.Contains(card);
            }
            return false;
        }

        static Suit LongestSuit(List<Card> cards)
        {
            var counts = new int[4];
            foreach (var c in cards)
                counts[(int)c.Suit]++;
            var best = Suit.Clubs;
            for (int s = 1; s < 4; s++)
                if (counts[s] > counts[(int)best])
                    best = (Suit)s;
            return best;
        }

        static Card Lowest(IReadOnlyList<Card> cards)
        {
            var best = cards[0];
            foreach (var c in cards)
                if (c.Rank < best.Rank || (c.Rank == best.Rank && c.Suit < best.Suit))
                    best = c;
            return best;
        }

        static Card Highest(IReadOnlyList<Card> cards)
        {
            var best = cards[0];
            foreach (var c in cards)
                if (c.Rank > best.Rank || (c.Rank == best.Rank && c.Suit > best.Suit))
                    best = c;
            return best;
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
