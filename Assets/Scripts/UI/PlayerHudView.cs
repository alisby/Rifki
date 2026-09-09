using System.Collections.Generic;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Compact player HUD: name + current-deal count + caller star.
    // Clicking a name opens the detailed quota/score/captured-card panel.
    public sealed class PlayerHudView
    {
        sealed class MiniCard
        {
            public Image Frame;
            public Image Body;
            public Text Label;
            public Color Ink;
        }

        static readonly Color PanelColor =
            new Color(0.015f, 0.115f, 0.055f, 0.985f);

        static readonly Color GoldBorder =
            new Color(0.72f, 0.54f, 0.18f, 1f);

        static readonly Color Gold =
            new Color32(255, 201, 43, 255);

        static readonly Color TrumpColor =
            new Color(0.43f, 0.66f, 0.98f, 1f);

        static readonly Color PenaltyCountColor =
            new Color(0.95f, 0.12f, 0.08f, 1f);

        static readonly Color MutedText =
            new Color(0.80f, 0.82f, 0.76f, 1f);

        static readonly Color CardBody =
            new Color(0.88f, 0.82f, 0.69f, 1f);

        static readonly Color CardFrame =
            new Color(0.34f, 0.28f, 0.16f, 1f);

        static readonly Color BlackInk =
            new Color(0.10f, 0.08f, 0.06f, 1f);

        static readonly Color RedInk =
            new Color(0.72f, 0.08f, 0.08f, 1f);

        static readonly string[] HeartRanks =
        {
            "A", "K", "Q", "J", "10", "9", "8",
            "7", "6", "5", "4", "3", "2"
        };

        const float UsedAlpha = 0.20f;

        readonly RectTransform[] roots =
            new RectTransform[4];

        readonly RectTransform[] details =
            new RectTransform[4];

        readonly Text[] dealCounts =
            new Text[4];

        readonly Text[] callerStars =
            new Text[4];

        readonly Text[] totalScores =
            new Text[4];

        readonly Image[,] penalty =
            new Image[4, 3];

        readonly Image[,] trump =
            new Image[4, 2];

        readonly MiniCard[,] queens =
            new MiniCard[4, 4];

        readonly MiniCard[,] men =
            new MiniCard[4, 8];

        readonly MiniCard[,] hearts =
            new MiniCard[4, 13];

        int openSeat = -1;

        public PlayerHudView(Transform canvas)
        {
            BuildSeat(
                canvas,
                Seat.South,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -222f));

            BuildSeat(
                canvas,
                Seat.West,
                new Vector2(0f, 0.5f),
                new Vector2(298f, 56f));

            BuildSeat(
                canvas,
                Seat.North,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -106f));

            BuildSeat(
                canvas,
                Seat.East,
                new Vector2(1f, 0.5f),
                new Vector2(-274f, 56f));

            ClearDeal();
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

        void BuildSeat(
            Transform canvas,
            Seat seat,
            Vector2 anchor,
            Vector2 position)
        {
            int s = (int)seat;

            roots[s] = UiKit.Rect(
                seat + "PlayerHud",
                canvas,
                anchor,
                new Vector2(0.5f, 0.5f),
                position,
                new Vector2(300f, 100f));

            BuildHeader(seat);
            details[s] = BuildDetails(seat);
            details[s].gameObject.SetActive(false);
        }

        void BuildHeader(Seat seat)
        {
            int s = (int)seat;
            var root = roots[s];

            var nameHit = UiKit.Rect(
                "PlayerNameButton",
                root,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 20f),
                new Vector2(270f, 50f));

            var hitImage = UiKit.RoundedImage(
                nameHit,
                new Color(0f, 0f, 0f, 0f));

            var button = UiKit.MakeButton(hitImage);
            button.onClick.AddListener(
                () => ToggleDetails(seat));

            var name = UiKit.Fill(
                "PlayerName",
                nameHit,
                GameText.SeatLabel(seat),
                40,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            name.fontStyle = FontStyle.Bold;
            name.resizeTextForBestFit = true;
            name.resizeTextMinSize = 27;
            name.resizeTextMaxSize = 40;
            AddShadow(name);

            dealCounts[s] = UiKit.Label(
                "DealCount",
                root,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-20f, -23f),
                new Vector2(70f, 48f),
                "0",
                44,
                PenaltyCountColor,
                TextAnchor.MiddleCenter);

            dealCounts[s].fontStyle = FontStyle.Bold;
            AddShadow(dealCounts[s]);

            callerStars[s] = UiKit.Label(
                "CallerStar",
                root,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(42f, -23f),
                new Vector2(50f, 48f),
                "★",
                42,
                Gold,
                TextAnchor.MiddleCenter);

            callerStars[s].fontStyle = FontStyle.Bold;
            AddShadow(callerStars[s]);
            callerStars[s].gameObject.SetActive(false);
        }

        RectTransform BuildDetails(Seat seat)
        {
            int s = (int)seat;

            Vector2 pivot;
            Vector2 position;

            switch (seat)
            {
                case Seat.South:
                    pivot = new Vector2(0.5f, 0f);
                    position = new Vector2(0f, 55f);
                    break;

                case Seat.North:
                    pivot = new Vector2(0.5f, 1f);
                    position = new Vector2(0f, -55f);
                    break;

                case Seat.West:
                    pivot = new Vector2(0f, 0.5f);
                    position = new Vector2(155f, 0f);
                    break;

                default: // East
                    pivot = new Vector2(1f, 0.5f);
                    position = new Vector2(-155f, 0f);
                    break;
            }

            var frame = UiKit.Rect(
                "PlayerDetails",
                roots[s],
                new Vector2(0.5f, 0.5f),
                pivot,
                position,
                new Vector2(540f, 500f));

            UiKit.RoundedImage(frame, GoldBorder);

            var body = UiKit.Rect(
                "Body",
                frame,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(528f, 488f));

            UiKit.RoundedImage(body, PanelColor);

            BuildSummary(body, seat);

            AddDivider(body, 178f);

            BuildSectionTitle(body, "Kızlar", 145f);
            BuildCardLine(
                body,
                queens,
                s,
                0,
                "Queens",
                110f,
                new[] { "Q♣", "Q♦", "Q♠", "Q♥" });

            AddDivider(body, 68f);

            BuildSectionTitle(body, "Erkekler", 52f);
            BuildCardLine(
                body,
                men,
                s,
                0,
                "Jacks",
                16f,
                new[] { "J♣", "J♦", "J♠", "J♥" });

            BuildCardLine(
                body,
                men,
                s,
                4,
                "Kings",
                -44f,
                new[] { "K♣", "K♦", "K♠", "K♥" });

            AddDivider(body, -82f);

            BuildSectionTitle(body, "Kupalar", -98f);
            BuildCardLine(
                body,
                hearts,
                s,
                0,
                "HeartsTop",
                -136f,
                new[] { "A♥", "K♥", "Q♥", "J♥", "10♥", "9♥", "8♥" });

            BuildCardLine(
                body,
                hearts,
                s,
                7,
                "HeartsBottom",
                -196f,
                new[] { "7♥", "6♥", "5♥", "4♥", "3♥", "2♥" });

            return frame;
        }

        void BuildSummary(
            Transform parent,
            Seat seat)
        {
            int s = (int)seat;

            for (int i = 0; i < 3; i++)
            {
                penalty[s, i] = BuildQuotaToken(
                    parent,
                    "Penalty" + i,
                    new Vector2(-190f + i * 40f, 210f),
                    "QuotaIcons/penalty_token");
            }

            for (int i = 0; i < 2; i++)
            {
                trump[s, i] = BuildQuotaToken(
                    parent,
                    "Trump" + i,
                    new Vector2(-20f + i * 45f, 210f),
                    "QuotaIcons/trump_token");
            }

            totalScores[s] = UiKit.Label(
                "Total",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(155f, 210f),
                new Vector2(150f, 44f),
                "0",
                32,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            totalScores[s].fontStyle = FontStyle.Bold;
            AddShadow(totalScores[s]);
        }

        Image BuildQuotaToken(
            Transform parent,
            string name,
            Vector2 position,
            string spritePath)
        {
            var rt = UiKit.Rect(
                name,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                new Vector2(36f, 36f));

            var image =
                rt.gameObject.AddComponent<Image>();

            image.sprite = LoadRuntimeSprite(spritePath);
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = Color.white;

            return image;
        }

        void AddDivider(
            Transform parent,
            float y)
        {
            var line = UiKit.Rect(
                "SectionDivider",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, y),
                new Vector2(472f, 2f));

            UiKit.RoundedImage(
                line,
                new Color(
                    GoldBorder.r,
                    GoldBorder.g,
                    GoldBorder.b,
                    0.60f));
        }

        void BuildSectionTitle(
            Transform parent,
            string title,
            float y)
        {
            var label = UiKit.Label(
                title + "Title",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-206f, y),
                new Vector2(120f, 28f),
                title,
                20,
                MutedText,
                TextAnchor.MiddleLeft);

            label.fontStyle = FontStyle.Bold;
        }

        void BuildCardLine(
            Transform parent,
            MiniCard[,] store,
            int seat,
            int startIndex,
            string name,
            float y,
            string[] texts)
        {
            const float cardWidth = 52f;
            const float cardHeight = 58f;
            const float spacing = 7f;

            float width =
                texts.Length * cardWidth
                + (texts.Length - 1) * spacing;

            float start =
                -width * 0.5f + cardWidth * 0.5f;

            for (int i = 0; i < texts.Length; i++)
            {
                bool red =
                    texts[i].Contains("♦")
                    || texts[i].Contains("♥");

                store[seat, startIndex + i] =
                    BuildMiniCard(
                        parent,
                        name + i,
                        new Vector2(
                            start + i * (cardWidth + spacing),
                            y),
                        new Vector2(cardWidth, cardHeight),
                        texts[i],
                        red ? RedInk : BlackInk,
                        texts[i].Contains("10") ? 17 : 21);
            }
        }

        MiniCard BuildMiniCard(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            string text,
            Color ink,
            int fontSize)
        {
            var frame = UiKit.Rect(
                name,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size);

            var frameImage =
                UiKit.RoundedImage(frame, CardFrame);

            var inner = UiKit.Rect(
                "Inner",
                frame,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(size.x - 4f, size.y - 4f));

            var bodyImage =
                UiKit.RoundedImage(inner, CardBody);

            var label = UiKit.Fill(
                "Label",
                inner,
                text,
                fontSize,
                ink,
                TextAnchor.MiddleCenter);

            label.fontStyle = FontStyle.Bold;

            var card = new MiniCard
            {
                Frame = frameImage,
                Body = bodyImage,
                Label = label,
                Ink = ink
            };

            SetCardActive(card, false);
            return card;
        }

        static void SetCardActive(
            MiniCard card,
            bool active)
        {
            Color frame = CardFrame;
            frame.a = active ? 1f : 0.26f;
            card.Frame.color = frame;

            Color body = CardBody;
            body.a = active ? 1f : 0.20f;
            card.Body.color = body;

            Color ink = card.Ink;
            ink.a = active ? 1f : 0.36f;
            card.Label.color = ink;
        }

        static void AddShadow(Text text)
        {
            var shadow =
                text.gameObject.AddComponent<Shadow>();

            shadow.effectColor =
                new Color(0f, 0f, 0f, 0.82f);

            shadow.effectDistance =
                new Vector2(2f, -2f);

            shadow.useGraphicAlpha = true;
        }

        public void Refresh(Session session)
        {
            if (session == null)
                return;

            for (int s = 0; s < 4; s++)
            {
                int penaltiesLeft =
                    session.PenaltySlotsLeft((Seat)s);

                int trumpsLeft =
                    session.TrumpCallsLeft((Seat)s);

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

                int score = session.Totals[s];

                totalScores[s].text =
                    score > 0
                        ? "+" + score
                        : score.ToString();

                totalScores[s].color =
                    score > 0
                        ? TrumpColor
                        : score < 0
                            ? PenaltyCountColor
                            : CardStyle.Cream;
            }
        }

        public void RefreshDeal(DealEngine deal)
        {
            ClearDeal();

            if (deal == null)
                return;

            bool isTrump =
                deal.Contract.Type == ContractType.Trump;

            for (int s = 0; s < 4; s++)
            {
                dealCounts[s].text =
                    deal.UnitsTaken((Seat)s).ToString();

                dealCounts[s].color =
                    isTrump
                        ? TrumpColor
                        : PenaltyCountColor;
            }

            foreach (var trick in deal.History)
            {
                foreach (var play in trick.Plays)
                {
                    int winner =
                        (int)trick.Winner;

                    switch (deal.Contract.Type)
                    {
                        case ContractType.NoQueens:
                            if (play.Card.Rank == Rank.Queen)
                                MarkQueen(winner, play.Card);
                            break;

                        case ContractType.NoMen:
                            if (play.Card.Rank == Rank.Jack
                                || play.Card.Rank == Rank.King)
                            {
                                MarkMan(winner, play.Card);
                            }
                            break;

                        case ContractType.NoHearts:
                            if (play.Card.Suit == Suit.Hearts)
                                MarkHeart(winner, play.Card);
                            break;

                        case ContractType.KingOfHearts:
                            if (play.Card.Suit == Suit.Hearts
                                && play.Card.Rank == Rank.King)
                            {
                                MarkHeart(winner, play.Card);
                            }
                            break;
                    }
                }
            }
        }

        public void ClearDeal()
        {
            for (int s = 0; s < 4; s++)
            {
                if (dealCounts[s] != null)
                {
                    dealCounts[s].text = "0";
                    dealCounts[s].color =
                        PenaltyCountColor;
                }

                for (int i = 0; i < 4; i++)
                    if (queens[s, i] != null)
                        SetCardActive(queens[s, i], false);

                for (int i = 0; i < 8; i++)
                    if (men[s, i] != null)
                        SetCardActive(men[s, i], false);

                for (int i = 0; i < 13; i++)
                    if (hearts[s, i] != null)
                        SetCardActive(hearts[s, i], false);
            }
        }

        public void MarkCaller(Seat caller)
        {
            for (int s = 0; s < 4; s++)
                callerStars[s].gameObject.SetActive(
                    s == (int)caller);
        }

        void MarkQueen(
            int seat,
            Card card)
        {
            int suit = SuitIndex(card.Suit);
            if (suit >= 0)
                SetCardActive(queens[seat, suit], true);
        }

        void MarkMan(
            int seat,
            Card card)
        {
            int suit = SuitIndex(card.Suit);
            if (suit < 0)
                return;

            int offset =
                card.Rank == Rank.King ? 4 : 0;

            SetCardActive(
                men[seat, offset + suit],
                true);
        }

        void MarkHeart(
            int seat,
            Card card)
        {
            string rank =
                CardStyle.RankGlyph(card.Rank);

            for (int i = 0; i < HeartRanks.Length; i++)
            {
                if (HeartRanks[i] != rank)
                    continue;

                SetCardActive(hearts[seat, i], true);
                return;
            }
        }

        static int SuitIndex(Suit suit)
        {
            switch (CardStyle.SuitGlyph(suit))
            {
                case "♣":
                    return 0;
                case "♦":
                    return 1;
                case "♠":
                    return 2;
                case "♥":
                    return 3;
                default:
                    return -1;
            }
        }

        void ToggleDetails(Seat seat)
        {
            int s = (int)seat;

            if (openSeat == s)
            {
                details[s].gameObject.SetActive(false);
                openSeat = -1;
                return;
            }

            if (openSeat >= 0)
                details[openSeat].gameObject.SetActive(false);

            details[s].gameObject.SetActive(true);
            details[s].SetAsLastSibling();
            openSeat = s;
        }
    }
}
