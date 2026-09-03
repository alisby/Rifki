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
        readonly Text[] totalScore = new Text[4];

        static readonly Color PenaltyColor =
            new Color(0.93f, 0.30f, 0.26f, 1f);

        static readonly Color TrumpColor =
            new Color(0.43f, 0.66f, 0.98f, 1f);

        static readonly Color QuotaBoxColor =
            new Color(0.020f, 0.085f, 0.050f, 0.62f);

        const float UsedAlpha = 0.20f;

        public PlayerQuotaView(Transform canvas)
        {
            // South is moved down, closer to the human cards.
            Build(
                canvas,
                Seat.South,
                new Vector2(0.5f, 0.5f),
                new Vector2(-89f, -222f));

            Build(
                canvas,
                Seat.West,
                new Vector2(0f, 0.5f),
                new Vector2(151f, -4f));

            Build(
                canvas,
                Seat.North,
                new Vector2(0.5f, 1f),
                new Vector2(-89f, -122f));

            Build(
                canvas,
                Seat.East,
                new Vector2(1f, 0.5f),
                new Vector2(-339f, -4f));
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
                new Vector2(240f, 60f));

            // Altlık kaldırıldı; 3D işaretler kullanılacak.

            float[] x =
            {
                -46f, -20f, 6f,
                44f, 70f
            };

            for (int i = 0; i < 3; i++)
            {
                penalty[(int)seat, i] = UiKit.Label(
                    "Penalty" + i,
                    row,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(x[i], 0f),
                    new Vector2(28f, 30f),
                    "●",
                    28,
                    PenaltyColor,
                    TextAnchor.MiddleCenter);

                penalty[(int)seat, i].fontStyle =
                    FontStyle.Bold;

                var penaltyShadow =
                    penalty[(int)seat, i].gameObject.AddComponent<Shadow>();

                penaltyShadow.effectColor =
                    new Color(0.16f, 0.04f, 0.04f, 0.62f);

                penaltyShadow.effectDistance =
                    new Vector2(1.3f, -1.3f);

                penaltyShadow.useGraphicAlpha = true;

                var penaltyOutline =
                    penalty[(int)seat, i].gameObject.AddComponent<Outline>();

                penaltyOutline.effectColor =
                    new Color(0.38f, 0.10f, 0.10f, 0.96f);

                penaltyOutline.effectDistance =
                    new Vector2(0.9f, -0.9f);

                penaltyOutline.useGraphicAlpha = true;

                var penaltyHighlight =
                    penalty[(int)seat, i].gameObject.AddComponent<Shadow>();

                penaltyHighlight.effectColor =
                    new Color(1f, 0.82f, 0.78f, 0.52f);

                penaltyHighlight.effectDistance =
                    new Vector2(-0.8f, 0.8f);

                penaltyHighlight.useGraphicAlpha = true;
            }

            for (int i = 0; i < 2; i++)
            {
                trump[(int)seat, i] = UiKit.Label(
                    "Trump" + i,
                    row,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(x[3 + i], 1f),
                    new Vector2(28f, 30f),
                    "▲",
                    26,
                    TrumpColor,
                    TextAnchor.MiddleCenter);

                trump[(int)seat, i].fontStyle =
                    FontStyle.Bold;

                var trumpShadow =
                    trump[(int)seat, i].gameObject.AddComponent<Shadow>();

                trumpShadow.effectColor =
                    new Color(0.03f, 0.08f, 0.18f, 0.62f);

                trumpShadow.effectDistance =
                    new Vector2(1.3f, -1.3f);

                trumpShadow.useGraphicAlpha = true;

                var trumpOutline =
                    trump[(int)seat, i].gameObject.AddComponent<Outline>();

                trumpOutline.effectColor =
                    new Color(0.08f, 0.19f, 0.42f, 0.96f);

                trumpOutline.effectDistance =
                    new Vector2(0.9f, -0.9f);

                trumpOutline.useGraphicAlpha = true;

                var trumpHighlight =
                    trump[(int)seat, i].gameObject.AddComponent<Shadow>();

                trumpHighlight.effectColor =
                    new Color(0.82f, 0.91f, 1f, 0.52f);

                trumpHighlight.effectDistance =
                    new Vector2(-0.8f, 0.8f);

                trumpHighlight.useGraphicAlpha = true;
            }

            dealTaken[(int)seat] = UiKit.Label(
                "DealTaken",
                row,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-88f, 0f),
                new Vector2(48f, 54f),
                "",
                48,
                PenaltyColor,
                TextAnchor.MiddleCenter);

            dealTaken[(int)seat].fontStyle =
                FontStyle.Bold;

            var countShadow =
                dealTaken[(int)seat].gameObject.AddComponent<Shadow>();

            countShadow.effectColor =
                new Color(0f, 0f, 0f, 0.62f);

            countShadow.effectDistance =
                new Vector2(1.9f, -1.9f);

            countShadow.useGraphicAlpha = true;

            var countOutline =
                dealTaken[(int)seat].gameObject.AddComponent<Outline>();

            countOutline.effectColor =
                new Color(0.18f, 0.14f, 0.08f, 0.92f);

            countOutline.effectDistance =
                new Vector2(1f, -1f);

            countOutline.useGraphicAlpha = true;

            var countHighlight =
                dealTaken[(int)seat].gameObject.AddComponent<Shadow>();

            countHighlight.effectColor =
                new Color(1f, 1f, 1f, 0.38f);

            countHighlight.effectDistance =
                new Vector2(-0.9f, 0.9f);

            countHighlight.useGraphicAlpha = true;

            var scoreFrame = UiKit.Rect(
                "TotalScoreFrame",
                row,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(198f, 0f),
                new Vector2(184f, 54f));

            UiKit.RoundedImage(
                scoreFrame,
                new Color(0.12f, 0.30f, 0.19f, 0.98f));

            var scoreBody = UiKit.Rect(
                "TotalScoreBody",
                scoreFrame,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(178f, 48f));

            UiKit.RoundedImage(
                scoreBody,
                new Color(0.050f, 0.155f, 0.098f, 0.98f));

            totalScore[(int)seat] = UiKit.Label(
                "TotalScore",
                scoreBody,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(172f, 46f),
                "0",
                44,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            totalScore[(int)seat].fontStyle = FontStyle.Bold;
            totalScore[(int)seat].resizeTextForBestFit = true;
            totalScore[(int)seat].resizeTextMinSize = 28;
            totalScore[(int)seat].resizeTextMaxSize = 44;
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

                int score = session.Totals[s];
                totalScore[s].text =
                    score > 0 ? "+" + score : score.ToString();

                totalScore[s].color =
                    score > 0
                        ? CardStyle.Gold
                        : score < 0
                            ? PenaltyColor
                            : CardStyle.Cream;

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
