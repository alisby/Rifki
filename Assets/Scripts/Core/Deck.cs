using System;
using System.Collections.Generic;

namespace King.Core
{
    public static class Deck
    {
        // All 52 cards: clubs, diamonds, hearts, spades, each two through ace.
        public static Card[] Standard()
        {
            var cards = new Card[52];
            int i = 0;
            for (int s = 0; s < 4; s++)
                for (int r = (int)Rank.Two; r <= (int)Rank.Ace; r++)
                    cards[i++] = new Card((Suit)s, (Rank)r);
            return cards;
        }

        // Fisher-Yates shuffle on the injected rng, then four hands of 13 in seat order.
        public static Card[][] Deal(Random rng)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            var cards = Standard();
            for (int i = cards.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }

            var hands = new Card[4][];
            for (int h = 0; h < 4; h++)
            {
                var hand = new Card[13];
                Array.Copy(cards, h * 13, hand, 0, 13);
                SortHand(hand);
                hands[h] = hand;
            }
            return hands;
        }

        // Display and hand order: group by suit, high card first within the suit.
        internal static int CompareInHand(Card a, Card b)
        {
            if (a.Suit != b.Suit) return a.Suit.CompareTo(b.Suit);
            return b.Rank.CompareTo(a.Rank);
        }

        internal static void SortHand(Card[] hand) => Array.Sort(hand, CompareInHand);

        internal static void SortHand(List<Card> hand) => hand.Sort(CompareInHand);
    }
}
