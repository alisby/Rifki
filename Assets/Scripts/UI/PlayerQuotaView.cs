using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Three penalty quotas and two trump quotas below each player.
    // Used symbols remain visible but faded.
    // Current-deal captured units are shown below their symbol group.
    public sealed class PlayerQuotaView
    {
        readonly Text[,] penalty = new Text[4, 3];
        readonly Text[,] trump = new Text[4, 2];

        readonly Text[] penaltyTaken = new Text[4];
        readonly Text[] trumpTaken = new Text[4];

        static readonly Color PenaltyColor =
            new Color(0.82f, 0.18f, 0.18f, 1f);

        static readonly Color TrumpColor =
            new Color(0.25f, 0.48f, 0.88f, 1f);

        static readonly Color QuotaBoxColor =
            new Color(0.025f, 0.10f, 0.06f, 0.78f);

        const float UsedAlpha = 0.20f;

        public PlayerQuotaView(Transform canvas)
        {
            // South is moved down, closer to the human cards.
            Build(
                canvas,
                Seat.South,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -258f));

            Build(
                canvas,
                Seat.West,
                new Vector2(0f, 0.5f),
                new Vector2(135f, -4f));

            Build(
                canvas,
                Seat.North,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -122f));

            Build(
                canvas,
                Seat.East,
                new Vector2(1f, 0.5f),
                new Vector2(-135f, -4f));
        }

        static Color Alpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
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
                new Vector2(480f, 82f));

            UiKit.RoundedImage(row, QuotaBoxColor);

            float[] x =
            {
                -76f, -40f, -4f,
                48f, 84f
            };

            for (int i = 0; i < 3; i++)
            {
                penalty[(int)seat, i] = UiKit.Label(
                    "Penalty" + i,
                    row,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(x[i], 13f),
                    new Vector2(34f, 36f),
                    "●",
                    34,
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
                    new Vector2(x[3 + i], 13f),
                    new Vector2(34f, 36f),
                    "▲",
                    32,
                    TrumpColor,
                    TextAnchor.MiddleCenter);
            }

            penaltyTaken[(int)seat] = UiKit.Label(
                "PenaltyTaken",
                row,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-190f, 0f),
                new Vector2(120f, 72f),
                "",
                60,
                PenaltyColor,
                TextAnchor.MiddleCenter);

            penaltyTaken[(int)seat].fontStyle =
                FontStyle.Bold;

            trumpTaken[(int)seat] = UiKit.Label(
                "TrumpTaken",
                row,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(190f, 0f),
                new Vector2(120f, 72f),
                "",
                60,
                TrumpColor,
                TextAnchor.MiddleCenter);

            trumpTaken[(int)seat].fontStyle =
                FontStyle.Bold;
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
                {
                    penalty[s, i].color =
                        Alpha(
                            PenaltyColor,
                            i < penaltiesLeft ? 1f : UsedAlpha);
                }

                for (int i = 0; i < 2; i++)
                {
                    trump[s, i].color =
                        Alpha(
                            TrumpColor,
                            i < trumpsLeft ? 1f : UsedAlpha);
                }
            }

            ClearDealCounts();
        }

        public void RefreshDeal(DealEngine deal)
        {
            ClearDealCounts();

            if (deal == null)
                return;

            for (int s = 0; s < 4; s++)
            {
                int taken = deal.UnitsTaken((Seat)s);

                if (deal.Contract.Type == ContractType.Trump)
                {
                    trumpTaken[s].text = taken.ToString();
                }
                else
                {
                    penaltyTaken[s].text = taken.ToString();
                }
            }
        }

        public void ClearDealCounts()
        {
            for (int s = 0; s < 4; s++)
            {
                penaltyTaken[s].text = "";
                trumpTaken[s].text = "";
            }
        }
    }
}
