using System;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class StatusLine
    {
        readonly Text dealText;
        readonly Text callerText;
        readonly Text contractText;
        readonly Text separatorText;
        readonly Text trumpGlyph;
        readonly Text kingText;
        readonly Image contractIcon;

        string currentIconPath;

        static readonly Color BoxColor =
            new Color(0f, 0f, 0f, 0f);

        static readonly Color PenaltyColor =
            new Color(0.90f, 0.38f, 0.38f, 1f);

        static readonly Color RedSuitColor =
            new Color(0.90f, 0.34f, 0.34f, 1f);

        static readonly Color TrumpColor =
            new Color(0.36f, 0.58f, 0.94f, 1f);

        static readonly Color BlackSuitColor =
            new Color(0.88f, 0.90f, 0.84f, 1f);

        public StatusLine(Transform canvas)
        {
            var box = UiKit.Rect(
                "StatusCard",
                canvas,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(18f, -18f),
                new Vector2(600f, 112f));

            UiKit.RoundedImage(
                box,
                BoxColor);

            dealText = UiKit.Label(
                "DealText",
                box,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(266f, 16f),
                new Vector2(138f, 56f),
                "",
                64,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            dealText.fontStyle =
                FontStyle.Bold;

            callerText = UiKit.Label(
                "Caller",
                box,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(18f, 13f),
                new Vector2(190f, 48f),
                "",
                34,
                CardStyle.Cream,
                TextAnchor.MiddleLeft);

            callerText.fontStyle =
                FontStyle.Bold;

            callerText.resizeTextForBestFit = true;
            callerText.resizeTextMinSize = 24;
            callerText.resizeTextMaxSize = 34;

            separatorText = UiKit.Label(
                "Separator",
                box,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(205f, 13f),
                new Vector2(24f, 48f),
                ":",
                32,
                CardStyle.Gold,
                TextAnchor.MiddleCenter);

            var iconRect = UiKit.Rect(
                "ContractIcon",
                box,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(266f, -119f),
                new Vector2(92f, 92f));

            contractIcon =
                iconRect.gameObject
                    .AddComponent<Image>();

            contractIcon.raycastTarget = false;
            contractIcon.preserveAspect = true;
            contractIcon.color = PenaltyColor;

            trumpGlyph = UiKit.Label(
                "TrumpGlyph",
                box,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(266f, -119f),
                new Vector2(106f, 106f),
                "",
                82,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            trumpGlyph.fontStyle =
                FontStyle.Bold;

            kingText = UiKit.Label(
                "KingDeclaration",
                box,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                // Contract icon center is x=128. Keep the King label on the
                // same horizontal center and place it below the contract name.
                new Vector2(370f, -90f),
                new Vector2(100f, 34f),
                "King",
                27,
                CardStyle.Gold,
                TextAnchor.MiddleCenter);

            kingText.fontStyle = FontStyle.Bold;

            var kingShadow =
                kingText.gameObject.AddComponent<Shadow>();
            kingShadow.effectColor =
                new Color(0.20f, 0.10f, 0f, 0.85f);
            kingShadow.effectDistance =
                new Vector2(2f, -2f);

            var kingGlow =
                kingText.gameObject.AddComponent<Outline>();
            kingGlow.effectColor =
                new Color(1f, 0.76f, 0.18f, 0.55f);
            kingGlow.effectDistance =
                new Vector2(1.2f, -1.2f);

            kingText.gameObject.SetActive(false);

            contractText = UiKit.Label(
                "Contract",
                box,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                // Show the current contract directly below its icon.
                // x=28 with width=200 shares the icon's x=128 center.
                new Vector2(212f, -181f),
                new Vector2(200f, 36f),
                "",
                27,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            contractText.fontStyle =
                FontStyle.Bold;

            contractText.resizeTextForBestFit = true;
            contractText.resizeTextMinSize = 20;
            contractText.resizeTextMaxSize = 27;

            contractIcon.gameObject.SetActive(false);
            trumpGlyph.gameObject.SetActive(false);
        }

        public void SetKingDeclared(bool declared)
        {
            kingText.gameObject.SetActive(declared);
        }

        public void Set(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                dealText.text = "";
                callerText.text = "";
                contractText.text = "";
                contractIcon.gameObject.SetActive(false);
                trumpGlyph.gameObject.SetActive(false);
                return;
            }

            string input = value.Trim();

            string[] sections = input.Split(
                new[] { "  " },
                StringSplitOptions.RemoveEmptyEntries);

            string dealPart =
                sections.Length > 0
                    ? sections[0].Trim()
                    : "";

            string contractPart =
                sections.Length > 1
                    ? sections[1].Trim()
                    : "";

            if (dealPart.StartsWith(
                "El ",
                StringComparison.OrdinalIgnoreCase))
            {
                dealText.text =
                    dealPart.Substring(3).Trim();
            }
            else
            {
                dealText.text =
                    dealPart;
            }

            int colon =
                contractPart.IndexOf((char)58);

            if (colon < 0)
            {
                callerText.text = "";
                contractText.text = contractPart;
                contractIcon.gameObject.SetActive(false);
                trumpGlyph.gameObject.SetActive(false);
                return;
            }

            string caller =
                contractPart.Substring(
                    0,
                    colon).Trim();

            string contract =
                contractPart.Substring(
                    colon + 1).Trim();

            callerText.text = "";
            separatorText.text = "";

            if (contract.IndexOf(
                "Koz",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ShowTrump(contract);
                return;
            }

            string iconPath = null;

            if (contract.IndexOf(
                "El Almaz",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconPath =
                    "StatusIcons/no_tricks";
            }
            else if (contract.IndexOf(
                "Kupa Almaz",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconPath =
                    "StatusIcons/no_hearts";
            }
            else if (contract.IndexOf(
                "Kız Almaz",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconPath =
                    "StatusIcons/no_queens";
            }
            else if (contract.IndexOf(
                "Erkek Almaz",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconPath =
                    "StatusIcons/no_men";
            }
            else if (contract.IndexOf(
                "Rıfkı",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconPath =
                    "StatusIcons/rifki";
            }
            else if (contract.IndexOf(
                "Son İki",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconPath =
                    "StatusIcons/last_two";
            }

            ShowPenaltyIcon(
                iconPath,
                contract);
        }

        void ShowPenaltyIcon(
            string path,
            string contract)
        {
            trumpGlyph.gameObject.SetActive(false);

            contractText.text = contract;

            contractText.color =
                CardStyle.Cream;

            if (string.IsNullOrEmpty(path))
            {
                contractIcon.gameObject.SetActive(false);
                return;
            }

            if (currentIconPath != path ||
                contractIcon.sprite == null)
            {
                var texture =
                    Resources.Load<Texture2D>(path);

                if (texture != null)
                {
                    contractIcon.sprite =
                        Sprite.Create(
                            texture,
                            new Rect(
                                0f,
                                0f,
                                texture.width,
                                texture.height),
                            new Vector2(0.5f, 0.5f),
                            100f);

                    currentIconPath = path;
                }
            }

            contractIcon.color =
                Color.white;

            contractIcon.gameObject.SetActive(
                contractIcon.sprite != null);
        }

        void ShowTrump(string contract)
        {
            trumpGlyph.gameObject.SetActive(false);

            string path = null;

            if (contract.Contains("♣"))
                path = "StatusIcons/trump_clubs";
            else if (contract.Contains("♦"))
                path = "StatusIcons/trump_diamonds";
            else if (contract.Contains("♥"))
                path = "StatusIcons/trump_hearts";
            else if (contract.Contains("♠"))
                path = "StatusIcons/trump_spades";

            contractText.text = contract;
            contractText.color = CardStyle.Cream;

            if (string.IsNullOrEmpty(path))
            {
                contractIcon.gameObject.SetActive(false);
                return;
            }

            if (currentIconPath != path ||
                contractIcon.sprite == null)
            {
                var texture =
                    Resources.Load<Texture2D>(path);

                if (texture != null)
                {
                    contractIcon.sprite =
                        Sprite.Create(
                            texture,
                            new Rect(
                                0f,
                                0f,
                                texture.width,
                                texture.height),
                            new Vector2(0.5f, 0.5f),
                            100f);

                    currentIconPath = path;
                }
            }

            contractIcon.color = Color.white;

            contractIcon.gameObject.SetActive(
                contractIcon.sprite != null);
        }
    }
}
