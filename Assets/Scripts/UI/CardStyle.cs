using King.Core;
using UnityEngine;

namespace King.UI
{
    // Glyphs and colors for drawing cards.
    public static class CardStyle
    {
        public static readonly Color Felt = new Color(0.086f, 0.42f, 0.20f);
        public static readonly Color FeltDark = new Color(0.055f, 0.28f, 0.13f);
        public static readonly Color CardWhite = new Color(0.98f, 0.97f, 0.94f);
        public static readonly Color BackBlue = new Color(0.16f, 0.25f, 0.55f);
        public static readonly Color RedInk = new Color(0.78f, 0.09f, 0.11f);
        public static readonly Color BlackInk = new Color(0.13f, 0.12f, 0.12f);
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
