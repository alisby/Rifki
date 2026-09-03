using System;
using System.Collections.Generic;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // South's cards as a centered row of buttons along the bottom edge.
    // Illegal cards are darkened. When only part of the hand is legal,
    // legal cards rise slightly to make the available plays obvious.
    public sealed class HandView
    {
        static readonly Vector2 CardSize = new Vector2(110f, 160f);
        const float LegalLift = 24f;

        sealed class Slot
        {
            public RectTransform Root;
            public CardFace Face;
            public Button Button;
            public Image IllegalShade;
            public Card Card;
            public bool InUse;
        }

        readonly Slot[] slots = new Slot[13];

        public HandView(Transform canvas, Action<Card> onClicked)
        {
            var row = UiKit.Rect(
                "SouthHand",
                canvas,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 8f),
                new Vector2(1640f, CardSize.y + 40f));

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

                slot.Root = UiKit.Rect(
                    "HandSlot" + i,
                    row,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    CardSize);

                slot.Face = new CardFace(
                    slot.Root,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    CardSize);

                slot.Button = UiKit.MakeButton(slot.Face.Body);
                slot.Button.interactable = false;

                var shadeRt = UiKit.Rect(
                    "IllegalShade",
                    slot.Face.Root.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    CardSize);

                slot.IllegalShade = UiKit.RoundedImage(
                    shadeRt,
                    new Color(0f, 0f, 0f, 0.48f));

                slot.IllegalShade.raycastTarget = false;
                slot.IllegalShade.gameObject.SetActive(false);

                slot.Root.gameObject.SetActive(false);

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
                slot.IllegalShade.gameObject.SetActive(false);
                slot.Face.Root.GetComponent<RectTransform>().anchoredPosition =
                    Vector2.zero;
                slot.Root.gameObject.SetActive(slot.InUse);
            }
        }

        public void EnableOnly(IReadOnlyList<Card> legal)
        {
            int inUseCount = 0;
            foreach (var slot in slots)
            {
                if (slot.InUse)
                    inUseCount++;
            }

            bool allLegal =
                inUseCount > 0 && legal.Count == inUseCount;

            foreach (var slot in slots)
            {
                if (!slot.InUse)
                    continue;

                bool ok = false;

                for (int i = 0; i < legal.Count && !ok; i++)
                    ok = legal[i] == slot.Card;

                slot.Button.interactable = ok;

                if (allLegal)
                {
                    slot.IllegalShade.gameObject.SetActive(false);
                    slot.Face.Root.GetComponent<RectTransform>().anchoredPosition =
                        Vector2.zero;
                }
                else
                {
                    slot.IllegalShade.gameObject.SetActive(!ok);
                    slot.Face.Root.GetComponent<RectTransform>().anchoredPosition =
                        ok
                            ? new Vector2(0f, LegalLift)
                            : Vector2.zero;
                }
            }
        }

        public void DisableAll()
        {
            foreach (var slot in slots)
            {
                slot.Button.interactable = false;

                if (!slot.InUse)
                    continue;

                slot.IllegalShade.gameObject.SetActive(false);
                slot.Face.Root.GetComponent<RectTransform>().anchoredPosition =
                    Vector2.zero;
            }
        }
    }
}
