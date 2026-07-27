using System;
using System.Collections.Generic;
using System.Linq;

namespace King.Core
{
    public sealed class DealEngine
    {
        readonly List<Card>[] hands = new List<Card>[4];
        readonly IReadOnlyList<Card>[] handViews = new IReadOnlyList<Card>[4];
        readonly List<(Seat Seat, Card Card)> currentTrick = new List<(Seat Seat, Card Card)>();
        readonly IReadOnlyList<(Seat Seat, Card Card)> currentTrickView;
        readonly List<CompletedTrick> history = new List<CompletedTrick>();
        readonly IReadOnlyList<CompletedTrick> historyView;

        Seat trickLeader;

        public ContractCall Contract { get; }
        public Seat ToPlay { get; private set; }
        public bool IsComplete { get; private set; }

        // 1-based number of the trick in progress; stays at 13 once the deal is done.
        public int TrickNumber { get; private set; }

        // Only meaningful in the hearts contracts, but tracked unconditionally.
        public bool HeartsBroken { get; private set; }

        public IReadOnlyList<(Seat Seat, Card Card)> CurrentTrick => currentTrickView;
        public IReadOnlyList<CompletedTrick> History => historyView;

        public DealEngine(ContractCall contract, IReadOnlyList<Card>[] hands, Seat leader)
        {
            if (hands == null) throw new ArgumentNullException(nameof(hands));
            if (hands.Length != 4)
                throw new ArgumentException("need exactly four hands", nameof(hands));
            if (leader < Seat.South || leader > Seat.East)
                throw new ArgumentOutOfRangeException(nameof(leader));

            var seen = new HashSet<Card>();
            for (int i = 0; i < 4; i++)
            {
                var hand = hands[i];
                if (hand == null)
                    throw new ArgumentException("hand for " + (Seat)i + " is null", nameof(hands));
                if (hand.Count != 13)
                    throw new ArgumentException("hand for " + (Seat)i + " must hold 13 cards", nameof(hands));
                foreach (var card in hand)
                    if (!seen.Add(card))
                        throw new ArgumentException(card + " appears in more than one hand", nameof(hands));
                // Four disjoint hands of 13 necessarily cover the whole deck.
                this.hands[i] = new List<Card>(hand);
                Deck.SortHand(this.hands[i]);
                handViews[i] = this.hands[i].AsReadOnly();
            }

            currentTrickView = currentTrick.AsReadOnly();
            historyView = history.AsReadOnly();

            Contract = contract;
            ToPlay = leader;
            trickLeader = leader;
            TrickNumber = 1;
        }

        // Remaining cards, kept sorted; removals never disturb the order.
        public IReadOnlyList<Card> HandOf(Seat seat)
        {
            if (seat < Seat.South || seat > Seat.East)
                throw new ArgumentOutOfRangeException(nameof(seat));
            return handViews[(int)seat];
        }

        public IReadOnlyList<Card> LegalPlays()
        {
            if (IsComplete)
                throw new InvalidOperationException("the deal is complete");
            var hand = hands[(int)ToPlay];
            return currentTrick.Count == 0
                ? LeadCandidates(hand)
                : FollowCandidates(hand, currentTrick[0].Card.Suit);
        }

        // Leading is unrestricted for now. The hearts-before-broken restriction for the
        // hearts contracts slots in here.
        List<Card> LeadCandidates(List<Card> hand) => new List<Card>(hand);

        // Follow suit if able; a void may play anything. Contract-specific tightening —
        // forced penalty dumps and must-trump-when-void — slots in here.
        List<Card> FollowCandidates(List<Card> hand, Suit led)
        {
            var inSuit = hand.Where(c => c.Suit == led).ToList();
            return inSuit.Count > 0 ? inSuit : new List<Card>(hand);
        }

        public CompletedTrick Play(Card card)
        {
            if (IsComplete)
                throw new InvalidOperationException("the deal is complete");
            if (!LegalPlays().Contains(card))
                throw new InvalidOperationException(card + " is not a legal play for " + ToPlay);

            hands[(int)ToPlay].Remove(card);

            // "Broken" means a heart went down on a trick led in another suit.
            if (card.Suit == Suit.Hearts && currentTrick.Count > 0 && currentTrick[0].Card.Suit != Suit.Hearts)
                HeartsBroken = true;

            currentTrick.Add((ToPlay, card));

            if (currentTrick.Count < 4)
            {
                ToPlay = Next(ToPlay);
                return null;
            }

            var plays = currentTrick.ToArray();
            var winner = TrickWinner(plays);
            var trick = new CompletedTrick(TrickNumber, trickLeader, winner, plays);
            history.Add(trick);
            currentTrick.Clear();

            // Early termination for the card-penalty contracts (nothing left in the deal
            // that can still score) gets decided here once scoring exists.
            if (history.Count == 13)
            {
                IsComplete = true;
            }
            else
            {
                TrickNumber++;
                trickLeader = winner;
                ToPlay = winner;
            }
            return trick;
        }

        public DealScore Score()
        {
            if (!IsComplete)
                throw new InvalidOperationException("the deal is still in progress");
            // Per-contract scoring lands in a later branch.
            throw new NotImplementedException("scoring is not implemented yet");
        }

        static Seat Next(Seat seat) => (Seat)(((int)seat + 1) % 4);

        Seat TrickWinner((Seat Seat, Card Card)[] plays)
        {
            var best = plays[0];
            var led = plays[0].Card.Suit;
            for (int i = 1; i < plays.Length; i++)
                if (Beats(plays[i].Card, best.Card, led))
                    best = plays[i];
            return best.Seat;
        }

        // No trump yet, so only the led suit competes and best always holds a led-suit
        // card. The trump branch extends this: a trump beats any non-trump, and among
        // trumps the higher rank wins.
        bool Beats(Card candidate, Card best, Suit led)
        {
            return candidate.Suit == led && candidate.Rank > best.Rank;
        }
    }
}
