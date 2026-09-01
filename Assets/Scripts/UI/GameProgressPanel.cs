using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Shows the six penalty contracts that are still relevant.
    // A contract stays visible after the first play.
    // After the second completed play, its box disappears.
    public sealed class GameProgressPanel
    {
        static readonly ContractType[] Types =
        {
            ContractType.NoTricks,
            ContractType.NoHearts,
            ContractType.NoQueens,
            ContractType.NoMen,
            ContractType.KingOfHearts,
            ContractType.NoLastTwo
        };

        static readonly Color PanelColor =
            new Color(0.025f, 0.10f, 0.06f, 0.90f);

        static readonly Color RowColor =
            new Color(0.045f, 0.14f, 0.085f, 0.92f);

        readonly RectTransform panel;
        readonly RectTransform[] rows = new RectTransform[Types.Length];

        const float RowWidth = 145f;
        const float RowHeight = 30f;
        const float ColumnGap = 8f;
        const float StartX = 12f;
        const float StartY = -54f;
        const float RowStep = 34f;
        const int RowsPerColumn = 3;

        public GameProgressPanel(Transform canvas)
        {
            panel = UiKit.Rect(
                "GameProgressPanel",
                canvas,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-190f, -18f),
                new Vector2(330f, 172f));

            UiKit.RoundedImage(panel, PanelColor);

            var title = UiKit.Label(
                "Title",
                panel,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(12f, -10f),
                new Vector2(200f, 30f),
                "Oyunlar",
                27,
                CardStyle.Gold,
                TextAnchor.MiddleLeft);

            title.fontStyle = FontStyle.Bold;

            for (int i = 0; i < Types.Length; i++)
            {
                rows[i] = UiKit.Rect(
                    Types[i] + "Row",
                    panel,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    Vector2.zero,
                    new Vector2(RowWidth, RowHeight));

                UiKit.RoundedImage(rows[i], RowColor);

                var label = UiKit.Label(
                    "Label",
                    rows[i],
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(10f, 0f),
                    new Vector2(RowWidth - 20f, RowHeight),
                    GameText.ContractLabel(Types[i]),
                    21,
                    CardStyle.Cream,
                    TextAnchor.MiddleLeft);

                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 14;
                label.resizeTextMaxSize = 21;
            }
        }

        public void Refresh(Session session)
        {
            int visibleIndex = 0;

            for (int i = 0; i < Types.Length; i++)
            {
                int played = 2 - session.PenaltyCallsLeft(Types[i]);
                bool show = played < 2;

                rows[i].gameObject.SetActive(show);

                if (!show)
                    continue;

                int column = visibleIndex / RowsPerColumn;
                int row = visibleIndex % RowsPerColumn;

                float x = StartX + column * (RowWidth + ColumnGap);
                float y = StartY - row * RowStep;

                rows[i].anchoredPosition = new Vector2(x, y);

                visibleIndex++;
            }
        }
    }
}
