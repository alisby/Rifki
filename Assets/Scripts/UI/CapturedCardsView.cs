using System.Collections.Generic;
using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class CapturedCardsView
    {
        const int MaxCards = 13;

        static readonly Vector2 HeartNorthCardSize =
            new Vector2(40f, 52f);

        static readonly Vector2 HeartWideCardSize =
            new Vector2(36f, 46f);

        static readonly Vector2 PenaltyCardSize =
            new Vector2(52f, 58f);

        enum DisplayMode
        {
            None,
            Hearts,
            Queens,
            Men
        }

        sealed class MiniCard
        {
            public RectTransform Root;
            public RectTransform Inner;
            public Text Label;
        }

        readonly MiniCard[,] cards =
            new MiniCard[4, MaxCards];

        readonly RectTransform[] roots =
            new RectTransform[4];

        public CapturedCardsView(Transform canvas)
        {
            BuildSeat(
                canvas,
                Seat.North,
                new Vector2(0.5f, 1f),
                new Vector2(-375f, -142f));

            BuildSeat(
                canvas,
                Seat.South,
                new Vector2(0.5f, 0.5f),
                new Vector2(365f, -257f));

            BuildSeat(
                canvas,
                Seat.West,
                new Vector2(0f, 0.5f),
                new Vector2(161f, -160f));

            BuildSeat(
                canvas,
                Seat.East,
                new Vector2(1f, 0.5f),
                new Vector2(-339f, 175f));

            Clear();
        }

        void BuildSeat(
            Transform canvas,
            Seat seat,
            Vector2 anchor,
            Vector2 position)
        {
            var root = UiKit.Rect(
                seat + "CapturedCards",
                canvas,
                anchor,
                new Vector2(0.5f, 0.5f),
                position,
                new Vector2(700f, 220f));

            roots[(int)seat] = root;

            for (int i = 0; i < MaxCards; i++)
            {
                var frame = UiKit.Rect(
                    "CapturedCard" + i,
                    root,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    HeartNorthCardSize);

                var frameImage =
                    UiKit.RoundedImage(
                        frame,
                        new Color(
                            0.28f,
                            0.24f,
                            0.16f,
                            1f));

                frameImage.raycastTarget = false;

                var inner = UiKit.Rect(
                    "Inner",
                    frame,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(
                        HeartNorthCardSize.x - 4f,
                        HeartNorthCardSize.y - 4f));

                var innerImage =
                    UiKit.RoundedImage(
                        inner,
                        new Color(
                            0.86f,
                            0.78f,
                            0.62f,
                            0.98f));

                innerImage.raycastTarget = false;

                var label = UiKit.Fill(
                    "Label",
                    inner,
                    "",
                    19,
                    CardStyle.BlackInk,
                    TextAnchor.MiddleCenter);

                label.fontStyle = FontStyle.Bold;
                label.raycastTarget = false;

                cards[(int)seat, i] =
                    new MiniCard
                    {
                        Root = frame,
                        Inner = inner,
                        Label = label
                    };
            }
        }

        public void Refresh(DealEngine deal)
        {
            Clear();

            if (deal == null)
                return;

            DisplayMode mode =
                ModeFor(deal.Contract.Type);

            if (mode == DisplayMode.None)
                return;

            ApplyLayout(mode);

            var captured =
                new List<Card>[4];

            for (int s = 0; s < 4; s++)
                captured[s] = new List<Card>();

            foreach (var trick in deal.History)
            {
                foreach (var play in trick.Plays)
                {
                    if (!RelevantCard(
                        mode,
                        play.Card))
                        continue;

                    captured[(int)trick.Winner]
                        .Add(play.Card);
                }
            }

            for (int s = 0; s < 4; s++)
            {
                int count =
                    Mathf.Min(
                        captured[s].Count,
                        MaxCards);

                for (int i = 0; i < count; i++)
                {
                    ShowCard(
                        (Seat)s,
                        i,
                        captured[s][i],
                        mode);
                }
            }
        }

        static DisplayMode ModeFor(
            ContractType type)
        {
            switch (type)
            {
                case ContractType.NoHearts:
                    return DisplayMode.Hearts;

                case ContractType.NoQueens:
                    return DisplayMode.Queens;

                case ContractType.NoMen:
                    return DisplayMode.Men;

                default:
                    return DisplayMode.None;
            }
        }

        static bool RelevantCard(
            DisplayMode mode,
            Card card)
        {
            switch (mode)
            {
                case DisplayMode.Hearts:
                    return card.Suit == Suit.Hearts;

                case DisplayMode.Queens:
                    return card.Rank == Rank.Queen;

                case DisplayMode.Men:
                    return card.Rank == Rank.Jack
                        || card.Rank == Rank.King;

                default:
                    return false;
            }
        }

        void ApplyLayout(
            DisplayMode mode)
        {
            // All modes use the same seat placement as Kupa Almaz.
            roots[(int)Seat.North].anchoredPosition =
                new Vector2(-410f, -105f);

            roots[(int)Seat.South].anchoredPosition =
                new Vector2(500f, -210f);

            roots[(int)Seat.West].anchoredPosition =
                new Vector2(305f, -75f);

            roots[(int)Seat.East].anchoredPosition =
                new Vector2(-355f, 175f);
        }

        void ShowCard(
            Seat seat,
            int index,
            Card card,
            DisplayMode mode)
        {
            MiniCard item =
                cards[(int)seat, index];

            Vector2 size;
            Vector2 position;
            int fontSize;

            bool rightToLeft =
                seat == Seat.North
                || seat == Seat.East;

            if (mode == DisplayMode.Hearts)
            {
                if (seat == Seat.North)
                {
                    size = HeartNorthCardSize;
                    fontSize = 19;

                    // North:
                    // right -> left, top -> bottom.
                    if (index < 7)
                    {
                        position = RowPosition(
                            6 - index,
                            7,
                            HeartNorthCardSize.x,
                            3f,
                            29f);
                    }
                    else
                    {
                        position = RowPosition(
                            5 - (index - 7),
                            6,
                            HeartNorthCardSize.x,
                            3f,
                            -29f);
                    }
                }
                else
                {
                    size = HeartWideCardSize;
                    fontSize = 16;

                    int column =
                        seat == Seat.East
                            ? 12 - index
                            : index;

                    position = RowPosition(
                        column,
                        13,
                        HeartWideCardSize.x,
                        2f,
                        0f);
                }

                item.Label.text =
                    CardStyle.RankGlyph(card.Rank)
                    + CardStyle.SuitGlyph(card.Suit);
            }
            else if (mode == DisplayMode.Queens)
            {
                size = PenaltyCardSize;
                fontSize = 30;

                int column =
                    rightToLeft
                        ? 3 - index
                        : index;

                position = RowPosition(
                    column,
                    4,
                    PenaltyCardSize.x,
                    7f,
                    0f);

                item.Label.text =
                    CardStyle.SuitGlyph(card.Suit);
            }
            else
            {
                size = PenaltyCardSize;
                fontSize = 23;

                int sequenceRow =
                    index / 4;

                int column =
                    index % 4;

                if (rightToLeft)
                    column = 3 - column;

                float y;

                if (seat == Seat.East)
                {
                    // East:
                    // right -> left, bottom -> top.
                    y =
                        sequenceRow == 0
                            ? -32f
                            : 32f;
                }
                else
                {
                    // North, West, South:
                    // top -> bottom.
                    y =
                        sequenceRow == 0
                            ? 32f
                            : -32f;
                }

                position = RowPosition(
                    column,
                    4,
                    PenaltyCardSize.x,
                    7f,
                    y);

                item.Label.text =
                    CardStyle.RankGlyph(card.Rank)
                    + CardStyle.SuitGlyph(card.Suit);
            }

            item.Root.sizeDelta = size;

            item.Inner.sizeDelta =
                new Vector2(
                    size.x - 4f,
                    size.y - 4f);

            item.Root.anchoredPosition =
                position + SeatOffset(mode, seat);

            item.Label.fontSize = fontSize;

            item.Label.color =
                CardStyle.Ink(card.Suit);

            item.Root.gameObject.SetActive(true);
        }

        static Vector2 RowPosition(
            int column,
            int count,
            float width,
            float spacing,
            float y)
        {
            float rowWidth =
                count * width
                + (count - 1) * spacing;

            float x =
                -rowWidth * 0.5f
                + width * 0.5f
                + column * (width + spacing);

            return new Vector2(x, y);
        }

        static Vector2 SeatOffset(
            DisplayMode mode,
            Seat seat)
        {
            switch (mode)
            {
                case DisplayMode.Queens:
                    switch (seat)
                    {
                        case Seat.North:
                            return new Vector2(
                                PenaltyCardSize.x,
                                0f);

                        case Seat.West:
                            return new Vector2(
                                -1.5f * PenaltyCardSize.x,
                                0f);

                        case Seat.South:
                            return new Vector2(
                                -3f * PenaltyCardSize.x,
                                0f);

                        default: // East
                            return new Vector2(
                                2f * PenaltyCardSize.x,
                                -0.5f * PenaltyCardSize.y);
                    }

                case DisplayMode.Men:
                    switch (seat)
                    {
                        case Seat.North:
                            return new Vector2(
                                PenaltyCardSize.x,
                                0f);

                        case Seat.West:
                            return new Vector2(
                                -1.5f * PenaltyCardSize.x,
                                -PenaltyCardSize.y);

                        case Seat.South:
                            return new Vector2(
                                -3f * PenaltyCardSize.x,
                                0f);

                        default: // East
                            return new Vector2(
                                2f * PenaltyCardSize.x,
                                0f);
                    }

                case DisplayMode.Hearts:
                    switch (seat)
                    {
                        case Seat.North:
                            return new Vector2(
                                HeartNorthCardSize.x,
                                0f);

                        case Seat.West:
                            return new Vector2(
                                -HeartWideCardSize.x,
                                0f);

                        case Seat.South:
                            return Vector2.zero;

                        default: // East
                            return new Vector2(
                                2f * HeartWideCardSize.x,
                                0f);
                    }

                default:
                    return Vector2.zero;
            }
        }

        public void Clear()
        {
            for (int s = 0; s < 4; s++)
            {
                for (int i = 0; i < MaxCards; i++)
                {
                    cards[s, i].Root.gameObject.SetActive(false);
                    cards[s, i].Label.text = "";
                }
            }
        }
    }
}
