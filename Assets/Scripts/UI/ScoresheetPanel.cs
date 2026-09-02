using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Compact score grid: game information on the left and one score
    // column for each player.
    public sealed class ScoresheetPanel
    {
        const int Rows = Session.DealCount;

        static readonly float[] ColumnX =
        {
            24f, 330f, 486f, 642f, 798f
        };

        static readonly float[] ColumnWidth =
        {
            292f, 144f, 144f, 144f, 144f
        };

        static readonly Color PanelColor =
            new Color(0.025f, 0.085f, 0.05f, 0.98f);

        static readonly Color HeaderColor =
            new Color(0.075f, 0.14f, 0.095f, 1f);

        static readonly Color RowColorA =
            new Color(0.075f, 0.095f, 0.085f, 0.98f);

        static readonly Color RowColorB =
            new Color(0.095f, 0.115f, 0.10f, 0.98f);

        static readonly Color TotalColor =
            new Color(0.075f, 0.16f, 0.095f, 1f);

        static readonly Color NegativeColor =
            new Color(0.92f, 0.56f, 0.56f, 1f);

        static readonly Color PositiveColor =
            new Color(0.72f, 0.86f, 0.68f, 1f);

        readonly GameObject panel;

        // 20 deal rows + final totals row.
        // Column 0 = game, columns 1-4 = players.
        readonly Text[,] cells = new Text[Rows + 1, 5];

        public ScoresheetPanel(Transform canvas)
        {
            var toggleRect = UiKit.Rect(
                "ScoresToggle",
                canvas,
                Vector2.one,
                Vector2.one,
                new Vector2(-24f, -14f),
                new Vector2(150f, 48f));

            var toggleImage = UiKit.RoundedImage(
                toggleRect,
                new Color(0.05f, 0.14f, 0.08f, 0.92f));

            var toggle = UiKit.MakeButton(toggleImage);

            UiKit.Fill(
                "Label",
                toggleRect,
                "Puanlar",
                26,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            var panelRect = UiKit.Rect(
                "Scoresheet",
                canvas,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 30f),
                new Vector2(970f, 640f));

            var background = UiKit.RoundedImage(
                panelRect,
                PanelColor);

            background.raycastTarget = true;
            panel = panelRect.gameObject;

            var cornerLogo =
                RifkiBranding.AddCornerLogo(
                    panelRect,
                    "ScoresheetLogo");

            cornerLogo
                .GetComponent<RectTransform>()
                .anchoredPosition +=
                    new Vector2(-132f, 132f);

            cornerLogo.SetActive(false);

            toggle.onClick.AddListener(() =>
            {
                bool show = !panel.activeSelf;
                panel.SetActive(show);
                cornerLogo.SetActive(show);
            });

            string[] headers =
            {
                "Oyun",
                GameText.SeatLabel(Seat.South),
                GameText.SeatLabel(Seat.West),
                GameText.SeatLabel(Seat.North),
                GameText.SeatLabel(Seat.East)
            };

            for (int c = 0; c < 5; c++)
            {
                var header = GridCell(
                    panelRect,
                    c,
                    26f,
                    38f,
                    headers[c],
                    HeaderColor,
                    c == 0
                        ? TextAnchor.MiddleLeft
                        : TextAnchor.MiddleCenter,
                    21);

                header.fontStyle = FontStyle.Bold;
                header.color = c == 0
                    ? CardStyle.Gold
                    : CardStyle.Cream;

                if (c > 0)
                {
                    header.resizeTextForBestFit = true;
                    header.resizeTextMinSize = 13;
                    header.resizeTextMaxSize = 21;
                }
            }

            for (int r = 0; r < Rows; r++)
            {
                float y = 72f + r * 24f;

                Color rowColor =
                    r % 2 == 0 ? RowColorA : RowColorB;

                for (int c = 0; c < 5; c++)
                {
                    cells[r, c] = GridCell(
                        panelRect,
                        c,
                        y,
                        22f,
                        "",
                        rowColor,
                        c == 0
                            ? TextAnchor.MiddleLeft
                            : TextAnchor.MiddleCenter,
                        c == 0 ? 18 : 19);
                }

                cells[r, 0].resizeTextForBestFit = true;
                cells[r, 0].resizeTextMinSize = 13;
                cells[r, 0].resizeTextMaxSize = 18;
            }

            float totalY = 72f + Rows * 24f + 12f;

            for (int c = 0; c < 5; c++)
            {
                cells[Rows, c] = GridCell(
                    panelRect,
                    c,
                    totalY,
                    30f,
                    "",
                    TotalColor,
                    c == 0
                        ? TextAnchor.MiddleLeft
                        : TextAnchor.MiddleCenter,
                    21);

                cells[Rows, c].fontStyle = FontStyle.Bold;
            }

            cells[Rows, 0].text = "Toplam";
            cells[Rows, 0].color = CardStyle.Gold;

            panel.SetActive(false);
        }

        Text GridCell(
            RectTransform parent,
            int column,
            float y,
            float height,
            string text,
            Color backgroundColor,
            TextAnchor alignment,
            int fontSize)
        {
            var rt = UiKit.Rect(
                "GridCell",
                parent,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(ColumnX[column], -y),
                new Vector2(ColumnWidth[column], height));

            var image = rt.gameObject.AddComponent<Image>();
            image.color = backgroundColor;
            image.raycastTarget = false;

            float padding = column == 0 ? 10f : 4f;

            return UiKit.Label(
                "Text",
                rt,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(padding, 0f),
                new Vector2(
                    ColumnWidth[column] - padding * 2f,
                    height),
                text,
                fontSize,
                CardStyle.Cream,
                alignment);
        }

        public void Refresh(Session session)
        {
            var sheet = session.Sheet;

            for (int r = 0; r < Rows; r++)
            {
                if (r < sheet.Count)
                {
                    var row = sheet[r];

                    cells[r, 0].text =
                        (r + 1) + ". " +
                        GameText.ContractLabel(row.Contract) +
                        " · " +
                        GameText.SeatLabel(row.Caller);

                    cells[r, 0].color =
                        row.Contract.Type == ContractType.Trump
                            ? CardStyle.Gold
                            : CardStyle.Cream;

                    for (int s = 0; s < 4; s++)
                    {
                        int points = row.Points[s];

                        cells[r, 1 + s].text =
                            points.ToString();

                        if (points < 0)
                            cells[r, 1 + s].color =
                                NegativeColor;
                        else if (points > 0)
                            cells[r, 1 + s].color =
                                PositiveColor;
                        else
                            cells[r, 1 + s].color =
                                CardStyle.Cream;
                    }
                }
                else
                {
                    for (int c = 0; c < 5; c++)
                    {
                        cells[r, c].text = "";
                        cells[r, c].color =
                            CardStyle.Cream;
                    }
                }
            }

            for (int s = 0; s < 4; s++)
            {
                cells[Rows, 1 + s].text =
                    session.Totals[s].ToString();

                cells[Rows, 1 + s].color =
                    CardStyle.Cream;
            }
        }
    }
}
