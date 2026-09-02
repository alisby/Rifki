using System;
using System.Collections.Generic;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class ContractPicker
    {
        static readonly Vector2 Half =
            new Vector2(0.5f, 0.5f);

        static readonly Color PenaltyColor =
            new Color(0.82f, 0.18f, 0.18f, 1f);

        static readonly Color TrumpColor =
            new Color(0.25f, 0.48f, 0.88f, 1f);

        static readonly Color QuotaBoxColor =
            new Color(0.025f, 0.10f, 0.06f, 0.88f);

        const float UsedAlpha = 0.20f;

        readonly GameObject overlay;
        readonly GameObject contractPage;
        readonly GameObject suitPage;

        readonly Button[] contractButtons =
            new Button[7];

        readonly Text[,] penaltyQuota =
            new Text[4, 3];

        readonly Text[,] trumpQuota =
            new Text[4, 2];

        Action<ContractCall> onChosen;

        public ContractPicker(Transform canvas)
        {
            var dim = UiKit.Stretched(
                "ContractPicker",
                canvas,
                new Color(0f, 0f, 0f, 0.55f));

            dim.raycastTarget = true;
            overlay = dim.gameObject;

            var panel = UiKit.Rect(
                "Panel",
                overlay.transform,
                Half,
                Half,
                Vector2.zero,
                new Vector2(640f, 800f));

            UiKit.RoundedImage(
                panel,
                new Color(0.07f, 0.19f, 0.11f, 0.98f));

            var contractPickerLogo =
                RifkiBranding.AddCornerLogo(
                    panel,
                    "ContractPickerLogo");

            contractPickerLogo
                .GetComponent<RectTransform>()
                .anchoredPosition +=
                    new Vector2(-132f, 132f);

            contractPage = BuildContractPage(panel);
            suitPage = BuildSuitPage(panel);

            BuildQuotaBar(panel);

            overlay.SetActive(false);
        }

        GameObject BuildContractPage(RectTransform panel)
        {
            var page = UiKit.Rect(
                "Contracts",
                panel,
                Half,
                Half,
                Vector2.zero,
                panel.sizeDelta);

            UiKit.Label(
                "Title",
                page,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -24f),
                new Vector2(520f, 42f),
                "Kontratı seçin",
                34,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            for (int i = 0; i < 7; i++)
            {
                var type = (ContractType)i;

                var button = MenuButton(
                    page,
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -96f - i * 68f),
                    new Vector2(440f, 56f),
                    GameText.ContractLabel(type),
                    27,
                    CardStyle.BlackInk);

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
            var page = UiKit.Rect(
                "TrumpSuit",
                panel,
                Half,
                Half,
                Vector2.zero,
                panel.sizeDelta);

            UiKit.Label(
                "Title",
                page,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -30f),
                new Vector2(520f, 42f),
                "Koz rengini seçin",
                34,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            for (int i = 0; i < 4; i++)
            {
                var suit = (Suit)i;

                var button = MenuButton(
                    page,
                    Half,
                    new Vector2(-180f + i * 120f, 70f),
                    new Vector2(100f, 120f),
                    CardStyle.SuitGlyph(suit),
                    60,
                    CardStyle.Ink(suit));

                button.onClick.AddListener(
                    () => Choose(
                        new ContractCall(
                            ContractType.Trump,
                            suit)));
            }

            var back = MenuButton(
                page,
                Half,
                new Vector2(0f, -80f),
                new Vector2(220f, 58f),
                "Geri",
                26,
                CardStyle.BlackInk);

            back.onClick.AddListener(() =>
            {
                suitPage.SetActive(false);
                contractPage.SetActive(true);
            });

            page.gameObject.SetActive(false);

            return page.gameObject;
        }

        void BuildQuotaBar(RectTransform panel)
        {
            var bar = UiKit.Rect(
                "QuotaBar",
                panel,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 16f),
                new Vector2(190f, 108f));

            int s = (int)Seat.South;
            var seat = Seat.South;

            var box = UiKit.Rect(
                "SouthPickerQuota",
                bar,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(180f, 96f));

            UiKit.RoundedImage(box, QuotaBoxColor);

            var name = UiKit.Label(
                "Name",
                box,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -8f),
                new Vector2(170f, 28f),
                GameText.SeatLabel(seat),
                24,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            name.fontStyle = FontStyle.Bold;

            float[] x =
            {
                -58f, -29f, 0f,
                39f, 68f
            };

            for (int i = 0; i < 3; i++)
            {
                penaltyQuota[s, i] =
                    UiKit.Label(
                        "Penalty" + i,
                        box,
                        Half,
                        Half,
                        new Vector2(x[i], -13f),
                        new Vector2(28f, 30f),
                        "●",
                        29,
                        PenaltyColor,
                        TextAnchor.MiddleCenter);
            }

            for (int i = 0; i < 2; i++)
            {
                trumpQuota[s, i] =
                    UiKit.Label(
                        "Trump" + i,
                        box,
                        Half,
                        Half,
                        new Vector2(x[3 + i], -13f),
                        new Vector2(28f, 30f),
                        "▲",
                        27,
                        TrumpColor,
                        TextAnchor.MiddleCenter);
            }
        }

        static Color Alpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        void RefreshQuota(Session session)
        {
            int s = (int)Seat.South;
            var seat = Seat.South;

            int penaltiesLeft =
                session.PenaltySlotsLeft(seat);

            int trumpsLeft =
                session.TrumpCallsLeft(seat);

            for (int i = 0; i < 3; i++)
            {
                penaltyQuota[s, i].color =
                    Alpha(
                        PenaltyColor,
                        i < penaltiesLeft
                            ? 1f
                            : UsedAlpha);
            }

            for (int i = 0; i < 2; i++)
            {
                trumpQuota[s, i].color =
                    Alpha(
                        TrumpColor,
                        i < trumpsLeft
                            ? 1f
                            : UsedAlpha);
            }
        }

        Button MenuButton(
            RectTransform page,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            string label,
            int fontSize,
            Color ink)
        {
            var rt = UiKit.Rect(
                "Button",
                page,
                anchor,
                Half,
                position,
                size);

            var image =
                UiKit.RoundedImage(rt, CardStyle.Cream);

            var button =
                UiKit.MakeButton(image);

            UiKit.Fill(
                "Label",
                rt,
                label,
                fontSize,
                ink,
                TextAnchor.MiddleCenter);

            return button;
        }

        public void Show(
            Session session,
            IReadOnlyList<ContractType> available,
            Action<ContractCall> chosen)
        {
            onChosen = chosen;

            RefreshQuota(session);

            for (int i = 0; i < 7; i++)
            {
                bool open = false;

                for (int a = 0;
                     a < available.Count && !open;
                     a++)
                {
                    open = (int)available[a] == i;
                }

                contractButtons[i].interactable =
                    open;
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
