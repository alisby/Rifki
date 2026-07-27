using System;

namespace King.Core
{
    public readonly struct Card : IEquatable<Card>
    {
        public Suit Suit { get; }
        public Rank Rank { get; }

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public bool Equals(Card other) => Suit == other.Suit && Rank == other.Rank;

        public override bool Equals(object obj) => obj is Card other && Equals(other);

        public override int GetHashCode() => ((int)Suit << 4) | (int)Rank;

        public static bool operator ==(Card left, Card right) => left.Equals(right);

        public static bool operator !=(Card left, Card right) => !left.Equals(right);

        public override string ToString() => $"{RankChar(Rank)}{SuitChar(Suit)}";

        static char RankChar(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ten: return 'T';
                case Rank.Jack: return 'J';
                case Rank.Queen: return 'Q';
                case Rank.King: return 'K';
                case Rank.Ace: return 'A';
                default: return (char)('0' + (int)rank);
            }
        }

        static char SuitChar(Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs: return '♣';
                case Suit.Diamonds: return '♦';
                case Suit.Hearts: return '♥';
                default: return '♠';
            }
        }
    }
}
