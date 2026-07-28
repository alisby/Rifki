using System.Collections.Generic;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // The middle of the table: one card slot per seat around the center, with a
    // seat name beside each. The current player's name is picked out in gold,
    // and a finished trick highlights its winner while it lingers.
    public sealed class TrickView
    {
        static readonly Vector2 CardSize = new Vector2(104f, 152f);

        readonly CardFace[] faces = new CardFace[4];
        readonly Text[] labels = new Text[4];

        public TrickView(Transform canvas)
        {
            var center = UiKit.Rect("TrickArea", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f), new Vector2(620f, 480f));

            // Card offsets from the center, matching where each player sits.
            var cardAt = new[]
            {
                new Vector2(0f, -120f),   // South
                new Vector2(-200f, 0f),   // West
                new Vector2(0f, 120f),    // North
                new Vector2(200f, 0f),    // East
            };
            var labelAt = new[]
            {
                new Vector2(0f, -220f),
                new Vector2(-200f, -100f),
                new Vector2(0f, 220f),
                new Vector2(200f, -100f),
            };

            for (int s = 0; s < 4; s++)
            {
                faces[s] = new CardFace(center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), cardAt[s], CardSize);
                faces[s].SetVisible(false);
                labels[s] = UiKit.Label(((Seat)s) + "Label", center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    labelAt[s], new Vector2(160f, 30f), GameText.SeatLabel((Seat)s), 26, CardStyle.Cream, TextAnchor.MiddleCenter);
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

        // Gold name for whoever is on play; pass null once the deal is over.
        public void MarkTurn(Seat? seat)
        {
            for (int s = 0; s < 4; s++)
                labels[s].color = seat.HasValue && (int)seat.Value == s ? CardStyle.Gold : CardStyle.Cream;
        }
    }
}
