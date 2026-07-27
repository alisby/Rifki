using System.Collections.Generic;

namespace King.Core
{
    public sealed class ScoreRow
    {
        public int DealNumber { get; }
        public Seat Caller { get; }
        public ContractCall Contract { get; }

        // Points by Seat index for this deal alone; sums to the contract's deal total.
        public IReadOnlyList<int> Points { get; }

        internal ScoreRow(int dealNumber, Seat caller, ContractCall contract, int[] points)
        {
            DealNumber = dealNumber;
            Caller = caller;
            Contract = contract;
            Points = points;
        }
    }
}
