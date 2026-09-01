using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Six penalty contracts remain permanently visible.
    // Played contracts fade instead of disappearing.
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
        readonly RectTransform[] rows =
            new RectTransform[Types.Length];

        readonly CanvasGroup[] rowGroups =
            new CanvasGroup[Types.Length];

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
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -10f),
                new Vector2(306f, 30f),
                "Oyunlar",
                27,
                CardStyle.Gold,
                TextAnchor.MiddleCenter);

            title.fontStyle = FontStyle.Bold;

            for (int i = 0; i < Types.Length; i++)
            {
                int column = i / RowsPerColumn;
                int row = i % RowsPerColumn;

                float x =
                    StartX + column * (RowWidth + ColumnGap);

                float y =
                    StartY - row * RowStep;

                rows[i] = UiKit.Rect(
                    Types[i] + "Row",
                    panel,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(x, y),
                    new Vector2(RowWidth, RowHeight));

                UiKit.RoundedImage(rows[i], RowColor);

                rowGroups[i] =
                    rows[i].gameObject.AddComponent<CanvasGroup>();

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
            for (int i = 0; i < Types.Length; i++)
            {
                int played =
                    2 - session.PenaltyCallsLeft(Types[i]);

                // Hic oynanmadi: tam gorunur.
                // Bir kez oynandi: orta derecede soluk.
                // Iki kez oynandi: belirgin bicimde soluk.
                rowGroups[i].alpha =
                    played <= 0 ? 1f :
                    played == 1 ? 0.58f :
                    0.25f;
            }
        }
    }
}
