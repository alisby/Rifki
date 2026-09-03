using System;
using System.Collections.Generic;

namespace King.Core
{
    public sealed class Session
    {
        public const int DealCount = 20;

        readonly Random rng;
        readonly int[] totals = new int[4];
        readonly IReadOnlyList<int> totalsView;
        readonly List<ScoreRow> sheet = new List<ScoreRow>();
        readonly IReadOnlyList<ScoreRow> sheetView;

        // Session-wide quota per penalty contract, indexed by ContractType; the
        // Trump slot stays at zero and is never read.
        readonly int[] penaltyLeft = { 2, 2, 2, 2, 2, 2, 0 };

        // Per player: trump calls still owed and penalty calls still open. They
        // always add up to that player's remaining calls, so "must trump now" is
        // simply penaltySlots hitting zero, and "can't trump" is trumpLeft at zero.
        readonly int[] trumpLeft = { 2, 2, 2, 2 };
        readonly int[] penaltySlots = { 3, 3, 3, 3 };

        DealEngine pending;

        // Cards for the deal about to be called. Held here because the caller has
        // to see their hand before naming a contract.
        IReadOnlyList<Card>[] dealtHands;

        // The first caller is whoever holds the two of diamonds.
        Seat? firstCaller;

        // 1-based number of the next (or in-progress) deal.
        public int DealNumber { get; private set; } = 1;

        public Seat Caller
        {
            get
            {
                if (!firstCaller.HasValue)
                    return Seat.South;

                Seat caller = firstCaller.Value;

                for (int n = 1; n < DealNumber; n++)
                    caller = RightOf(caller);

                return caller;
            }
        }

        // Contract calling rotates counter-clockwise: to the player on the right.
        static Seat RightOf(Seat seat)
        {
            switch (seat)
            {
                case Seat.South: return Seat.East;
                case Seat.East: return Seat.North;
                case Seat.North: return Seat.West;
                default: return Seat.South;
            }
        }
        public bool RifkiEnded { get; private set; }
        public Seat? RifkiDeclarer { get; private set; }
        public bool RifkiSucceeded { get; private set; }

        public bool IsComplete => RifkiEnded || DealNumber > DealCount;
        public IReadOnlyList<int> Totals => totalsView;
        public IReadOnlyList<ScoreRow> Sheet => sheetView;

        public Session(int seed)
        {
            rng = new Random(seed);
            totalsView = Array.AsReadOnly(totals);
            sheetView = sheet.AsReadOnly();
        }

        public IReadOnlyList<ContractType> AvailableContracts()
        {
            var list = new List<ContractType>();
            if (IsComplete)
                return list;
            // İlk elde gerçek kontrat seçicisi, karo 2 bulunan el
            // dağıtılmadan bilinemez. Kontrat listesini oluşturmadan önce
            // ilk dağıtımı gerçekleştirerek Caller değerini sabitle.
            if (!firstCaller.HasValue)
                DealHands();

            int caller = (int)Caller;
            if (penaltySlots[caller] > 0)
                for (var t = ContractType.NoTricks; t < ContractType.Trump; t++)
                    if (penaltyLeft[(int)t] > 0)
                        list.Add(t);
            // İlk dört oyunda koz kontratı seçilemez.
            if (DealNumber > 4 && trumpLeft[caller] > 0)
                list.Add(ContractType.Trump);
            return list;
        }

        public int PenaltyCallsLeft(ContractType t)
        {
            if (t < ContractType.NoTricks || t >= ContractType.Trump)
                throw new ArgumentOutOfRangeException(nameof(t), "not a penalty contract");
            return penaltyLeft[(int)t];
        }

        public int PenaltySlotsLeft(Seat s)
        {
            if (s < Seat.South || s > Seat.East)
                throw new ArgumentOutOfRangeException(nameof(s));
            return penaltySlots[(int)s];
        }

        public int TrumpCallsLeft(Seat s)
        {
            if (s < Seat.South || s > Seat.East)
                throw new ArgumentOutOfRangeException(nameof(s));
            return trumpLeft[(int)s];
        }

