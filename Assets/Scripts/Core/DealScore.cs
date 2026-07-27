using System.Collections.Generic;

namespace King.Core
{
    public sealed class DealScore
    {
        // Points by Seat index; always sums to Contracts.DealTotal for the deal's contract.
        public IReadOnlyList<int> Points { get; }

        // Tricks taken, or penalty cards captured, by Seat index.
        public IReadOnlyList<int> UnitsTaken { get; }

        internal DealScore(int[] points, int[] unitsTaken)
        {
            Points = points;
            UnitsTaken = unitsTaken;
        }
    }
}
