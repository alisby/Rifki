using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // One face-up card: rounded white body, rank+suit in the corner, big suit
    // pip in the middle. Rebindable so hand slots and trick slots can reuse it.
    public sealed class CardFace
    {
        readonly Text corner;
        readonly Text bottomCorner;
        readonly Text pip;

        public GameObject Root { get; }
        public Image Body { get; }

        public CardFace(Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var rt = UiKit.Rect("Card", parent, anchor, pivot, position, size);
            Root = rt.gameObject;
            Body = UiKit.RoundedImage(rt, CardStyle.CardWhite);

            var shadow = Body.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.28f);
            shadow.effectDistance = new Vector2(3f, -3f);
            shadow.useGraphicAlpha = true;

            var outline = Body.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.16f, 0.16f, 0.14f, 0.40f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;

            int cornerSize = Mathf.RoundToInt(size.y * 0.18f);
            int pipSize = Mathf.RoundToInt(size.y * 0.34f);
            corner = UiKit.Label("Corner", rt, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(size.x * 0.08f, -size.y * 0.03f), new Vector2(size.x * 0.85f, size.y * 0.3f),
                "", cornerSize, CardStyle.BlackInk, TextAnchor.UpperLeft);

            corner.fontStyle = FontStyle.Bold;

            bottomCorner = UiKit.Label("BottomCorner", rt,
                new Vector2(1f, 0f), new Vector2(0f, 1f),
                new Vector2(-size.x * 0.08f, size.y * 0.03f),
                new Vector2(size.x * 0.85f, size.y * 0.3f),
                "", cornerSize, CardStyle.BlackInk, TextAnchor.UpperLeft);

            bottomCorner.fontStyle = FontStyle.Bold;
            bottomCorner.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);

            pip = UiKit.Fill("Pip", rt, "", pipSize,
                CardStyle.BlackInk, TextAnchor.MiddleCenter);
        }

        public void Bind(Card card)
        {
            var ink = CardStyle.Ink(card.Suit);
            corner.text = CardStyle.RankGlyph(card.Rank);
            corner.color = ink;

            bottomCorner.text =
                CardStyle.RankGlyph(card.Rank);
            bottomCorner.color = ink;

            pip.text = CardStyle.SuitGlyph(card.Suit);
            pip.color = ink;
        }

        public void SetVisible(bool visible)
        {
            if (Root.activeSelf != visible)
                Root.SetActive(visible);
        }
    }
}
