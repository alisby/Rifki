using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // One line of state in the top-left corner: deal number, contract, whose turn.
    public sealed class StatusLine
    {
        readonly Text text;

        public StatusLine(Transform canvas)
        {
            text = UiKit.Label("Status", canvas, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -18f), new Vector2(1100f, 36f), "", 28, CardStyle.Cream, TextAnchor.MiddleLeft);
        }

        public void Set(string value) => text.text = value;
    }
}
