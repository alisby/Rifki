using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public static class RifkiBranding
    {
        const string MainLogoPath =
            "Brand/rifki_logo_main";

        const string FeltLogoPath =
            "Brand/rifki_logo_felt";

        const float CornerSize = 160f;
        const float FeltSize = 560f;
        const float FeltAlpha = 0.10f;

        static Sprite mainLogo;
        static Sprite feltLogo;

        static Sprite LoadSprite(
            string resourcePath,
            ref Sprite cache)
        {
            if (cache != null)
                return cache;

            var texture =
                Resources.Load<Texture2D>(
                    resourcePath);

            if (texture == null)
            {
                Debug.LogError(
                    "Rıfkı logo bulunamadı: " +
                    resourcePath);

                return null;
            }

            cache = Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    texture.width,
                    texture.height),
                new Vector2(0.5f, 0.5f),
                100f);

            return cache;
        }

        static Sprite MainLogo()
        {
            return LoadSprite(
                MainLogoPath,
                ref mainLogo);
        }

        static Sprite FeltLogo()
        {
            return LoadSprite(
                FeltLogoPath,
                ref feltLogo);
        }

        public static void AddFeltWatermark(
            Transform canvas)
        {
            var rt = UiKit.Rect(
                "RifkiFeltWatermark",
                canvas,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 48f),
                new Vector2(
                    FeltSize,
                    FeltSize));

            var image =
                rt.gameObject.AddComponent<Image>();

            image.sprite = FeltLogo();
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    FeltAlpha);
        }

        public static GameObject AddCornerLogo(
            Transform parent,
            string name)
        {
            var rt = UiKit.Rect(
                name,
                parent,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(
                    CornerSize / 2f,
                    -CornerSize / 2f),
                new Vector2(
                    CornerSize,
                    CornerSize));

            var image =
                rt.gameObject.AddComponent<Image>();

            image.sprite = MainLogo();
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = Color.white;

            return rt.gameObject;
        }

        public static void ShowSplash(
            MonoBehaviour host,
            Transform canvas)
        {
            var background = UiKit.Stretched(
                "RifkiSplash",
                canvas,
                new Color(
                    0.008f,
                    0.008f,
                    0.008f,
                    1f));

            background.raycastTarget = true;

            var overlay =
                background.gameObject;

            var logoRect = UiKit.Rect(
                "Logo",
                overlay.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(760f, 760f));

            var logo =
                logoRect.gameObject
                    .AddComponent<Image>();

            logo.sprite = MainLogo();
            logo.preserveAspect = true;
            logo.raycastTarget = false;

            var group =
                overlay.AddComponent<CanvasGroup>();

            host.StartCoroutine(
                FadeSplash(
                    overlay,
                    group));
        }

        static IEnumerator FadeSplash(
            GameObject overlay,
            CanvasGroup group)
        {
            yield return
                new WaitForSecondsRealtime(1.35f);

            const float fadeTime = 0.35f;

            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    1f -
                    Mathf.Clamp01(
                        elapsed / fadeTime);

                yield return null;
            }

            Object.Destroy(overlay);
        }
    }
}
