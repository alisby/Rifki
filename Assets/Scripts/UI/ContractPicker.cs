using System;
using System.Collections.Generic;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Modal for South's call. First page lists the seven contracts with only the
    // ones the session still allows enabled; picking Trump slides to a second
    // page for the suit. The dimmer swallows clicks so the table underneath
    // stays untouched while the modal is up.
    public sealed class ContractPicker
    {
        static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

        readonly GameObject overlay;
        readonly GameObject contractPage;
        readonly GameObject suitPage;
        readonly Button[] contractButtons = new Button[7];

        Action<ContractCall> onChosen;

        public ContractPicker(Transform canvas)
        {
            var dim = UiKit.Stretched("ContractPicker", canvas, new Color(0f, 0f, 0f, 0.55f));
            dim.raycastTarget = true;
            overlay = dim.gameObject;

            var panel = UiKit.Rect("Panel", overlay.transform, Half, Half, Vector2.zero, new Vector2(470f, 700f));
            UiKit.RoundedImage(panel, new Color(0.07f, 0.19f, 0.11f, 0.98f));

            contractPage = BuildContractPage(panel);
            suitPage = BuildSuitPage(panel);
            overlay.SetActive(false);
        }

        GameObject BuildContractPage(RectTransform panel)
        {
            var page = UiKit.Rect("Contracts", panel, Half, Half, Vector2.zero, panel.sizeDelta);
            UiKit.Label("Title", page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f),
                new Vector2(420f, 40f), "Your call", 34, CardStyle.Cream, TextAnchor.MiddleCenter);

            for (int i = 0; i < 7; i++)
            {
                var type = (ContractType)i;
                var button = MenuButton(page, new Vector2(0.5f, 1f), new Vector2(0f, -112f - i * 76f),
                    new Vector2(380f, 62f), GameText.ContractLabel(type), 28, CardStyle.BlackInk);
                button.onClick.AddListener(() =>
                {
                    if (type == ContractType.Trump)
                    {
                        contractPage.SetActive(false);
                        suitPage.SetActive(true);
                    }
                    else
                    {
                        Choose(new ContractCall(type));
                    }
                });
                contractButtons[i] = button;
            }
            return page.gameObject;
        }

        GameObject BuildSuitPage(RectTransform panel)
        {
            var page = UiKit.Rect("TrumpSuit", panel, Half, Half, Vector2.zero, panel.sizeDelta);
            UiKit.Label("Title", page, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f),
                new Vector2(420f, 40f), "Pick the trump suit", 34, CardStyle.Cream, TextAnchor.MiddleCenter);

            for (int i = 0; i < 4; i++)
            {
                var suit = (Suit)i;
                var button = MenuButton(page, Half, new Vector2(-153f + i * 102f, 40f), new Vector2(90f, 110f),
                    CardStyle.SuitGlyph(suit), 56, CardStyle.Ink(suit));
                button.onClick.AddListener(() => Choose(new ContractCall(ContractType.Trump, suit)));
            }

            var back = MenuButton(page, Half, new Vector2(0f, -120f), new Vector2(200f, 54f), "Back", 26, CardStyle.BlackInk);
            back.onClick.AddListener(() =>
            {
                suitPage.SetActive(false);
                contractPage.SetActive(true);
            });
            page.gameObject.SetActive(false);
            return page.gameObject;
        }

        Button MenuButton(RectTransform page, Vector2 anchor, Vector2 position, Vector2 size, string label, int fontSize, Color ink)
        {
            var rt = UiKit.Rect("Button", page, anchor, Half, position, size);
            var image = UiKit.RoundedImage(rt, CardStyle.Cream);
            var button = UiKit.MakeButton(image);
            UiKit.Fill("Label", rt, label, fontSize, ink, TextAnchor.MiddleCenter);
            return button;
        }

        public void Show(IReadOnlyList<ContractType> available, Action<ContractCall> chosen)
        {
            onChosen = chosen;
            for (int i = 0; i < 7; i++)
            {
                bool open = false;
                for (int a = 0; a < available.Count && !open; a++)
                    open = (int)available[a] == i;
                contractButtons[i].interactable = open;
            }
            suitPage.SetActive(false);
            contractPage.SetActive(true);
            overlay.SetActive(true);
        }

        void Choose(ContractCall call)
        {
            overlay.SetActive(false);
            var callback = onChosen;
            onChosen = null;
            callback?.Invoke(call);
        }
    }
}
