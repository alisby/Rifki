using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Face-up playing card with conventional corner indices,
    // a large central suit mark and restrained depth effects.
    public sealed class CardFace
    {
        readonly Text cornerRank;
        readonly Text cornerSuit;
        readonly Text bottomRank;
        readonly Text bottomSuit;
        readonly Text pip;

        public GameObject Root { get; }
        public Image Body { get; }

        public CardFace(
            Transform parent,
            Vector2 anchor,
            Vector2 pivot,
            Vector2 position,
            Vector2 size)
        {
            var rt = UiKit.Rect(
                "Card",
                parent,
                anchor,
                pivot,
                position,
                size);

            Root = rt.gameObject;

            Body = UiKit.RoundedImage(
                rt,
                new Color(0.28f, 0.24f, 0.16f, 1f));



            // Slightly inset warm paper surface.
            var innerRt = UiKit.Rect(
                "CardInner",
                rt,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(
                    size.x - 4f,
                    size.y - 4f));

            var inner =
                UiKit.RoundedImage(
                    innerRt,
                    new Color(
                        0.86f,
                        0.78f,
                        0.62f,
                        0.97f));

            inner.raycastTarget = false;


            int rankSize =
                Mathf.RoundToInt(size.y * 0.205f);

            int suitSize =
                Mathf.RoundToInt(size.y * 0.13f);

            int pipSize =
                Mathf.RoundToInt(size.y * 0.285f);

            cornerRank = UiKit.Label(
                "CornerRank",
                rt,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(
                    size.x * 0.09f,
                    -size.y * 0.025f),
                new Vector2(
                    size.x * 0.40f,
                    size.y * 0.22f),
                "",
                rankSize,
                CardStyle.BlackInk,
                TextAnchor.UpperLeft);

            cornerRank.fontStyle =
                FontStyle.Bold;

            cornerSuit = UiKit.Label(
                "CornerSuit",
                rt,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(
                    size.x * 0.085f,
                    -size.y * 0.205f),
                new Vector2(
                    size.x * 0.30f,
                    size.y * 0.20f),
                "",
                suitSize,
                CardStyle.BlackInk,
                TextAnchor.UpperLeft);

            cornerSuit.fontStyle =
                FontStyle.Bold;

            bottomRank = UiKit.Label(
                "BottomRank",
                rt,
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(
                    -size.x * 0.09f,
                    size.y * 0.025f),
                new Vector2(
                    size.x * 0.40f,
                    size.y * 0.22f),
                "",
                rankSize,
                CardStyle.BlackInk,
                TextAnchor.UpperLeft);

            bottomRank.fontStyle =
                FontStyle.Bold;

            bottomRank.rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, 180f);

            bottomSuit = UiKit.Label(
                "BottomSuit",
                rt,
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(
                    -size.x * 0.085f,
                    size.y * 0.205f),
                new Vector2(
                    size.x * 0.30f,
                    size.y * 0.20f),
                "",
                suitSize,
                CardStyle.BlackInk,
                TextAnchor.UpperLeft);

            bottomSuit.fontStyle =
                FontStyle.Bold;

            bottomSuit.rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, 180f);

            pip = UiKit.Fill(
                "Pip",
                rt,
                "",
                pipSize,
                CardStyle.BlackInk,
                TextAnchor.MiddleCenter);

            pip.fontStyle =
                FontStyle.Bold;

            var pipShadow =
                pip.gameObject.AddComponent<Shadow>();

            pipShadow.effectColor =
                new Color(0f, 0f, 0f, 0.16f);

            pipShadow.effectDistance =
                new Vector2(1.5f, -1.5f);

            pipShadow.useGraphicAlpha = true;
        }

        public void Bind(Card card)
        {
            var ink =
                CardStyle.Ink(card.Suit);

            string rank =
                CardStyle.RankGlyph(card.Rank);

            string suit =
                CardStyle.SuitGlyph(card.Suit);

            cornerRank.text = rank;
            cornerRank.color = ink;

            cornerSuit.text = suit;
            cornerSuit.color = ink;

            bottomRank.text = rank;
            bottomRank.color = ink;

            bottomSuit.text = suit;
            bottomSuit.color = ink;

            pip.text = suit;
            pip.color = ink;
        }

        public void SetVisible(bool visible)
        {
            if (Root.activeSelf != visible)
                Root.SetActive(visible);
        }
    }
}
