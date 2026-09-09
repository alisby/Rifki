using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // A short-lived strip under the status line for moments the table state
    // alone does not explain: hearts breaking, a deal ending early. It runs its
    // timer on the bootstrap's coroutine scheduler without blocking play.
    public sealed class NoticeBanner
    {
        readonly GameObject root;
        readonly Text text;

        Coroutine active;

        public NoticeBanner(Transform canvas)
        {
            var border = UiKit.Rect(
                "NoticeBorder",
                canvas,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 48f),
                new Vector2(652f, 74f));

            UiKit.RoundedImage(
                border,
                new Color(0.72f, 0.54f, 0.18f, 1f));

            var rt = UiKit.Rect(
                "Notice",
                border,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(640f, 62f));

            UiKit.RoundedImage(
                rt,
                new Color(0.015f, 0.115f, 0.055f, 0.96f));

            text = UiKit.Fill(
                "Text",
                rt,
                "",
                30,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            root = border.gameObject;
            root.SetActive(false);
        }

        public void Flash(MonoBehaviour host, string message, float seconds)
        {
            if (active != null)
                host.StopCoroutine(active);
            active = host.StartCoroutine(Run(message, seconds));
        }

        IEnumerator Run(string message, float seconds)
        {
            text.text = message;
            root.SetActive(true);
            yield return new WaitForSeconds(seconds);
            root.SetActive(false);
            active = null;
        }
    }
}
