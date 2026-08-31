using King.Core;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Card backs for West, North and East with a count of what they still hold.
    public sealed class OpponentsView
    {
        static readonly Vector2 BackSize = new Vector2(84f, 122f);

        readonly Text[] counts = new Text[4];

        public OpponentsView(Transform canvas)
        {
            Build(canvas, Seat.West, new Vector2(0f, 0.5f), new Vector2(120f, 40f));
            Build(canvas, Seat.North, new Vector2(0.5f, 1f), new Vector2(0f, -120f));
            Build(canvas, Seat.East, new Vector2(1f, 0.5f), new Vector2(-120f, 40f));
        }

        void Build(Transform canvas, Seat seat, Vector2 anchor, Vector2 position)
        {
            var panel = UiKit.Rect(seat + "Panel", canvas, anchor, new Vector2(0.5f, 0.5f), position,
                new Vector2(BackSize.x, BackSize.y + 40f));
            var backRect = UiKit.Rect("Back", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, BackSize);
            UiKit.RoundedImage(backRect, CardStyle.BackBlue);
            counts[(int)seat] = UiKit.Fill("Count", backRect, "13", 40, CardStyle.Cream, TextAnchor.MiddleCenter);
        }

        public void Refresh(DealEngine deal)
        {
            for (int s = 1; s < 4; s++)
                counts[s].text = deal.HandOf((Seat)s).Count.ToString();
        }
    }
}
