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
        readonly Text pip;

        public GameObject Root { get; }
        public Image Body { get; }

        public CardFace(Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var rt = UiKit.Rect("Card", parent, anchor, pivot, position, size);
            Root = rt.gameObject;
            Body = UiKit.RoundedImage(rt, CardStyle.CardWhite);

            int cornerSize = Mathf.RoundToInt(size.y * 0.17f);
            int pipSize = Mathf.RoundToInt(size.y * 0.38f);
            corner = UiKit.Label("Corner", rt, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(size.x * 0.08f, -size.y * 0.03f), new Vector2(size.x * 0.85f, size.y * 0.3f),
                "", cornerSize, CardStyle.BlackInk, TextAnchor.UpperLeft);
            pip = UiKit.Fill("Pip", rt, "", pipSize, CardStyle.BlackInk, TextAnchor.MiddleCenter);
        }

        public void Bind(Card card)
        {
            var ink = CardStyle.Ink(card.Suit);
            corner.text = CardStyle.RankGlyph(card.Rank) + CardStyle.SuitGlyph(card.Suit);
            corner.color = ink;
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
