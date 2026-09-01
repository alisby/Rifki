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

        public GameProgressPanel(Transform canvas)
        {
            panel = UiKit.Rect(
                "GameProgressPanel",
                canvas,
                new Vector2(0.5f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-165f, -18f),
                new Vector2(300f, 310f));

            UiKit.RoundedImage(panel, PanelColor);

            var title = UiKit.Label(
                "Title",
                panel,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(16f, -12f),
                new Vector2(260f, 36f),
                "Oyunlar",
                28,
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
                    new Vector2(12f, -58f - i * 40f),
                    new Vector2(276f, 34f));

                UiKit.RoundedImage(rows[i], RowColor);

                UiKit.Label(
                    "Label",
                    rows[i],
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(12f, 0f),
                    new Vector2(252f, 34f),
                    GameText.ContractLabel(Types[i]),
                    23,
                    CardStyle.Cream,
                    TextAnchor.MiddleLeft);
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

                rows[i].anchoredPosition =
                    new Vector2(12f, -58f - visibleIndex * 40f);

                visibleIndex++;
            }
        }
    }
}
