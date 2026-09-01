using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // West, North and East are represented by a small fan of face-down cards.
    public sealed class OpponentsView
    {
        static readonly Vector2 BackSize = new Vector2(72f, 104f);

        static readonly Color BackLine = new Color(0.39f, 0.25f, 0.28f, 1f);
        static readonly Color BackCenter = new Color(0.30f, 0.19f, 0.21f, 1f);

        public OpponentsView(Transform canvas)
        {
            Build(canvas, Seat.West,
                new Vector2(0f, 0.5f), new Vector2(135f, 40f));

            Build(canvas, Seat.North,
                new Vector2(0.5f, 1f), new Vector2(0f, -115f));

            Build(canvas, Seat.East,
                new Vector2(1f, 0.5f), new Vector2(-135f, 40f));
        }

        void Build(Transform canvas, Seat seat, Vector2 anchor, Vector2 position)
        {
            var panel = UiKit.Rect(
                seat + "Panel",
                canvas,
                anchor,
                new Vector2(0.5f, 0.5f),
                position,
                new Vector2(130f, 125f));

            var offsets = new[]
            {
                new Vector2(-20f, 3f),
                new Vector2(0f, 0f),
                new Vector2(20f, 3f)
            };

            var angles = new[] { -8f, 0f, 8f };

            for (int i = 0; i < 3; i++)
            {
                var backRect = UiKit.Rect(
                    "Back" + i,
                    panel,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    offsets[i],
                    BackSize);

                backRect.localRotation = Quaternion.Euler(0f, 0f, angles[i]);

                // Outer card back body.
                UiKit.RoundedImage(backRect, CardStyle.BackBlue);

                // Inner dark burgundy line.
                var lineRect = UiKit.Rect(
                    "Line",
                    backRect,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(BackSize.x - 8f, BackSize.y - 8f));

                UiKit.RoundedImage(lineRect, BackLine);

                // Center fill.
                var centerRect = UiKit.Rect(
                    "Center",
                    lineRect,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(BackSize.x - 16f, BackSize.y - 16f));

                UiKit.RoundedImage(centerRect, BackCenter);
            }
        }

        // Kept because GameBootstrap already calls Refresh.
        public void Refresh(DealEngine deal)
        {
        }
    }
}
