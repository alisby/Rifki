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

        public TrickView(Transform canvas)
        {
            var center = UiKit.Rect(
                "TrickArea",
                canvas,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f),
                new Vector2(620f, 480f));

            var cardAt = new[]
            {
                new Vector2(0f, -120f),   // South
                new Vector2(-200f, 0f),   // West
                new Vector2(0f, 120f),    // North
                new Vector2(200f, 0f),    // East
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
                new Vector2(0f, -228f),
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
                labels[s].fontStyle = FontStyle.Bold;
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
