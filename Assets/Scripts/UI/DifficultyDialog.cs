using System;
using King.AI;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class DifficultyDialog
    {
        readonly GameObject overlay;
        readonly Image[] difficultyImages = new Image[3];
        readonly GameObject cancelObject;
        Action<BotDifficulty> onStart;
        BotDifficulty selected = BotDifficulty.Normal;
        float previousTimeScale = 1f;

        static readonly Color NormalColor =
            new Color(0.055f, 0.18f, 0.105f, 1f);

        static readonly Color SelectedColor =
            new Color(0.62f, 0.52f, 0.25f, 1f);

        static readonly Color StartColor =
            new Color(0.10f, 0.36f, 0.19f, 1f);

        static readonly Color CancelColor =
            new Color(0.38f, 0.12f, 0.10f, 1f);

        public DifficultyDialog(Transform canvas)
        {
            var root = new GameObject(
                "DifficultyOverlay",
                typeof(RectTransform),
                typeof(Image));

            var rt = (RectTransform)root.transform;
            rt.SetParent(canvas, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var dim = root.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.68f);
            dim.raycastTarget = true;
            overlay = root;

            var panel = UiKit.Rect(
                "DifficultyPanel",
                rt,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(760f, 390f));

            UiKit.RoundedImage(
                panel,
                new Color(0.035f, 0.12f, 0.07f, 0.99f));

            var title = UiKit.Label(
                "Title",
                panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 125f),
                new Vector2(620f, 52f),
                "Zorluk Seçimi",
                36,
                CardStyle.Gold,
                TextAnchor.MiddleCenter);

            title.fontStyle = FontStyle.Bold;

            UiKit.Label(
                "Message",
                panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f),
                new Vector2(650f, 42f),
                "Bilgisayar oyuncularının seviyesini seçin",
                23,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            string[] labels = { "Kolay", "Normal", "Zor" };

            for (int i = 0; i < 3; i++)
            {
                int index = i;

                var option = UiKit.Rect(
                    "Difficulty" + i,
                    panel,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(-220f + i * 220f, 5f),
                    new Vector2(190f, 64f));

                var image =
                    UiKit.RoundedImage(option, NormalColor);

                difficultyImages[i] = image;

                var button = UiKit.MakeButton(image);

                var label = UiKit.Fill(
                    "Label",
                    option,
                    labels[i],
                    27,
                    CardStyle.Cream,
                    TextAnchor.MiddleCenter);

                label.fontStyle = FontStyle.Bold;

                button.onClick.AddListener(
                    () => Select((BotDifficulty)index));
            }

            var startRect = UiKit.Rect(
                "Start",
                panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-115f, -105f),
                new Vector2(200f, 60f));

            var startImage =
                UiKit.RoundedImage(startRect, StartColor);

            var startButton = UiKit.MakeButton(startImage);

            UiKit.Fill(
                "Label",
                startRect,
                "Başlat",
                26,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            startButton.onClick.AddListener(StartSelected);

            var cancelRect = UiKit.Rect(
                "Cancel",
                panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(115f, -105f),
                new Vector2(200f, 60f));

            cancelObject = cancelRect.gameObject;

            var cancelImage =
                UiKit.RoundedImage(cancelRect, CancelColor);

            var cancelButton = UiKit.MakeButton(cancelImage);

            UiKit.Fill(
                "Label",
                cancelRect,
                "Vazgeç",
                26,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            cancelButton.onClick.AddListener(Hide);

            overlay.SetActive(false);
            Select(BotDifficulty.Normal);
        }

        public void Show(
            BotDifficulty current,
            bool allowCancel,
            Action<BotDifficulty> callback)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            onStart = callback;
            Select(current);
            cancelObject.SetActive(allowCancel);

            var start =
                cancelObject.transform.parent.Find("Start")
                    as RectTransform;

            if (start != null)
                start.anchoredPosition =
                    allowCancel
                        ? new Vector2(-115f, -105f)
                        : new Vector2(0f, -105f);

            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();
        }

        void Select(BotDifficulty value)
        {
            selected = value;

            for (int i = 0; i < difficultyImages.Length; i++)
                difficultyImages[i].color =
                    i == (int)value
                        ? SelectedColor
                        : NormalColor;
        }

        void StartSelected()
        {
            overlay.SetActive(false);
            Time.timeScale = previousTimeScale;
            var callback = onStart;
            onStart = null;
            callback?.Invoke(selected);
        }

        void Hide()
        {
            overlay.SetActive(false);
            Time.timeScale = previousTimeScale;
            onStart = null;
        }
    }
}
