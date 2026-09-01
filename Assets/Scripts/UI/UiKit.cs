using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    // Small factory for the uGUI objects the table is built from. Everything is
    // created in code, so this is the only place that knows about fonts, sprites
    // and RectTransform plumbing.
    public static class UiKit
    {
        static Font font;
        static Sprite rounded;
        static bool triedRounded;

        public static Font Font
        {
            get
            {
                if (font == null)
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        // The stock UISprite is nine-sliced with rounded corners, which is exactly
        // the card look we want. If it ever comes back null the images just render
        // as plain rectangles, so nobody null-checks the result.
        public static Sprite Rounded
        {
            get
            {
                if (!triedRounded)
                {
                    triedRounded = true;
                    rounded = CreateRoundedSprite();
                }

                return rounded;
            }
        }

        static Sprite CreateRoundedSprite()
        {
            const int size = 64;
            const int radius = 12;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "RuntimeRoundedRect";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    float cx = Mathf.Clamp(px, radius, size - radius);
                    float cy = Mathf.Clamp(py, radius, size - radius);

                    float dx = px - cx;
                    float dy = py - cy;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = Mathf.Clamp01(radius + 0.5f - distance);

                    pixels[y * size + x] =
                        new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            var border = new Vector4(radius, radius, radius, radius);

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                border
            );

            sprite.name = "RuntimeRoundedRect";
            return sprite;
        }

        // A RectTransform anchored to a single point of its parent.
        public static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        // An image stretched over the whole parent.
        public static Image Stretched(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Image RoundedImage(RectTransform rt, Color color)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = Rounded;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Text Label(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, string text, int fontSize, Color color, TextAnchor alignment)
        {
            var rt = Rect(name, parent, anchor, pivot, position, size);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = alignment;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.text = text;
            return t;
        }

        // A text label stretched over its parent, for centering inside buttons and cells.
        public static Text Fill(string name, Transform parent, string text, int fontSize, Color color, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.font = Font;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = alignment;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.text = text;
            return t;
        }

        public static Button MakeButton(Image target)
        {
            target.raycastTarget = true;
            var button = target.gameObject.AddComponent<Button>();
            button.targetGraphic = target;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.97f, 0.82f);
            colors.pressedColor = new Color(0.83f, 0.83f, 0.83f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f);
            button.colors = colors;
            return button;
        }
    }
}
