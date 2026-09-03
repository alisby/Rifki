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

        static readonly Color PanelColor =
        new Color(0.07f, 0.19f, 0.11f, 0.98f);

        static readonly Color GoldFrameColor =
        new Color(0.88f, 0.70f, 0.28f, 1f);

        static readonly Color GoldTextColor =
        new Color(0.97f, 0.86f, 0.52f, 1f);

        static readonly Color SuitTileColor =
        new Color(0.02f, 0.24f, 0.12f, 1f);

        static readonly Color PenaltyLeftColor =
        new Color(0.60f, 0.06f, 0.06f, 1f);

        static readonly Color PenaltyRightColor =
        new Color(0.12f, 0.13f, 0.16f, 1f);

        const float UsedAlpha = 0.20f;
        const float DisabledAlpha = 0.35f;

        readonly GameObject overlay;
        readonly GameObject contractPage;

        readonly Button[] contractButtons =
        new Button[7];

        readonly Button[] suitButtons =
        new Button[4];

        readonly CanvasGroup[] contractGroups =
        new CanvasGroup[7];

        readonly CanvasGroup[] suitGroups =
        new CanvasGroup[4];

        readonly Text[,] penaltyQuota =
        new Text[4, 3];

        readonly Text[,] trumpQuota =
        new Text[4, 2];

        readonly Dictionary<string, Sprite> spriteCache =
        new Dictionary<string, Sprite>();

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
                new Vector2(0f, 55f),
                                   new Vector2(700f, 690f));

            UiKit.RoundedImage(panel, PanelColor);

            var contractPickerLogo =
            RifkiBranding.AddCornerLogo(
                panel,
                "ContractPickerLogo");

            contractPickerLogo
            .GetComponent<RectTransform>()
            .anchoredPosition +=
            new Vector2(-148f, 134f);

            contractPage = BuildContractPage(panel);

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
                        new Vector2(0f, -26f),
                        new Vector2(560f, 42f),
                        "Kontratı seçin",
                        34,
                        CardStyle.Cream,
                        TextAnchor.MiddleCenter);

            UiKit.Label(
                "Subtitle",
                page,
                new Vector2(0.5f, 1f),
                        new Vector2(0.5f, 1f),
                        new Vector2(0f, -62f),
                        new Vector2(600f, 28f),
                        "Üstte koz, altta ceza kontratları",
                        20,
                        GoldTextColor,
                        TextAnchor.MiddleCenter);

            float[] suitX =
            {
                -228f, -76f, 76f, 228f
            };

            for (int i = 0; i < 4; i++)
                BuildTrumpButton(
                    page,
                    (Suit)i,
                                 new Vector2(suitX[i], -120f));

                BuildPenaltyCard(
                    page,
                    ContractType.NoTricks,
                    new Vector2(-152f, -260f));

                BuildPenaltyCard(
                    page,
                    ContractType.NoHearts,
                    new Vector2(152f, -260f));

                BuildPenaltyCard(
                    page,
                    ContractType.NoMen,
                    new Vector2(-152f, -360f));

                BuildPenaltyCard(
                    page,
                    ContractType.NoQueens,
                    new Vector2(152f, -360f));

                BuildPenaltyCard(
                    page,
                    ContractType.KingOfHearts,
                    new Vector2(-152f, -460f));

                BuildPenaltyCard(
                    page,
                    ContractType.NoLastTwo,
                    new Vector2(152f, -460f));

                return page.gameObject;
        }

        void BuildTrumpButton(
            RectTransform page,
            Suit suit,
            Vector2 position)
        {
            var rt = UiKit.Rect(
                "Trump" + suit,
                page,
                new Vector2(0.5f, 1f),
                                new Vector2(0.5f, 1f),
                                position,
                                new Vector2(122f, 122f));

            suitGroups[(int)suit] =
            rt.gameObject.AddComponent<CanvasGroup>();

            var frame =
            UiKit.RoundedImage(rt, GoldFrameColor);

            var button =
            UiKit.MakeButton(frame);

            suitButtons[(int)suit] = button;

            var inner = UiKit.Rect(
                "Inner",
                rt,
                Half,
                Half,
                Vector2.zero,
                new Vector2(108f, 108f));

            var innerBg =
            UiKit.RoundedImage(inner, SuitTileColor);

            innerBg.raycastTarget = false;

            var iconHolder = UiKit.Rect(
                "Icon",
                inner,
                Half,
                Half,
                new Vector2(0f, -4f),
                                        new Vector2(66f, 66f));

            var icon =
            iconHolder.gameObject.AddComponent<Image>();

            icon.sprite = LoadSprite(TrumpIconPath(suit));
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var label = UiKit.Label(
                "Label",
                inner,
                new Vector2(0.5f, 0f),
                                    new Vector2(0.5f, 0f),
                                    new Vector2(0f, 11f),
                                    new Vector2(90f, 18f),
                                    "KOZ",
                                    16,
                                    GoldTextColor,
                                    TextAnchor.MiddleCenter);

            label.fontStyle = FontStyle.Bold;
            label.raycastTarget = false;

            button.onClick.AddListener(
                () => Choose(
                    new ContractCall(
                        ContractType.Trump,
                        suit)));
        }

        void BuildPenaltyCard(
            RectTransform page,
            ContractType type,
            Vector2 position)
        {
            var rt = UiKit.Rect(
                "Penalty" + type,
                page,
                new Vector2(0.5f, 1f),
                                new Vector2(0.5f, 1f),
                                position,
                                new Vector2(274f, 94f));

            contractGroups[(int)type] =
            rt.gameObject.AddComponent<CanvasGroup>();

            var frame =
            UiKit.RoundedImage(rt, GoldFrameColor);

            var button =
            UiKit.MakeButton(frame);

            contractButtons[(int)type] = button;

            var left = UiKit.Rect(
                "Left",
                rt,
                Half,
                Half,
                new Vector2(-82f, 0f),
                                  new Vector2(88f, 80f));

            var leftBg =
            UiKit.RoundedImage(left, PenaltyRightColor);

            leftBg.raycastTarget = false;

            var right = UiKit.Rect(
                "Right",
                rt,
                Half,
                Half,
                new Vector2(47f, 0f),
                                   new Vector2(162f, 80f));

            var rightBg =
            UiKit.RoundedImage(right, PenaltyRightColor);

            rightBg.raycastTarget = false;

            var iconHolder = UiKit.Rect(
                "Icon",
                left,
                Half,
                Half,
                Vector2.zero,
                new Vector2(56f, 56f));

            var icon =
            iconHolder.gameObject.AddComponent<Image>();

            icon.sprite = LoadSprite(PenaltyIconPath(type));
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var label = UiKit.Label(
                "Label",
                right,
                Half,
                Half,
                Vector2.zero,
                new Vector2(140f, 66f),
                                    PenaltyButtonLabel(type),
                                    22,
                                    GoldTextColor,
                                    TextAnchor.MiddleLeft);

            label.fontStyle = FontStyle.Bold;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 17;
            label.resizeTextMaxSize = 24;
            label.raycastTarget = false;

            button.onClick.AddListener(
                () => Choose(new ContractCall(type)));
        }

        void BuildQuotaBar(RectTransform panel)
        {
            var bar = UiKit.Rect(
                "QuotaBar",
                panel,
                new Vector2(0.5f, 0f),
                                 new Vector2(0.5f, 0f),
                                 new Vector2(0f, 5f),
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

        void SetVisualState(
            Button button,
            CanvasGroup group,
            bool enabled)
        {
            if (button != null)
                button.interactable = enabled;

            if (group != null)
            {
                group.alpha =
                enabled ? 1f : DisabledAlpha;

                group.interactable = enabled;
                group.blocksRaycasts = enabled;
            }
        }

        Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (spriteCache.TryGetValue(path, out var cached))
                return cached;

            var texture =
            Resources.Load<Texture2D>(path);

            if (texture == null)
                return null;

            var sprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    texture.width,
                    texture.height),
                    new Vector2(0.5f, 0.5f),
                          100f);

            spriteCache[path] = sprite;
            return sprite;
        }

        static string PenaltyIconPath(ContractType type)
        {
            switch (type)
            {
                case ContractType.NoTricks:
                    return "StatusIcons/no_tricks";
                case ContractType.NoHearts:
                    return "StatusIcons/no_hearts";
                case ContractType.NoQueens:
                    return "StatusIcons/no_queens";
                case ContractType.NoMen:
                    return "StatusIcons/no_men";
                case ContractType.KingOfHearts:
                    return "StatusIcons/rifki";
                case ContractType.NoLastTwo:
                    return "StatusIcons/last_two";
                default:
                    return null;
            }
        }

        static string TrumpIconPath(Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs:
                    return "StatusIcons/trump_clubs";
                case Suit.Diamonds:
                    return "StatusIcons/trump_diamonds";
                case Suit.Hearts:
                    return "StatusIcons/trump_hearts";
                default:
                    return "StatusIcons/trump_spades";
            }
        }

        static string PenaltyButtonLabel(ContractType type)
        {
            switch (type)
            {
                case ContractType.NoTricks:
                    return "El\nalmaz";
                case ContractType.NoHearts:
                    return "Kupa\nalmaz";
                case ContractType.NoMen:
                    return "Erkek\nalmaz";
                case ContractType.NoQueens:
                    return "Kız\nalmaz";
                case ContractType.KingOfHearts:
                    return "Rıfkı\nalmaz";
                case ContractType.NoLastTwo:
                    return "Son iki\nalmaz";
                default:
                    return GameText.ContractLabel(type);
            }
        }

        public void Show(
            Session session,
            IReadOnlyList<ContractType> available,
            Action<ContractCall> chosen)
        {
            onChosen = chosen;

            RefreshQuota(session);

            bool trumpOpen = false;

            for (int i = 0; i < 7; i++)
            {
                bool open = false;

                for (int a = 0;
                     a < available.Count && !open;
                a++)
                     {
                         open = (int)available[a] == i;
                     }

                     if (i == (int)ContractType.Trump)
                     {
                         trumpOpen = open;
                     }
                     else
                     {
                         SetVisualState(
                             contractButtons[i],
                             contractGroups[i],
                             open);
                     }
            }

            for (int i = 0; i < 4; i++)
            {
                SetVisualState(
                    suitButtons[i],
                    suitGroups[i],
                    trumpOpen);
            }

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
