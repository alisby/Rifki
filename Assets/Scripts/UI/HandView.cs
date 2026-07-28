using System;
using System.Collections.Generic;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // South's cards as a centered row of buttons along the bottom edge. Thirteen
    // slots are built once and rebound each refresh; unused ones are hidden and
    // the layout group closes the gap.
    public sealed class HandView
    {
        static readonly Vector2 CardSize = new Vector2(110f, 160f);

        sealed class Slot
        {
            public CardFace Face;
            public Button Button;
            public Card Card;
            public bool InUse;
        }

        readonly Slot[] slots = new Slot[13];

        public HandView(Transform canvas, Action<Card> onClicked)
        {
            var row = UiKit.Rect("SouthHand", canvas, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 18f), new Vector2(1640f, CardSize.y + 10f));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = new Slot();
                slot.Face = new CardFace(row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, CardSize);
                slot.Button = UiKit.MakeButton(slot.Face.Body);
                slot.Button.interactable = false;
                slot.Face.SetVisible(false);   // nothing to show until the first deal
                var captured = slot;
                slot.Button.onClick.AddListener(() =>
                {
                    if (captured.InUse)
                        onClicked(captured.Card);
                });
                slots[i] = slot;
            }
        }

        public void Show(IReadOnlyList<Card> cards)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.InUse = i < cards.Count;
                if (slot.InUse)
                {
                    slot.Card = cards[i];
                    slot.Face.Bind(cards[i]);
                }
                slot.Button.interactable = false;
                slot.Face.SetVisible(slot.InUse);
            }
        }

        public void EnableOnly(IReadOnlyList<Card> legal)
        {
            foreach (var slot in slots)
            {
                if (!slot.InUse)
                    continue;
                bool ok = false;
                for (int i = 0; i < legal.Count && !ok; i++)
                    ok = legal[i] == slot.Card;
                slot.Button.interactable = ok;
            }
        }

        public void DisableAll()
        {
            foreach (var slot in slots)
                slot.Button.interactable = false;
        }
    }
}
