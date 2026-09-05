using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class ScoresheetPanel
    {
        const int HistoryRows = Session.DealCount;
        const int SummaryRows = 7;

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
            new Color(0.055f, 0.105f, 0.075f, 0.96f);

        static readonly Color RowColorB =
            new Color(0.075f, 0.125f, 0.09f, 0.96f);

        static readonly Color TotalColor =
            new Color(0.075f, 0.18f, 0.105f, 1f);

        static readonly Color NegativeColor =
            new Color(0.92f, 0.48f, 0.48f, 1f);

        static readonly Color PositiveColor =
            new Color(0.68f, 0.88f, 0.64f, 1f);

        static readonly Color PenaltyColor =
            new Color(0.88f, 0.32f, 0.32f, 1f);

        static readonly Color TrumpColor =
            new Color(0.36f, 0.58f, 0.94f, 1f);

        static readonly Color InactiveTabColor =
            new Color(0.055f, 0.12f, 0.075f, 1f);

        static readonly Color ActiveTabColor =
            new Color(0.16f, 0.25f, 0.16f, 1f);

        readonly GameObject panel;
        readonly GameObject summaryPage;
        readonly GameObject historyPage;
        readonly GameObject cornerLogo;

        readonly Image summaryTabImage;
        readonly Image historyTabImage;

        readonly Text[,] summaryCells =
            new Text[SummaryRows + 1, 5];

        readonly Text[,] historyCells =
            new Text[HistoryRows + 1, 5];

        public ScoresheetPanel(Transform canvas)
        {
            var toggleRect = UiKit.Rect(
                "ScoresToggle",
                canvas,
                Vector2.one,
                Vector2.one,
                new Vector2(-24f, -130f),
                new Vector2(150f, 48f));

            var toggleImage = UiKit.RoundedImage(
                toggleRect,
                new Color(0.05f, 0.14f, 0.08f, 0.92f));

            var toggle = UiKit.MakeButton(toggleImage);

            UiKit.Fill(
                "Label",
                toggleRect,
                "Puanlar",
                24,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            var panelRect = UiKit.Rect(
                "Scoresheet",
                canvas,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 30f),
                new Vector2(970f, 640f));

            var background =
                UiKit.RoundedImage(panelRect, PanelColor);

            background.raycastTarget = true;
            panel = panelRect.gameObject;

            cornerLogo =
                RifkiBranding.AddCornerLogo(
                    panelRect,
                    "ScoresheetLogo");

            cornerLogo
                .GetComponent<RectTransform>()
                .anchoredPosition +=
                    new Vector2(-132f, 132f);

            // Sekmeler
            var summaryTabRect = UiKit.Rect(
                "SummaryTab",
                panelRect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-105f, -26f),
                new Vector2(190f, 44f));

            summaryTabImage =
                UiKit.RoundedImage(
                    summaryTabRect,
                    ActiveTabColor);

            var summaryTab =
                UiKit.MakeButton(summaryTabImage);

            UiKit.Fill(
                "Label",
                summaryTabRect,
                "Özet",
                23,
                CardStyle.Gold,
                TextAnchor.MiddleCenter);

            var historyTabRect = UiKit.Rect(
                "HistoryTab",
                panelRect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(105f, -26f),
                new Vector2(190f, 44f));

            historyTabImage =
                UiKit.RoundedImage(
                    historyTabRect,
                    InactiveTabColor);

            var historyTab =
                UiKit.MakeButton(historyTabImage);

            UiKit.Fill(
                "Label",
                historyTabRect,
                "El Geçmişi",
                23,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            summaryPage =
                BuildSummaryPage(panelRect);

            historyPage =
                BuildHistoryPage(panelRect);

            summaryTab.onClick.AddListener(
                () => ShowSummary());

            historyTab.onClick.AddListener(
                () => ShowHistory());

            toggle.onClick.AddListener(() =>
            {
                bool show = !panel.activeSelf;

                panel.SetActive(show);

                if (show)
                    ShowSummary();
            });

            ShowSummary();
            panel.SetActive(false);
        }

        GameObject BuildSummaryPage(
            RectTransform parent)
        {
            var page = UiKit.Rect(
                "SummaryPage",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                parent.sizeDelta);

            BuildHeader(page, 88f, 42f);

            for (int r = 0; r < SummaryRows; r++)
            {
                float y = 140f + r * 54f;

                Color rowColor =
                    r % 2 == 0
                        ? RowColorA
                        : RowColorB;

                for (int c = 0; c < 5; c++)
                {
                    summaryCells[r, c] =
                        GridCell(
                            page,
                            c,
                            y,
                            48f,
                            "",
                            rowColor,
                            c == 0
                                ? TextAnchor.MiddleLeft
                                : TextAnchor.MiddleCenter,
                            c == 0 ? 23 : 25);
                }

                summaryCells[r, 0].fontStyle =
                    FontStyle.Bold;
            }

            float totalY =
                140f + SummaryRows * 54f + 12f;

            for (int c = 0; c < 5; c++)
            {
                summaryCells[SummaryRows, c] =
                    GridCell(
                        page,
                        c,
                        totalY,
                        50f,
                        "",
                        TotalColor,
                        c == 0
                            ? TextAnchor.MiddleLeft
                            : TextAnchor.MiddleCenter,
                        26);

                summaryCells[SummaryRows, c].fontStyle =
                    FontStyle.Bold;
            }

            summaryCells[SummaryRows, 0].text =
                "TOPLAM";

            summaryCells[SummaryRows, 0].color =
                CardStyle.Gold;

            return page.gameObject;
        }

        GameObject BuildHistoryPage(
            RectTransform parent)
        {
            var page = UiKit.Rect(
                "HistoryPage",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                parent.sizeDelta);

            BuildHeader(page, 82f, 34f);

            for (int r = 0; r < HistoryRows; r++)
            {
                float y =
                    122f + r * 21f;

                Color rowColor =
                    r % 2 == 0
                        ? RowColorA
                        : RowColorB;

                for (int c = 0; c < 5; c++)
                {
                    historyCells[r, c] =
                        GridCell(
                            page,
                            c,
                            y,
                            19f,
                            "",
                            rowColor,
                            c == 0
                                ? TextAnchor.MiddleLeft
                                : TextAnchor.MiddleCenter,
                            c == 0 ? 16 : 17);
                }

                historyCells[r, 0]
                    .resizeTextForBestFit = true;

                historyCells[r, 0]
                    .resizeTextMinSize = 12;

                historyCells[r, 0]
                    .resizeTextMaxSize = 16;
            }

            float totalY =
                122f +
                HistoryRows * 21f +
                10f;

            for (int c = 0; c < 5; c++)
            {
                historyCells[HistoryRows, c] =
                    GridCell(
                        page,
                        c,
                        totalY,
                        30f,
                        "",
                        TotalColor,
                        c == 0
                            ? TextAnchor.MiddleLeft
                            : TextAnchor.MiddleCenter,
                        21);

                historyCells[HistoryRows, c]
                    .fontStyle = FontStyle.Bold;
            }

            historyCells[HistoryRows, 0].text =
                "Toplam";

            historyCells[HistoryRows, 0].color =
                CardStyle.Gold;

            return page.gameObject;
        }

        void BuildHeader(
            RectTransform parent,
            float y,
            float height)
        {
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
                var header =
                    GridCell(
                        parent,
                        c,
                        y,
                        height,
                        headers[c],
                        HeaderColor,
                        c == 0
                            ? TextAnchor.MiddleLeft
                            : TextAnchor.MiddleCenter,
                        22);

                header.fontStyle =
                    FontStyle.Bold;

                header.color =
                    c == 0
                        ? CardStyle.Gold
                        : CardStyle.Cream;

                if (c > 0)
                {
                    header.resizeTextForBestFit =
                        true;

                    header.resizeTextMinSize = 13;
                    header.resizeTextMaxSize = 22;
                }
            }
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
                new Vector2(
                    ColumnX[column],
                    -y),
                new Vector2(
                    ColumnWidth[column],
                    height));

            var image =
                rt.gameObject.AddComponent<Image>();

            image.color = backgroundColor;
            image.raycastTarget = false;

            float padding =
                column == 0 ? 12f : 4f;

            return UiKit.Label(
                "Text",
                rt,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(padding, 0f),
                new Vector2(
                    ColumnWidth[column] -
                    padding * 2f,
                    height),
                text,
                fontSize,
                CardStyle.Cream,
                alignment);
        }

        public void Refresh(Session session)
        {
            RefreshSummary(session);
            RefreshHistory(session);
        }

        void RefreshSummary(Session session)
        {
            var sheet = session.Sheet;

            var totals =
                new int[SummaryRows, 4];

            var played =
                new int[SummaryRows];

            for (int i = 0;
                 i < sheet.Count;
                 i++)
            {
                var row = sheet[i];

                int type =
                    (int)row.Contract.Type;

                if (type < 0 ||
                    type >= SummaryRows)
                    continue;

                played[type]++;

                for (int s = 0;
                     s < 4;
                     s++)
                {
                    totals[type, s] +=
                        row.Points[s];
                }
            }

            for (int r = 0;
                 r < SummaryRows;
                 r++)
            {
                var type =
                    (ContractType)r;

                string prefix =
                    type == ContractType.Trump
                        ? "▲  "
                        : "●  ";

                summaryCells[r, 0].text =
                    prefix +
                    GameText.ContractLabel(type);

                summaryCells[r, 0].color =
                    type == ContractType.Trump
                        ? TrumpColor
                        : PenaltyColor;

                for (int s = 0;
                     s < 4;
                     s++)
                {
                    if (played[r] == 0)
                    {
                        summaryCells[r, 1 + s].text =
                            "—";

                        summaryCells[r, 1 + s].color =
                            new Color(
                                CardStyle.Cream.r,
                                CardStyle.Cream.g,
                                CardStyle.Cream.b,
                                0.35f);

                        continue;
                    }

                    int points =
                        totals[r, s];

                    summaryCells[r, 1 + s].text =
                        points.ToString();

                    SetScoreColor(
                        summaryCells[r, 1 + s],
                        points);
                }
            }

            for (int s = 0;
                 s < 4;
                 s++)
            {
                int total =
                    session.Totals[s];

                summaryCells[
                    SummaryRows,
                    1 + s].text =
                    total.ToString();

                SetScoreColor(
                    summaryCells[
                        SummaryRows,
                        1 + s],
                    total);
            }
        }

        void RefreshHistory(Session session)
        {
            var sheet = session.Sheet;

            for (int r = 0;
                 r < HistoryRows;
                 r++)
            {
                if (r < sheet.Count)
                {
                    var row =
                        sheet[r];

                    historyCells[r, 0].text =
                        (r + 1) +
                        ". " +
                        GameText.ContractLabel(
                            row.Contract) +
                        " · " +
                        GameText.SeatLabel(
                            row.Caller);

                    historyCells[r, 0].color =
                        row.Contract.Type ==
                        ContractType.Trump
                            ? TrumpColor
                            : CardStyle.Cream;

                    for (int s = 0;
                         s < 4;
                         s++)
                    {
                        int points =
                            row.Points[s];

                        historyCells[
                            r,
                            1 + s].text =
                            points.ToString();

                        SetScoreColor(
                            historyCells[
                                r,
                                1 + s],
                            points);
                    }
                }
                else
                {
                    for (int c = 0;
                         c < 5;
                         c++)
                    {
                        historyCells[r, c].text =
                            "";

                        historyCells[r, c].color =
                            CardStyle.Cream;
                    }
                }
            }

            for (int s = 0;
                 s < 4;
                 s++)
            {
                int total =
                    session.Totals[s];

                historyCells[
                    HistoryRows,
                    1 + s].text =
                    total.ToString();

                SetScoreColor(
                    historyCells[
                        HistoryRows,
                        1 + s],
                    total);
            }
        }

        static void SetScoreColor(
            Text text,
            int points)
        {
            if (points < 0)
                text.color = NegativeColor;
            else if (points > 0)
                text.color = PositiveColor;
            else
                text.color = CardStyle.Cream;
        }

        void ShowSummary()
        {
            summaryPage.SetActive(true);
            historyPage.SetActive(false);

            summaryTabImage.color =
                ActiveTabColor;

            historyTabImage.color =
                InactiveTabColor;
        }

        void ShowHistory()
        {
            summaryPage.SetActive(false);
            historyPage.SetActive(true);

            summaryTabImage.color =
                InactiveTabColor;

            historyTabImage.color =
                ActiveTabColor;
        }
    }
}
