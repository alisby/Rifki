using System;
using System.Collections.Generic;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // End of the twentieth deal: final standings best-first, the winner in
    // gold, and a button that reloads the scene for a fresh session.
    public sealed class SessionOverScreen
    {
        readonly GameObject overlay;
        readonly Text winnerLine;
        readonly Text[] standings = new Text[4];

        public SessionOverScreen(Transform canvas, Action onRestart)
        {
            var dim = UiKit.Stretched("SessionOver", canvas, new Color(0f, 0f, 0f, 0.65f));
            dim.raycastTarget = true;
            overlay = dim.gameObject;

            var panel = UiKit.Rect("Panel", overlay.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1200f, 580f));
            UiKit.RoundedImage(panel, new Color(0.07f, 0.19f, 0.11f, 0.98f));

            UiKit.Label("Title", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f),
                new Vector2(1100f, 44f), "Oyun bitti", 38, CardStyle.Cream, TextAnchor.MiddleCenter);
            winnerLine = UiKit.Label("Winner", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -86f),
                new Vector2(1100f, 44f), "", 30, CardStyle.Gold, TextAnchor.MiddleCenter);

            for (int i = 0; i < 4; i++)
                standings[i] = UiKit.Label("Standing", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -160f - i * 64f), new Vector2(900f, 40f), "", 30, CardStyle.Cream, TextAnchor.MiddleCenter);

            var buttonRect = UiKit.Rect("Restart", panel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 36f), new Vector2(230f, 60f));
            var buttonImage = UiKit.RoundedImage(buttonRect, CardStyle.Cream);
            var button = UiKit.MakeButton(buttonImage);
            UiKit.Fill("Label", buttonRect, "Tekrar oyna", 28, CardStyle.BlackInk, TextAnchor.MiddleCenter);
            button.onClick.AddListener(() => onRestart());

            overlay.SetActive(false);
        }

        public void Show(Session session)
        {
            if (session.RifkiEnded)
            {
                ShowRifkiResult(session);
                return;
            }

            var totals = session.Totals;

            // Best score first; ties break toward the earlier seat since Array.Sort
            // alone makes no ordering promise for equal keys.
            var order = new[] { 0, 1, 2, 3 };
            Array.Sort(order, (a, b) =>
            {
                int byTotal = totals[b].CompareTo(totals[a]);
                return byTotal != 0 ? byTotal : a.CompareTo(b);
            });

            winnerLine.text = GameText.SeatLabel((Seat)order[0]) + " kazandı";
            for (int i = 0; i < 4; i++)
            {
                int seat = order[i];
                standings[i].text = GameText.SeatLabel((Seat)seat) + "   " + totals[seat];
                standings[i].color = i == 0 ? CardStyle.Gold : CardStyle.Cream;
            }
            overlay.SetActive(true);
        }

        void ShowRifkiResult(Session session)
        {
            Seat declarer = session.RifkiDeclarer.Value;

            winnerLine.text = session.RifkiSucceeded
                ? GameText.SeatLabel(declarer) + " Rıfkı yaptı — tek başına çıktı"
                : GameText.SeatLabel(declarer) + " Rıfkı yapamadı — tek başına battı";

            for (int s = 0; s < 4; s++)
            {
                bool isDeclarer = s == (int)declarer;
                bool outPlayer = session.RifkiSucceeded ? isDeclarer : !isDeclarer;

                standings[s].text =
                    GameText.SeatLabel((Seat)s)
                    + (outPlayer ? "   ÇIKTI" : "   BATTI");

                standings[s].color =
                    outPlayer ? CardStyle.Gold : CardStyle.Cream;
            }

            overlay.SetActive(true);
        }
    }
}
