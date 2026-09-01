using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Remaining contract calls below each player name:
    // three red circles for penalty calls and two blue triangles for trump calls.
    public sealed class PlayerQuotaView
    {
        readonly Text[,] penalty = new Text[4, 3];
        readonly Text[,] trump = new Text[4, 2];

        static readonly Color PenaltyColor =
            new Color(0.82f, 0.18f, 0.18f, 1f);

        static readonly Color TrumpColor =
            new Color(0.25f, 0.48f, 0.88f, 1f);

        static readonly Color QuotaBoxColor =
            new Color(0.025f, 0.10f, 0.06f, 0.72f);

        public PlayerQuotaView(Transform canvas)
        {
            Build(
                canvas,
                Seat.South,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -212f));

            Build(
                canvas,
                Seat.West,
                new Vector2(0f, 0.5f),
                new Vector2(135f, -67f));

            Build(
                canvas,
                Seat.North,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -220f));

            Build(
                canvas,
                Seat.East,
                new Vector2(1f, 0.5f),
                new Vector2(-135f, -67f));
        }

        void Build(
            Transform canvas,
            Seat seat,
            Vector2 anchor,
            Vector2 position)
        {
            var row = UiKit.Rect(
                seat + "Quota",
                canvas,
                anchor,
                new Vector2(0.5f, 0.5f),
                position,
                new Vector2(140f, 30f));

            UiKit.RoundedImage(row, QuotaBoxColor);

            float[] x =
            {
                -46f, -25f, -4f,
                25f, 47f
            };

            for (int i = 0; i < 3; i++)
            {
                penalty[(int)seat, i] = UiKit.Label(
                    "Penalty" + i,
                    row,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(x[i], 0f),
                    new Vector2(20f, 22f),
                    "●",
                    20,
                    PenaltyColor,
                    TextAnchor.MiddleCenter);
            }

            for (int i = 0; i < 2; i++)
            {
                trump[(int)seat, i] = UiKit.Label(
                    "Trump" + i,
                    row,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(x[3 + i], 0f),
                    new Vector2(20f, 22f),
                    "▲",
                    18,
                    TrumpColor,
                    TextAnchor.MiddleCenter);
            }
        }

        public void Refresh(Session session)
        {
            for (int s = 0; s < 4; s++)
            {
                var seat = (Seat)s;

                int penaltiesLeft =
                    session.PenaltySlotsLeft(seat);

                int trumpsLeft =
                    session.TrumpCallsLeft(seat);

                for (int i = 0; i < 3; i++)
                    penalty[s, i].gameObject.SetActive(
                        i < penaltiesLeft);

                for (int i = 0; i < 2; i++)
                    trump[s, i].gameObject.SetActive(
                        i < trumpsLeft);
            }
        }
    }
}
