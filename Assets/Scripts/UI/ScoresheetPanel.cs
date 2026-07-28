using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // The paper scoresheet: twenty rows of deal, caller, contract and the four
    // point columns, with a running-totals row under them. A button in the top
    // right corner shows and hides it; the cells are built once and refilled
    // after every deal.
    public sealed class ScoresheetPanel
    {
        const int Rows = Session.DealCount;

        static readonly float[] ColumnX = { 34f, 108f, 226f, 424f, 544f, 664f, 784f };
        static readonly float[] ColumnWidth = { 66f, 110f, 190f, 112f, 112f, 112f, 112f };

        readonly GameObject panel;
        readonly Text[,] cells = new Text[Rows + 1, 7];   // last row is the running totals

        public ScoresheetPanel(Transform canvas)
        {
            var toggleRect = UiKit.Rect("ScoresToggle", canvas, Vector2.one, Vector2.one,
                new Vector2(-24f, -14f), new Vector2(150f, 48f));
            var toggleImage = UiKit.RoundedImage(toggleRect, new Color(0.05f, 0.14f, 0.08f, 0.92f));
            var toggle = UiKit.MakeButton(toggleImage);
            UiKit.Fill("Label", toggleRect, "Scores", 26, CardStyle.Cream, TextAnchor.MiddleCenter);
            toggle.onClick.AddListener(() => panel.SetActive(!panel.activeSelf));

            var panelRect = UiKit.Rect("Scoresheet", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 6f), new Vector2(920f, 850f));
            var background = UiKit.RoundedImage(panelRect, new Color(0.04f, 0.12f, 0.07f, 0.97f));
            background.raycastTarget = true;   // cards under the sheet should not catch clicks
            panel = panelRect.gameObject;

            string[] headers = { "Deal", "Caller", "Contract", "South", "West", "North", "East" };
            for (int c = 0; c < 7; c++)
            {
                var header = Cell(panelRect, c, 26f, headers[c]);
                header.fontStyle = FontStyle.Bold;
                header.color = CardStyle.Gold;
            }

            for (int r = 0; r <= Rows; r++)
            {
                float y = r < Rows ? 66f + r * 34f : 66f + Rows * 34f + 14f;
                for (int c = 0; c < 7; c++)
                    cells[r, c] = Cell(panelRect, c, y, "");
                if (r == Rows)
                    for (int c = 0; c < 7; c++)
                        cells[r, c].fontStyle = FontStyle.Bold;
            }
            cells[Rows, 2].text = "Running total";

            panel.SetActive(false);
        }

        Text Cell(RectTransform parent, int column, float y, string text)
        {
            var alignment = column < 3 ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            return UiKit.Label("Cell", parent, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(ColumnX[column], -y), new Vector2(ColumnWidth[column], 30f),
                text, 22, CardStyle.Cream, alignment);
        }

        public void Refresh(Session session)
        {
            var sheet = session.Sheet;
            for (int r = 0; r < Rows; r++)
            {
                cells[r, 0].text = (r + 1).ToString();
                if (r < sheet.Count)
                {
                    var row = sheet[r];
                    cells[r, 1].text = GameText.SeatLabel(row.Caller);
                    cells[r, 2].text = GameText.ContractLabel(row.Contract);
                    for (int s = 0; s < 4; s++)
                        cells[r, 3 + s].text = row.Points[s].ToString();
                }
                else
                {
                    for (int c = 1; c < 7; c++)
                        cells[r, c].text = "";
                }
            }
            for (int s = 0; s < 4; s++)
                cells[Rows, 3 + s].text = session.Totals[s].ToString();
        }
    }
}
