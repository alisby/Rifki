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
        readonly Image[,] penalty = new Image[4, 3];
        readonly Image[,] trump = new Image[4, 2];

        readonly Text[] dealTaken = new Text[4];
        readonly Text[] totalScore = new Text[4];

        static readonly Color PenaltyColor =
            new Color(0.93f, 0.30f, 0.26f, 1f);

        static readonly Color TrumpColor =
            new Color(0.43f, 0.66f, 0.98f, 1f);

        const float UsedAlpha = 0.20f;

        public PlayerQuotaView(Transform canvas)
        {
            // South is moved down, closer to the human cards.
            Build(
                canvas,
                Seat.South,
                new Vector2(0.5f, 0.5f),
                new Vector2(-89f, -257f));

            Build(
                canvas,
                Seat.West,
                new Vector2(0f, 0.5f),
                new Vector2(161f, -4f));

            Build(
                canvas,
                Seat.North,
                new Vector2(0.5f, 1f),
                new Vector2(-89f, -142f));

            Build(
                canvas,
                Seat.East,
                new Vector2(1f, 0.5f),
                new Vector2(-339f, -4f));
        }

        static Sprite LoadRuntimeSprite(string path)
        {
            var texture = Resources.Load<Texture2D>(path);
            if (texture == null)
                return null;

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
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

            float[] x =
            {
                -48f, -18f, 12f,
                50f, 80f
            };

            var penaltySprite =
                LoadRuntimeSprite("QuotaIcons/penalty_token");
            var trumpSprite =
                LoadRuntimeSprite("QuotaIcons/trump_token");

            for (int i = 0; i < 3; i++)
            {
                var rt = UiKit.Rect(
                    "Penalty" + i,
                    row,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(x[i], 0f),
                    new Vector2(36f, 36f));

                penalty[(int)seat, i] =
                    rt.gameObject.AddComponent<Image>();

                penalty[(int)seat, i].sprite =
                    penaltySprite;
                penalty[(int)seat, i].color =
                    Color.white;
                penalty[(int)seat, i].preserveAspect = true;
                penalty[(int)seat, i].raycastTarget = false;
            }

            for (int i = 0; i < 2; i++)
            {
                var rt = UiKit.Rect(
                    "Trump" + i,
                    row,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(x[3 + i], 1f),
                    new Vector2(36f, 36f));

                trump[(int)seat, i] =
                    rt.gameObject.AddComponent<Image>();

                trump[(int)seat, i].sprite =
                    trumpSprite;
                trump[(int)seat, i].color =
                    Color.white;
                trump[(int)seat, i].preserveAspect = true;
                trump[(int)seat, i].raycastTarget = false;
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
                new Color(1f, 1f, 1f, 0.10f);

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
                        ? TrumpColor
                        : score < 0
                            ? new Color(0.95f, 0.12f, 0.08f, 1f)
                            : CardStyle.Cream;

                for (int i = 0; i < 3; i++)
                {
                    penalty[s, i].color =
                        new Color(
                            1f,
                            1f,
                            1f,
                            i < penaltiesLeft ? 1f : UsedAlpha);
                }

                for (int i = 0; i < 2; i++)
                {
                    trump[s, i].color =
                        new Color(
                            1f,
                            1f,
                            1f,
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
                        : new Color(0.95f, 0.12f, 0.08f, 1f);
            }
        }

        public void ClearDealCounts()
        {
            for (int s = 0; s < 4; s++)
                dealTaken[s].text = "";
        }
    }
}
