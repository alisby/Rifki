using System.Collections.Generic;
using King.Core;
using UnityEngine;

namespace King.UI
{
    // The middle of the table: one played-card slot per seat.
    public sealed class TrickView
    {
        static readonly Vector2 CardSize = new Vector2(104f, 152f);

        readonly CardFace[] faces = new CardFace[4];

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
        }

        public void Clear()
        {
            for (int s = 0; s < 4; s++)
                faces[s].SetVisible(false);
        }
    }
}
