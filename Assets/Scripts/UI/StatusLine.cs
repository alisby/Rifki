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
            new Vector2(340f, 52f);

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

            int turnIndex = input.LastIndexOf(
                " sıra ",
                StringComparison.OrdinalIgnoreCase);

            string mainPart = input;
            string turnPart = "";

            if (turnIndex >= 0)
            {
                mainPart = input.Substring(0, turnIndex).Trim();
                turnPart = input.Substring(turnIndex + 1).Trim();
            }

            string[] parts = mainPart.Split(
                new[] { ' ' },
                3,
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2 &&
                parts[0].Equals("El", StringComparison.OrdinalIgnoreCase) &&
                parts[1].Contains("/"))
            {
                dealText.text = parts[1];
                contractText.text =
                    parts.Length >= 3 ? parts[2] : "";
            }
            else
            {
                dealText.text = "";
                contractText.text = mainPart;
            }

            if (!string.IsNullOrEmpty(turnPart))
            {
                turnPart =
                    char.ToUpper(turnPart[0]) +
                    turnPart.Substring(1);

                if (!turnPart.EndsWith("!"))
                    turnPart += "!";

                turnText.text = turnPart;
            }
            else
            {
                turnText.text = "";
            }
        }
    }
}
