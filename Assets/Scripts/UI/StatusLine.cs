using System;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Three compact status boxes in the top-left corner:
    // deal number, contract and whose turn.
    public sealed class StatusLine
    {
        readonly Text dealText;
        readonly Text contractText;
        readonly Text turnText;

        static readonly Color BoxColor =
            new Color(0.025f, 0.10f, 0.06f, 0.88f);

        static readonly Vector2 BoxSize =
            new Vector2(500f, 52f);

        const int FontSize = 30;

        public StatusLine(Transform canvas)
        {
            dealText = MakeBox(
                canvas,
                "DealStatus",
                new Vector2(18f, -18f));

            contractText = MakeBox(
                canvas,
                "ContractStatus",
                new Vector2(18f, -80f));

            turnText = MakeBox(
                canvas,
                "TurnStatus",
                new Vector2(18f, -142f));

            turnText.color = CardStyle.Gold;
        }

        static Text MakeBox(
            Transform canvas,
            string name,
            Vector2 position)
        {
            var box = UiKit.Rect(
                name + "Box",
                canvas,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                position,
                BoxSize);

            UiKit.RoundedImage(box, BoxColor);

            return UiKit.Label(
                name,
                box,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(18f, 0f),
                new Vector2(BoxSize.x - 36f, BoxSize.y),
                "",
                FontSize,
                CardStyle.Cream,
                TextAnchor.MiddleLeft);
        }

        public void Set(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                dealText.text = "";
                contractText.text = "";
                turnText.text = "";
                return;
            }

            string input = value.Trim();

            // GameBootstrap separates the three status sections
            // with multiple spaces.
            string[] sections = input.Split(
                new[] { "  " },
                StringSplitOptions.RemoveEmptyEntries);

            string dealPart =
                sections.Length > 0 ? sections[0].Trim() : "";

            string contractPart =
                sections.Length > 1 ? sections[1].Trim() : "";

            string turnPart =
                sections.Length > 2 ? sections[2].Trim() : "";

            if (dealPart.StartsWith(
                "El ",
                StringComparison.OrdinalIgnoreCase))
            {
                dealText.text = dealPart.Substring(3).Trim();
            }
            else
            {
                dealText.text = dealPart;
            }

            contractText.text = contractPart;

            if (turnPart.Equals(
                "sıra sizde",
                StringComparison.OrdinalIgnoreCase))
            {
                turnText.text = "Sıra sizde!";
            }
            else if (!string.IsNullOrEmpty(turnPart))
            {
                turnText.text =
                    char.ToUpper(turnPart[0]) +
                    turnPart.Substring(1);
            }
            else
            {
                turnText.text = "";
            }
        }

    }
}
