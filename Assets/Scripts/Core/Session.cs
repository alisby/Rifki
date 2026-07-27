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

        // 1-based number of the next (or in-progress) deal.
        public int DealNumber { get; private set; } = 1;

        public Seat Caller => (Seat)((DealNumber - 1) % 4);
        public bool IsComplete => DealNumber > DealCount;
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
            int caller = (int)Caller;
            if (penaltySlots[caller] > 0)
                for (var t = ContractType.NoTricks; t < ContractType.Trump; t++)
                    if (penaltyLeft[(int)t] > 0)
                        list.Add(t);
            if (trumpLeft[caller] > 0)
                list.Add(ContractType.Trump);
            return list;
        }

        public int PenaltyCallsLeft(ContractType t)
        {
            if (t < ContractType.NoTricks || t >= ContractType.Trump)
                throw new ArgumentOutOfRangeException(nameof(t), "not a penalty contract");
            return penaltyLeft[(int)t];
        }

        public int TrumpCallsLeft(Seat s)
        {
            if (s < Seat.South || s > Seat.East)
                throw new ArgumentOutOfRangeException(nameof(s));
            return trumpLeft[(int)s];
        }

        public DealEngine StartDeal(ContractCall call)
        {
            if (IsComplete)
                throw new InvalidOperationException("the session is over");
            if (pending != null)
                throw new InvalidOperationException("a deal is already in progress");

            int caller = (int)Caller;
            if (call.Type == ContractType.Trump)
            {
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

            var dealt = Deck.Deal(rng);
            var hands = new IReadOnlyList<Card>[4];
            for (int i = 0; i < 4; i++)
                hands[i] = dealt[i];
            pending = new DealEngine(call, hands, Caller);
            return pending;
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
            pending = null;
            DealNumber++;
        }
    }
}
