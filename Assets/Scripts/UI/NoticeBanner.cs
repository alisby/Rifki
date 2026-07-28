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
            var rt = UiKit.Rect("Notice", canvas, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -78f), new Vector2(640f, 62f));
            UiKit.RoundedImage(rt, new Color(0.04f, 0.12f, 0.07f, 0.94f));
            text = UiKit.Fill("Text", rt, "", 30, CardStyle.Gold, TextAnchor.MiddleCenter);
            root = rt.gameObject;
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
