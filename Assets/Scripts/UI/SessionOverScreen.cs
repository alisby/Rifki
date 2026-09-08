using System;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class SessionOverScreen
    {
        readonly GameObject overlay;
        readonly Text subtitleLine;

        readonly Image[] rowBorders = new Image[4];
        readonly Image[] rowBackgrounds = new Image[4];
        readonly RectTransform[] rowBackgroundRects = new RectTransform[4];
        readonly Text[] rowNames = new Text[4];
        readonly Text[] rowScores = new Text[4];
        readonly Text[] rowIcons = new Text[4];

        static readonly Color PanelColor =
            new Color(0.015f, 0.115f, 0.055f, 0.985f);

        static readonly Color PanelBorder =
            new Color(0.72f, 0.54f, 0.18f, 1f);

        static readonly Color RowColor =
            new Color(0.018f, 0.145f, 0.070f, 0.98f);

        static readonly Color RowBorder =
            new Color(0.22f, 0.38f, 0.25f, 1f);

        static readonly Color WinnerFill =
            new Color(0.25f, 0.005f, 0.005f, 0.98f);

        static readonly Color WinnerBorder =
            new Color(0.76f, 0.08f, 0.06f, 1f);

        static readonly Color WinnerText =
            new Color(1f, 0.86f, 0.18f, 1f);

        static readonly Color LoserFill =
            new Color(0.01f, 0.11f, 0.09f, 0.98f);

        static readonly Color LoserBorder =
            new Color(0.20f, 0.62f, 0.78f, 1f);

        static readonly Color LoserText =
            new Color(0.34f, 0.78f, 0.94f, 1f);

        public SessionOverScreen(Transform canvas, Action onRestart)
        {
            var dim = UiKit.Stretched(
                "SessionOver",
                canvas,
                new Color(0f, 0f, 0f, 0.68f));

            dim.raycastTarget = true;
            overlay = dim.gameObject;

            var panelBorder = UiKit.Rect(
                "PanelBorder",
                overlay.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(1240f, 706f));

            UiKit.RoundedImage(panelBorder, PanelBorder);

            var panel = UiKit.Rect(
                "Panel",
                panelBorder,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(1228f, 694f));

            UiKit.RoundedImage(panel, PanelColor);

            UiKit.Label(
                "Title",
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -46f),
                new Vector2(1000f, 58f),
                "Oyun bitti",
                46,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            var leftLine = UiKit.Rect(
                "TitleLineLeft",
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-400f, -76f),
                new Vector2(230f, 3f));

            UiKit.RoundedImage(leftLine, PanelBorder);

            var rightLine = UiKit.Rect(
                "TitleLineRight",
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(400f, -76f),
                new Vector2(230f, 3f));

            UiKit.RoundedImage(rightLine, PanelBorder);

            subtitleLine = UiKit.Label(
                "Subtitle",
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -110f),
                new Vector2(1050f, 38f),
                "Final puan durumu",
                24,
                new Color(0.80f, 0.82f, 0.76f, 1f),
                TextAnchor.MiddleCenter);

            for (int i = 0; i < 4; i++)
                BuildStandingRow(panel, i, -176f - i * 84f);

            var buttonBorder = UiKit.Rect(
                "RestartBorder",
                panel,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 35f),
                new Vector2(278f, 68f));

            UiKit.RoundedImage(buttonBorder, PanelBorder);

            var buttonRect = UiKit.Rect(
                "Restart",
                buttonBorder,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(268f, 58f));

            var buttonImage = UiKit.RoundedImage(
                buttonRect,
                CardStyle.Cream);

            var button = UiKit.MakeButton(buttonImage);

            UiKit.Fill(
                "Label",
                buttonRect,
                "Tekrar oyna",
                28,
                CardStyle.BlackInk,
                TextAnchor.MiddleCenter);

            button.onClick.AddListener(() => onRestart());

            overlay.SetActive(false);
        }

        void BuildStandingRow(Transform panel, int index, float y)
        {
            var borderRect = UiKit.Rect(
                "StandingBorder" + index,
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, y),
                new Vector2(930f, 72f));

            rowBorders[index] =
                UiKit.RoundedImage(borderRect, RowBorder);

            var backgroundRect = UiKit.Rect(
                "StandingBackground" + index,
                borderRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(924f, 66f));

            rowBackgroundRects[index] = backgroundRect;

            rowBackgrounds[index] =
                UiKit.RoundedImage(backgroundRect, RowColor);

            rowNames[index] = UiKit.Label(
                "Name",
                backgroundRect,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(32f, 0f),
                new Vector2(510f, 52f),
                "",
                31,
                CardStyle.Cream,
                TextAnchor.MiddleLeft);

            rowScores[index] = UiKit.Label(
                "Score",
                backgroundRect,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-118f, 0f),
                new Vector2(260f, 52f),
                "",
                31,
                CardStyle.Cream,
                TextAnchor.MiddleRight);

            rowIcons[index] = UiKit.Label(
                "Icon",
                backgroundRect,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-35f, 0f),
                new Vector2(64f, 52f),
                "",
                31,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            rowNames[index].fontStyle = FontStyle.Bold;
            rowScores[index].fontStyle = FontStyle.Bold;
        }

        public void Show(Session session)
        {
            if (session.KingEnded)
            {
                ShowKingResult(session);
                return;
            }

            subtitleLine.text = "Final puan durumu";

            var totals = session.Totals;
            var order = new[] { 0, 1, 2, 3 };

            Array.Sort(order, (a, b) =>
            {
                int byTotal = totals[b].CompareTo(totals[a]);
                return byTotal != 0 ? byTotal : a.CompareTo(b);
            });

            int bestTotal = totals[order[0]];

            for (int i = 0; i < 4; i++)
            {
                int seat = order[i];
                int total = totals[seat];

                bool isWinner = total == bestTotal;
                bool isLoser = total < 0 && !isWinner;

                rowNames[i].text = GameText.SeatLabel((Seat)seat);
                rowScores[i].text = total.ToString();

                if (isWinner)
                    SetWinnerStyle(i);
                else if (total < 0)
                    SetLoserStyle(i);
                else if (total > 0)
                    SetNormalStyle(i);
                else
                    SetNeutralStyle(i);
            }

            overlay.SetActive(true);
        }

        void SetWinnerStyle(int index)
        {
            rowBorders[index].color =
                new Color(0.20f, 0.62f, 0.78f, 1f);

            rowBackgrounds[index].color =
                new Color(0.01f, 0.11f, 0.09f, 0.98f);

            rowBackgroundRects[index].sizeDelta =
                new Vector2(922f, 64f);

            rowNames[index].color =
                new Color(0.34f, 0.78f, 0.94f, 1f);

            rowScores[index].color =
                new Color(0.34f, 0.78f, 0.94f, 1f);

            rowIcons[index].text = "🏆";
            rowIcons[index].color = CardStyle.Cream;
        }

        void SetLoserStyle(int index)
        {
            rowBorders[index].color =
                new Color(0.76f, 0.08f, 0.06f, 1f);

            rowBackgrounds[index].color =
                new Color(0.25f, 0.005f, 0.005f, 0.98f);

            rowBackgroundRects[index].sizeDelta =
                new Vector2(914f, 56f);

            rowNames[index].color =
                new Color(1f, 0.86f, 0.18f, 1f);

            rowScores[index].color =
                new Color(1f, 0.86f, 0.18f, 1f);

            rowIcons[index].text = "😭";
            rowIcons[index].color = CardStyle.Cream;
        }

        void SetNormalStyle(int index)
        {
            rowBorders[index].color =
                new Color(0.20f, 0.48f, 0.18f, 1f);

            rowBackgrounds[index].color =
                new Color(0.03f, 0.18f, 0.08f, 0.98f);

            rowBackgroundRects[index].sizeDelta =
                new Vector2(924f, 66f);

            rowNames[index].color =
                new Color(0.72f, 0.92f, 0.56f, 1f);

            rowScores[index].color =
                new Color(0.72f, 0.92f, 0.56f, 1f);

            rowIcons[index].text = "";
        }

        void SetNeutralStyle(int index)
        {
            rowBorders[index].color = RowBorder;
            rowBackgrounds[index].color = RowColor;
            rowBackgroundRects[index].sizeDelta =
                new Vector2(924f, 66f);

            rowNames[index].color = CardStyle.Cream;
            rowScores[index].color = CardStyle.Cream;
            rowIcons[index].text = "";
        }

        void SetKingOutStyle(int index)
        {
            rowBorders[index].color = new Color(0.20f, 0.62f, 0.78f, 1f);
            rowBackgrounds[index].color = new Color(0.01f, 0.11f, 0.09f, 0.98f);
            rowBackgroundRects[index].sizeDelta = new Vector2(922f, 64f);
            rowNames[index].color = new Color(0.34f, 0.78f, 0.94f, 1f);
            rowScores[index].color = new Color(0.34f, 0.78f, 0.94f, 1f);
            rowIcons[index].text = "🏆";
            rowIcons[index].color = CardStyle.Cream;
        }

        void SetKingBattiStyle(int index)
        {
            rowBorders[index].color = new Color(0.76f, 0.08f, 0.06f, 1f);
            rowBackgrounds[index].color = new Color(0.25f, 0.005f, 0.005f, 0.98f);
            rowBackgroundRects[index].sizeDelta = new Vector2(914f, 56f);
            rowNames[index].color = new Color(1f, 0.86f, 0.18f, 1f);
            rowScores[index].color = new Color(1f, 0.86f, 0.18f, 1f);
            rowIcons[index].text = "😭";
            rowIcons[index].color = CardStyle.Cream;
        }

        void ShowKingResult(Session session)
        {
            Seat declarer = session.KingDeclarer.Value;

            subtitleLine.text = session.KingSucceeded
                ? GameText.SeatLabel(declarer)
                    + " King yaptı — tek başına çıktı"
                : GameText.SeatLabel(declarer)
                    + " King yapamadı — tek başına battı";

            for (int s = 0; s < 4; s++)
            {
                bool isDeclarer = s == (int)declarer;
                bool outPlayer =
                    session.KingSucceeded
                        ? isDeclarer
                        : !isDeclarer;

                rowNames[s].text = GameText.SeatLabel((Seat)s);
                rowScores[s].text = outPlayer ? "ÇIKTI" : "BATTI";

                if (outPlayer)
                    SetKingOutStyle(s);
                else
                    SetKingBattiStyle(s);
            }

            overlay.SetActive(true);
        }
    }
}
