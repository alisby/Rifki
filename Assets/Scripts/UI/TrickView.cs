using System.Collections.Generic;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // The middle of the table: one played-card slot per seat.
    public sealed class TrickView
    {
        static readonly Vector2 CardSize = new Vector2(104f, 152f);

        readonly CardFace[] faces = new CardFace[4];
        readonly Text[] labels = new Text[4];
        readonly Text[] callerStars = new Text[4];

        public TrickView(Transform canvas)
        {
            var center = UiKit.Rect(
                "TrickArea",
                canvas,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 68f),
                new Vector2(620f, 480f));

            var cardAt = new[]
            {
                new Vector2(0f, -90f),    // South
                new Vector2(-200f, 30f),  // West
                new Vector2(0f, 150f),    // North
                new Vector2(200f, 30f),   // East
            };

            for (int s = 0; s < 4; s++)
            {
                faces[s] = new CardFace(
                    center,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    cardAt[s],
                    CardSize);

                faces[s].SetVisible(false);
            }

            // Ali remains above the human hand.
            labels[(int)Seat.South] = UiKit.Label(
                "SouthLabel",
                center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -210f),
                new Vector2(190f, 42f),
                GameText.SeatLabel(Seat.South),
                36,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            // Hande: directly below the West card fan.
            labels[(int)Seat.West] = UiKit.Label(
                "WestLabel",
                canvas,
                new Vector2(0f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(250f, 76f),
                new Vector2(190f, 42f),
                GameText.SeatLabel(Seat.West),
                36,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            // Nevra: directly below the North card fan.
            labels[(int)Seat.North] = UiKit.Label(
                "NorthLabel",
                canvas,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -42f),
                new Vector2(190f, 42f),
                GameText.SeatLabel(Seat.North),
                36,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            // Melis: directly below the East card fan.
            labels[(int)Seat.East] = UiKit.Label(
                "EastLabel",
                canvas,
                new Vector2(1f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-250f, 76f),
                new Vector2(190f, 42f),
                GameText.SeatLabel(Seat.East),
                36,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            for (int s = 0; s < 4; s++)
            {
                labels[s].fontStyle = FontStyle.Bold;
                labels[s].resizeTextForBestFit = true;
                labels[s].resizeTextMinSize = 28;
                labels[s].resizeTextMaxSize = 38;

                var nameShadow =
                    labels[s].gameObject.AddComponent<Shadow>();

                nameShadow.effectColor =
                    new Color(0f, 0f, 0f, 0.70f);

                nameShadow.effectDistance =
                    new Vector2(1.5f, -1.5f);

                nameShadow.useGraphicAlpha = true;

                callerStars[s] = UiKit.Label(
                    "CallerStar" + s,
                    labels[s].transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(150f, 1f),
                    new Vector2(54f, 54f),
                    "★",
                    50,
                    new Color32(255, 201, 43, 255),
                    TextAnchor.MiddleCenter);

                callerStars[s].fontStyle = FontStyle.Bold;
                callerStars[s].raycastTarget = false;

                var shadow = callerStars[s].gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color32(50, 27, 0, 210);
                shadow.effectDistance = new Vector2(3f, -4f);
                shadow.useGraphicAlpha = true;

                var outline = callerStars[s].gameObject.AddComponent<Outline>();
                outline.effectColor = new Color32(164, 96, 0, 255);
                outline.effectDistance = new Vector2(1.5f, -1.5f);
                outline.useGraphicAlpha = true;

                callerStars[s].gameObject.SetActive(false);
            }
        }

        public void ShowCurrent(IReadOnlyList<(Seat Seat, Card Card)> plays)
        {
            for (int s = 0; s < 4; s++)
                faces[s].SetVisible(false);

            foreach (var play in plays)
            {
                faces[(int)play.Seat].Bind(play.Card);
                faces[(int)play.Seat].SetVisible(true);
            }
        }

        public void ShowCompleted(CompletedTrick trick)
        {
            ShowCurrent(trick.Plays);
            labels[(int)trick.Winner].color = CardStyle.Gold;
        }

        public void Clear()
        {
            for (int s = 0; s < 4; s++)
            {
                faces[s].SetVisible(false);
                labels[s].color = CardStyle.Cream;
            }
        }

        public void MarkCaller(Seat caller)
        {
            for (int s = 0; s < 4; s++)
            {
                labels[s].text = GameText.SeatLabel((Seat)s);
                callerStars[s].gameObject.SetActive(s == (int)caller);
            }
        }

        public void MarkTurn(Seat? seat)
        {
            for (int s = 0; s < 4; s++)
                labels[s].color =
                    seat.HasValue && (int)seat.Value == s
                        ? CardStyle.Gold
                        : CardStyle.Cream;
        }
    }
}
