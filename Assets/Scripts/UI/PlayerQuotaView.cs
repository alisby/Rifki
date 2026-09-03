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

        readonly Text[] dealTaken = new Text[4];

        static readonly Color PenaltyColor =
            new Color(0.82f, 0.18f, 0.18f, 1f);

        static readonly Color TrumpColor =
            new Color(0.25f, 0.48f, 0.88f, 1f);

        static readonly Color QuotaBoxColor =
            new Color(0.025f, 0.10f, 0.06f, 0.34f);

        const float UsedAlpha = 0.20f;

        public PlayerQuotaView(Transform canvas)
        {
            // South is moved down, closer to the human cards.
            Build(
                canvas,
                Seat.South,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -230f));

            Build(
                canvas,
                Seat.West,
                new Vector2(0f, 0.5f),
                new Vector2(250f, -4f));

            Build(
                canvas,
                Seat.North,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -122f));

            Build(
                canvas,
                Seat.East,
                new Vector2(1f, 0.5f),
                new Vector2(-250f, -4f));
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
                    new Vector2(x[i], 0f),
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
                    new Vector2(x[3 + i], 0f),
                    new Vector2(34f, 36f),
                    "▲",
                    32,
                    TrumpColor,
                    TextAnchor.MiddleCenter);
            }

            dealTaken[(int)seat] = UiKit.Label(
                "DealTaken",
                row,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-150f, 0f),
                new Vector2(80f, 72f),
                "",
                56,
                PenaltyColor,
                TextAnchor.MiddleCenter);

            dealTaken[(int)seat].fontStyle =
                FontStyle.Bold;

            var countShadow =
                dealTaken[(int)seat].gameObject.AddComponent<Shadow>();

            countShadow.effectColor =
                new Color(0f, 0f, 0f, 0.65f);

            countShadow.effectDistance =
                new Vector2(2f, -2f);

            countShadow.useGraphicAlpha = true;
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

                dealTaken[s].text = taken.ToString();
                dealTaken[s].color =
                    deal.Contract.Type == ContractType.Trump
                        ? TrumpColor
                        : PenaltyColor;
            }
        }

        public void ClearDealCounts()
        {
            for (int s = 0; s < 4; s++)
            {
                dealTaken[s].text = "";
            }
        }
    }
}
