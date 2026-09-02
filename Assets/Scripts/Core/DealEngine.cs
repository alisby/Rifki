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

        // Koz kontratında, koz henüz oynanmadan önce kozla çıkış normalde
        // yasaktır. Kontratı seçenin başlangıç elinde koz A-K-Q birlikteyse
        // bu yasak baştan kalkar.
        readonly bool trumpMayBeLedBeforeBroken;

        public ContractCall Contract { get; }
        public Seat ToPlay { get; private set; }
        public bool IsComplete { get; private set; }

        // 1-based number of the trick in progress. Once the deal is done it stays
        // on the last trick played, which is short of 13 when the deal ended early.
        public int TrickNumber { get; private set; }

        // Only meaningful in the hearts contracts, but tracked unconditionally.
        public bool HeartsBroken { get; private set; }

        // İlk koz oynandığında true olur.
        public bool TrumpBroken { get; private set; }

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

            if (contract.TrumpSuit != null)
            {
                Suit trump = contract.TrumpSuit.Value;
                var callerHand = this.hands[(int)leader];

                trumpMayBeLedBeforeBroken =
                    callerHand.Any(c => c.Suit == trump && c.Rank == Rank.Ace)
                    && callerHand.Any(c => c.Suit == trump && c.Rank == Rank.King)
                    && callerHand.Any(c => c.Suit == trump && c.Rank == Rank.Queen);
            }

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

        // In the hearts contracts a heart may not be led until one has been discarded
        // on an earlier trick, unless hearts are all the leader has left.
        List<Card> LeadCandidates(List<Card> hand)
        {
            if (LeadingHeartsRestricted && !HeartsBroken)
            {
                var offSuit = hand.Where(c => c.Suit != Suit.Hearts).ToList();
                if (offSuit.Count > 0)
                    return offSuit;
            }

            // Koz henüz açılmamışsa kozla çıkılamaz.
            // İstisna: kontratı seçen oyuncunun başlangıç elinde
            // koz As-Papaz-Kız birlikte bulunuyorsa koz baştan açılabilir.
            if (Contract.TrumpSuit != null
                && !TrumpBroken
                && !trumpMayBeLedBeforeBroken)
            {
                var nonTrumps =
                    hand.Where(c => c.Suit != Contract.TrumpSuit.Value).ToList();

                // Oyuncunun elinde yalnızca koz kaldıysa oyun kilitlenmemeli.
                if (nonTrumps.Count > 0)
                    return nonTrumps;
            }

            return new List<Card>(hand);
        }

        bool LeadingHeartsRestricted =>
            Contract.Type == ContractType.NoHearts || Contract.Type == ContractType.KingOfHearts;

        // Follow suit if able. A void must trump in a trump deal (with no obligation
        // to beat trumps already on the table); in the card-penalty deals a void must
        // dump a penalty card if any is held, and a follower whose penalty card is
        // already outranked on the table must let it go.
        List<Card> FollowCandidates(List<Card> hand, Suit led)
        {
            var inSuit = hand.Where(c => c.Suit == led).ToList();
            if (inSuit.Count > 0)
            {
                var beaten = inSuit.Where(c => IsPenaltyCard(c) && c.Rank < HighestOnTable(led)).ToList();
                return beaten.Count > 0 ? beaten : inSuit;
            }

            if (Contract.TrumpSuit != null)
            {
                var trumps = hand.Where(c => c.Suit == Contract.TrumpSuit.Value).ToList();
                if (trumps.Count > 0)
                    return trumps;
            }

            // Rıfkı: oyuncu çıkılan renkten yoksa önce Kupa Papazı
            // atılmak zorundadır. Kupa Papazı elde yoksa, elde herhangi
            // bir kupa varsa kupa atmak zorunludur.
            if (Contract.Type == ContractType.KingOfHearts)
            {
                var rifki = hand
                    .Where(c =>
                        c.Suit == Suit.Hearts &&
                        c.Rank == Rank.King)
                    .ToList();

                if (rifki.Count > 0)
                    return rifki;

                var hearts = hand
                    .Where(c => c.Suit == Suit.Hearts)
                    .ToList();

                if (hearts.Count > 0)
                    return hearts;

                return new List<Card>(hand);
            }

            var dumps = hand.Where(IsPenaltyCard).ToList();
            if (dumps.Count > 0)
                return dumps;

            return new List<Card>(hand);
        }

        // Highest rank of the led suit on the trick so far. Only called while
        // following, so the led card itself guarantees a match.
        Rank HighestOnTable(Suit led)
        {
            var best = Rank.Two;
            foreach (var play in currentTrick)
                if (play.Card.Suit == led && play.Card.Rank > best)
                    best = play.Card.Rank;
            return best;
        }

        // Cards that cost their captor points under the current contract. Forced
        // dumping and early termination both key off this. Trump, no tricks and
        // no last two hold no penalty cards, so neither rule touches them.
        internal static bool ScoresByCard(ContractType type) =>
            type == ContractType.NoHearts || type == ContractType.NoQueens
            || type == ContractType.NoMen || type == ContractType.KingOfHearts;

        bool IsPenaltyCard(Card card)
        {
            switch (Contract.Type)
            {
                case ContractType.NoHearts:
                    return card.Suit == Suit.Hearts;
                case ContractType.NoQueens:
                    return card.Rank == Rank.Queen;
                case ContractType.NoMen:
                    return card.Rank == Rank.King || card.Rank == Rank.Jack;
                case ContractType.KingOfHearts:
                    return card.Suit == Suit.Hearts && card.Rank == Rank.King;
                default:
                    return false;
            }
        }

        public CompletedTrick Play(Card card)
        {
            if (IsComplete)
                throw new InvalidOperationException("the deal is complete");
            if (!LegalPlays().Contains(card))
                throw new InvalidOperationException(card + " is not a legal play for " + ToPlay);

            hands[(int)ToPlay].Remove(card);

            // Any heart on the table breaks them. Usually that's a discard on
            // another suit's lead, but a player down to nothing but hearts has to
            // lead one, and it would be daft to keep the suit shut after that.
            if (card.Suit == Suit.Hearts)
                HeartsBroken = true;

            // İlk koz oynandığı andan itibaren koz açılmış sayılır.
            if (Contract.TrumpSuit != null
                && card.Suit == Contract.TrumpSuit.Value)
                TrumpBroken = true;

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

            if (history.Count == 13 || NothingLeftCanScore())
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

        public int UnitsTaken(Seat seat)
        {
            if (seat < Seat.South || seat > Seat.East)
                throw new ArgumentOutOfRangeException(nameof(seat));

            return CountUnits()[(int)seat];
        }

        public DealScore Score()
        {
            if (!IsComplete)
                throw new InvalidOperationException("the deal is still in progress");

            var units = CountUnits();
            var points = new int[4];
            int value = Contracts.UnitValue(Contract.Type);
            for (int s = 0; s < 4; s++)
                points[s] = units[s] * value;
            return new DealScore(points, units);
        }

        // Scoring units captured per seat. Trump and no-tricks count every trick for
        // its winner, no-last-two only the final pair, and the card-penalty deals
        // charge each captured penalty card to its trick's winner.
        int[] CountUnits()
        {
            var units = new int[4];
            switch (Contract.Type)
            {
                case ContractType.Trump:
                case ContractType.NoTricks:
                    foreach (var trick in history)
                        units[(int)trick.Winner]++;
                    return units;
                case ContractType.NoLastTwo:
                    foreach (var trick in history)
                        if (trick.Number >= 12)
                            units[(int)trick.Winner]++;
                    return units;
                default:
                    foreach (var trick in history)
                        foreach (var play in trick.Plays)
                            if (IsPenaltyCard(play.Card))
                                units[(int)trick.Winner]++;
                    return units;
            }
        }

        // A card-penalty deal stops early once every card that can still score
        // has been captured. Trump, no tricks and no last two are scored by the
        // trick rather than the card, so they always play out all thirteen.
        bool NothingLeftCanScore()
        {
            if (!ScoresByCard(Contract.Type))
                return false;

            int captured = 0;
            foreach (var trick in history)
                foreach (var play in trick.Plays)
                    if (IsPenaltyCard(play.Card))
                        captured++;
            return captured == Contracts.UnitsInDeal(Contract.Type);
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

        // A trump beats any non-trump, and among trumps the higher rank wins. With no
        // trump involved, only the led suit competes.
        bool Beats(Card candidate, Card best, Suit led)
        {
            if (Contract.TrumpSuit != null)
            {
                bool candidateTrumps = candidate.Suit == Contract.TrumpSuit.Value;
                bool bestTrumps = best.Suit == Contract.TrumpSuit.Value;
                if (candidateTrumps != bestTrumps)
                    return candidateTrumps;
                if (candidateTrumps)
                    return candidate.Rank > best.Rank;
            }
            return candidate.Suit == led && candidate.Rank > best.Rank;
        }
    }
}
