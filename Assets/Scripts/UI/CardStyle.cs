using King.Core;
using UnityEngine;

namespace King.UI
{
    // Glyphs and colors for drawing cards.
    public static class CardStyle
    {
        public static readonly Color Felt = new Color(0.065f, 0.23f, 0.14f);
        public static readonly Color FeltDark = new Color(0.038f, 0.15f, 0.09f);
        public static readonly Color CardWhite = new Color(0.82f, 0.72f, 0.55f);
        public static readonly Color BackBlue = new Color(0.24f, 0.15f, 0.17f);
        public static readonly Color RedInk = new Color(0.76f, 0.045f, 0.065f);
        public static readonly Color BlackInk = new Color(0.085f, 0.08f, 0.075f);
        public static readonly Color Gold = new Color(1f, 0.85f, 0.35f);
        public static readonly Color Cream = new Color(0.95f, 0.94f, 0.88f);

        public static string RankGlyph(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ten: return "10";
                case Rank.Jack: return "J";
                case Rank.Queen: return "Q";
                case Rank.King: return "K";
                case Rank.Ace: return "A";
                default: return ((int)rank).ToString();
            }
        }

        public static string SuitGlyph(Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs: return "♣";
                case Suit.Diamonds: return "♦";
                case Suit.Hearts: return "♥";
                default: return "♠";
            }
        }

        public static Color Ink(Suit suit) =>
            suit == Suit.Hearts || suit == Suit.Diamonds ? RedInk : BlackInk;
    }
}
