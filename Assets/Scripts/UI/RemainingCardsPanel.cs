using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class RemainingCardsPanel
    {
        static readonly Color PanelColor =
            new Color(0.025f, 0.085f, 0.05f, 0.98f);

        static readonly Color HeaderColor =
            new Color(0.075f, 0.14f, 0.095f, 1f);

        static readonly Color RowColorA =
            new Color(0.055f, 0.105f, 0.075f, 0.96f);

        static readonly Color RowColorB =
            new Color(0.075f, 0.125f, 0.09f, 0.96f);

        static readonly Color BlackSuitColor =
            new Color(0.88f, 0.90f, 0.84f, 1f);

        static readonly Color RedSuitColor =
            new Color(0.88f, 0.34f, 0.34f, 1f);

        readonly GameObject panel;

        readonly Card[] cards =
            new Card[52];

        readonly CanvasGroup[] groups =
            new CanvasGroup[52];

        static Color DisplaySuitColor(Suit suit)
        {
            Color ink = CardStyle.Ink(suit);

            bool isRed =
                ink.r > 0.35f &&
                ink.r > ink.g * 1.2f;

            return isRed
                ? RedSuitColor
                : BlackSuitColor;
        }

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
                new Vector2(1180f, 390f));

            var background =
                UiKit.RoundedImage(
                    panelRect,
                    PanelColor);

            background.raycastTarget = true;
            panel = panelRect.gameObject;

            var cornerLogo =
                RifkiBranding.AddCornerLogo(
                    panelRect,
                    "RemainingCardsLogo");

            cornerLogo
                .GetComponent<RectTransform>()
                .anchoredPosition +=
                    new Vector2(-132f, 132f);

            cornerLogo.SetActive(false);

            var title = UiKit.Label(
                "Title",
                panelRect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -18f),
                new Vector2(1000f, 40f),
                "Elde Kalan Kartlar",
                28,
                CardStyle.Gold,
                TextAnchor.MiddleCenter);

            title.fontStyle =
                FontStyle.Bold;

            int index = 0;

            for (int s = 0;
                 s < 4;
                 s++)
            {
                var suit =
                    (Suit)s;

                float y =
                    -72f - s * 58f;

                Color rowColor =
                    s % 2 == 0
                        ? RowColorA
                        : RowColorB;

                var suitLabel =
                    MakeCell(
                        panelRect,
                        new Vector2(24f, y),
                        new Vector2(76f, 52f),
                        CardStyle.SuitGlyph(suit),
                        34,
                        DisplaySuitColor(suit),
                        rowColor);

                suitLabel.fontStyle =
                    FontStyle.Bold;

                for (int r = (int)Rank.Ace;
                     r >= (int)Rank.Two;
                     r--)
                {
                    var card =
                        new Card(
                            suit,
                            (Rank)r);

                    cards[index] =
                        card;

                    int column =
                        (int)Rank.Ace - r;

                    float x =
                        104f + column * 80f;

                    var cell = UiKit.Rect(
                        "Card_" + card,
                        panelRect,
                        new Vector2(0f, 1f),
                        new Vector2(0f, 1f),
                        new Vector2(x, y),
                        new Vector2(76f, 52f));

                    var image =
                        cell.gameObject
                            .AddComponent<Image>();

                    image.color =
                        rowColor;

                    image.raycastTarget =
                        false;

                    groups[index] =
                        cell.gameObject
                            .AddComponent<CanvasGroup>();

                    var label =
                        UiKit.Label(
                            "Label",
                            cell,
                            new Vector2(0.5f, 0.5f),
                            new Vector2(0.5f, 0.5f),
                            Vector2.zero,
                            new Vector2(70f, 48f),
                            CardStyle.RankGlyph(
                                card.Rank),
                            22,
                            DisplaySuitColor(
                                card.Suit),
                            TextAnchor.MiddleCenter);

                    label.fontStyle =
                        FontStyle.Bold;

                    index++;
                }
            }

            toggle.onClick.AddListener(() =>
            {
                bool show =
                    !panel.activeSelf;

                panel.SetActive(show);
                cornerLogo.SetActive(show);
            });

            panel.SetActive(false);
            cornerLogo.SetActive(false);

            ResetForNewDeal();
        }

        static Text MakeCell(
            RectTransform parent,
            Vector2 position,
            Vector2 size,
            string text,
            int fontSize,
            Color textColor,
            Color backgroundColor)
        {
            var rt = UiKit.Rect(
                "GridCell",
                parent,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                position,
                size);

            var image =
                rt.gameObject
                    .AddComponent<Image>();

            image.color =
                backgroundColor;

            image.raycastTarget =
                false;

            return UiKit.Label(
                "Text",
                rt,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                size,
                text,
                fontSize,
                textColor,
                TextAnchor.MiddleCenter);
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
                    : 0.16f;
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
