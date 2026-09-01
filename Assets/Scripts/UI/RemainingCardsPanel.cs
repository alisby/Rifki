using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class RemainingCardsPanel
    {
        static readonly Color PanelColor =
            new Color(0.025f, 0.085f, 0.05f, 0.98f);

        static readonly Color CellColor =
            new Color(0.93f, 0.91f, 0.84f, 1f);

        readonly GameObject panel;

        readonly Card[] cards =
            new Card[52];

        readonly CanvasGroup[] groups =
            new CanvasGroup[52];

        public RemainingCardsPanel(Transform canvas)
        {
            var toggleRect = UiKit.Rect(
                "RemainingCardsToggle",
                canvas,
                Vector2.one,
                Vector2.one,
                new Vector2(-24f, -72f),
                new Vector2(150f, 48f));

            var toggleImage =
                UiKit.RoundedImage(
                    toggleRect,
                    new Color(
                        0.05f,
                        0.14f,
                        0.08f,
                        0.92f));

            var toggle =
                UiKit.MakeButton(toggleImage);

            UiKit.Fill(
                "Label",
                toggleRect,
                "Kalanlar",
                24,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            var panelRect = UiKit.Rect(
                "RemainingCards",
                canvas,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f),
                new Vector2(1180f, 370f));

            var background =
                UiKit.RoundedImage(
                    panelRect,
                    PanelColor);

            background.raycastTarget = true;

            panel = panelRect.gameObject;

            var title = UiKit.Label(
                "Title",
                panelRect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -18f),
                new Vector2(1100f, 42f),
                "Elde Kalan Kartlar",
                30,
                CardStyle.Gold,
                TextAnchor.MiddleCenter);

            title.fontStyle = FontStyle.Bold;

            int index = 0;

            for (int s = 0; s < 4; s++)
            {
                var suit = (Suit)s;

                float y =
                    -82f - s * 66f;

                var suitLabel = UiKit.Label(
                    "Suit",
                    panelRect,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(42f, y),
                    new Vector2(50f, 50f),
                    CardStyle.SuitGlyph(suit),
                    38,
                    CardStyle.Ink(suit),
                    TextAnchor.MiddleCenter);

                suitLabel.fontStyle =
                    FontStyle.Bold;

                for (int r = (int)Rank.Ace;
                     r >= (int)Rank.Two;
                     r--)
                {
                    var card =
                        new Card(suit, (Rank)r);

                    cards[index] = card;

                    float x =
                        100f +
                        ((int)Rank.Ace - r) * 80f;

                    var cell = UiKit.Rect(
                        "Card_" + card,
                        panelRect,
                        new Vector2(0f, 1f),
                        new Vector2(0f, 1f),
                        new Vector2(x, y),
                        new Vector2(70f, 50f));

                    UiKit.RoundedImage(
                        cell,
                        CellColor);

                    groups[index] =
                        cell.gameObject
                            .AddComponent<CanvasGroup>();

                    var label = UiKit.Label(
                        "Label",
                        cell,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        Vector2.zero,
                        new Vector2(66f, 46f),
                        CardStyle.RankGlyph(card.Rank)
                            + CardStyle.SuitGlyph(card.Suit),
                        23,
                        CardStyle.Ink(card.Suit),
                        TextAnchor.MiddleCenter);

                    label.fontStyle =
                        FontStyle.Bold;

                    index++;
                }
            }

            toggle.onClick.AddListener(
                () =>
                    panel.SetActive(
                        !panel.activeSelf));

            panel.SetActive(false);

            ResetForNewDeal();
        }

        public void ResetForNewDeal()
        {
            for (int i = 0;
                 i < groups.Length;
                 i++)
            {
                groups[i].alpha = 1f;
            }
        }

        public void Refresh(DealEngine deal)
        {
            if (deal == null)
                return;

            for (int i = 0;
                 i < cards.Length;
                 i++)
            {
                groups[i].alpha =
                    IsStillInHand(
                        deal,
                        cards[i])
                    ? 1f
                    : 0.20f;
            }
        }

        static bool IsStillInHand(
            DealEngine deal,
            Card target)
        {
            for (int s = 0; s < 4; s++)
            {
                var hand =
                    deal.HandOf((Seat)s);

                for (int i = 0;
                     i < hand.Count;
                     i++)
                {
                    if (hand[i] == target)
                        return true;
                }
            }

            return false;
        }
    }
}