        // Deals the next hand without committing to a contract. Calling it twice
        // before the deal starts hands back the same cards, so a player can study
        // them, back out of the picker, and come back to the same thirteen.
        public IReadOnlyList<Card>[] DealHands()
        {
            if (IsComplete)
                throw new InvalidOperationException("the session is over");
            if (pending != null)
                throw new InvalidOperationException("a deal is already in progress");

            if (dealtHands == null)
            {
                var dealt = Deck.Deal(rng);
                dealtHands = new IReadOnlyList<Card>[4];
                for (int i = 0; i < 4; i++)
                    dealtHands[i] = Array.AsReadOnly(dealt[i]);

                // Only the first deal determines the starting caller.
                // Find the player holding the two of diamonds.
                if (!firstCaller.HasValue)
                {
                    for (int i = 0; i < 4 && !firstCaller.HasValue; i++)
                    {
                        foreach (var card in dealtHands[i])
                        {
                            if (card.Suit == Suit.Diamonds && card.Rank == Rank.Two)
                            {
                                firstCaller = (Seat)i;
                                break;
                            }
                        }
                    }

                    if (!firstCaller.HasValue)
                        throw new InvalidOperationException("two of diamonds was not found");
                }
            }
            return (IReadOnlyList<Card>[])dealtHands.Clone();
        }

        public void RedealUnstarted()
        {
            if (pending != null)
                throw new InvalidOperationException("a deal is already in progress");
            dealtHands = null;
        }

        public DealEngine StartDeal(ContractCall call)
        {
            if (IsComplete)
                throw new InvalidOperationException("the session is over");
            if (pending != null)
                throw new InvalidOperationException("a deal is already in progress");

            // İlk elde Caller hesaplanmadan önce kartların dağıtılmış
            // olması gerekir. Aksi halde kota Southtan düşüp oyun başka
            // oyuncu adına başlayabilir.
            var hands = DealHands();
            int caller = (int)Caller;

            if (call.Type == ContractType.Trump)
            {
                // Arayüz veya yapay zekâ bu kuralı aşmaya çalışsa bile
                // ilk dört oyunda koz kontratı başlatılamaz.
                if (DealNumber <= 4)
                    throw new InvalidOperationException("trump cannot be called in the first four deals");

                if (trumpLeft[caller] == 0)
                    throw new InvalidOperationException(Caller + " has already called trump twice");
                trumpLeft[caller]--;
            }
            else
            {
                if (penaltySlots[caller] == 0)
                    throw new InvalidOperationException(Caller + " must call trump now");
                if (penaltyLeft[(int)call.Type] == 0)
                    throw new InvalidOperationException(call.Type + " has already been called twice this session");
                penaltySlots[caller]--;
                penaltyLeft[(int)call.Type]--;
            }

            // Quota checks run first so a rejected call never burns a shuffle.
            pending = new DealEngine(call, hands, Caller);
            dealtHands = null;
            return pending;
        }

        public void CancelDeal()
        {
            if (pending == null)
                throw new InvalidOperationException("no deal has been started");

            int caller = (int)Caller;

            // StartDeal sırasında harcanan kontrat hakkını geri ver.
            if (pending.Contract.Type == ContractType.Trump)
            {
                trumpLeft[caller]++;
            }
            else
            {
                penaltySlots[caller]++;
                penaltyLeft[(int)pending.Contract.Type]++;
            }

            // El numarası ve çağıran değişmez. Bir sonraki DealHands
            // aynı oyuncu için yeni kart dağıtır.
            pending = null;
            dealtHands = null;
        }

        public void FinishDeal()
        {
            if (pending == null)
                throw new InvalidOperationException("no deal has been started");

            // Score() throws while the deal is still being played, which is exactly
            // the guard this method needs.
            var score = pending.Score();
            var points = new int[4];
            for (int s = 0; s < 4; s++)
            {
                points[s] = score.Points[s];
                totals[s] += points[s];
            }
            sheet.Add(new ScoreRow(DealNumber, Caller, pending.Contract, points));

            if (pending.Contract.RifkiDeclared)
            {
                RifkiEnded = true;
                RifkiDeclarer = Caller;
                RifkiSucceeded = pending.RifkiSucceeded;
            }

            pending = null;
            DealNumber++;
        }
    }
}
