using System.Collections.Generic;

namespace King.Core
{
    public sealed class CompletedTrick
    {
        // 1-based trick number within the deal.
        public int Number { get; }

        public Seat Leader { get; }
        public Seat Winner { get; }

        // The four plays in the order they hit the table.
        public IReadOnlyList<(Seat Seat, Card Card)> Plays { get; }

        internal CompletedTrick(int number, Seat leader, Seat winner, (Seat Seat, Card Card)[] plays)
        {
            Number = number;
            Leader = leader;
            Winner = winner;
            Plays = plays;
        }
    }
}
