using System;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class PlayerNameScreen
    {
        static readonly string[] Keys =
        {
            "Rifki.PlayerName.South",
            "Rifki.PlayerName.West",
            "Rifki.PlayerName.North",
            "Rifki.PlayerName.East"
        };

        static readonly string[] Defaults =
        {
            "Güney",
            "Batı",
            "Kuzey",
            "Doğu"
        };

        readonly GameObject overlay;
        readonly InputField[] fields = new InputField[4];
        readonly Action<string, string, string, string> onStart;

        public PlayerNameScreen(
            Transform canvas,
            Action<string, string, string, string> onStart)
        {
            this.onStart = onStart;

            var root = UiKit.Rect(
                "PlayerNames",
                canvas,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero);

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var dim = UiKit.RoundedImage(
                root,
                new Color(0f, 0f, 0f, 0.65f));

            dim.raycastTarget = true;
            overlay = root.gameObject;

            var panel = UiKit.Rect(
                "Panel",
                overlay.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(680f, 620f));

            UiKit.RoundedImage(
                panel,
                new Color(0.07f, 0.19f, 0.11f, 0.98f));

            UiKit.Label(
                "Title",
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -32f),
                new Vector2(580f, 48f),
                "Oyuncu isimleri",
                36,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            UiKit.Label(
                "Info",
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -82f),
                new Vector2(580f, 32f),
                "İsimleri değiştirin veya yön adlarını bırakın",
                21,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            string[] labels =
            {
                "Güney (Siz)",
                "Batı",
                "Kuzey",
                "Doğu"
            };

            for (int i = 0; i < 4; i++)
            {
                float y = -155f - i * 90f;

                UiKit.Label(
                    "SeatLabel",
                    panel,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(-215f, y),
                    new Vector2(180f, 56f),
                    labels[i],
                    25,
                    CardStyle.Cream,
                    TextAnchor.MiddleLeft);

                string initial =
                    PlayerPrefs.GetString(Keys[i], Defaults[i]);

                fields[i] = MakeInput(
                    panel,
                    i,
                    new Vector2(95f, y),
                    initial);
            }

            var buttonRect = UiKit.Rect(
                "Start",
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -535f),
                new Vector2(220f, 62f));

            var buttonImage =
                UiKit.RoundedImage(buttonRect, CardStyle.Cream);

            var button = UiKit.MakeButton(buttonImage);

            UiKit.Fill(
                "Label",
                buttonRect,
                "Başla",
                28,
                CardStyle.BlackInk,
                TextAnchor.MiddleCenter);

            button.onClick.AddListener(Begin);
        }

        InputField MakeInput(
            RectTransform parent,
            int index,
            Vector2 position,
            string initial)
        {
            var rt = UiKit.Rect(
                "NameInput" + index,
                parent,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                position,
                new Vector2(370f, 56f));

            var image =
                UiKit.RoundedImage(rt, CardStyle.Cream);

            image.raycastTarget = true;

            var textComponent = UiKit.Fill(
                "Text",
                rt,
                initial,
                26,
                CardStyle.BlackInk,
                TextAnchor.MiddleLeft);

            textComponent.rectTransform.offsetMin =
                new Vector2(14f, 0f);

            textComponent.rectTransform.offsetMax =
                new Vector2(-14f, 0f);

            var input = rt.gameObject.AddComponent<InputField>();
            input.targetGraphic = image;
            input.textComponent = textComponent;
            input.text = initial;
            input.characterLimit = GameText.MaxPlayerNameLength;
            input.lineType = InputField.LineType.SingleLine;
            input.contentType = InputField.ContentType.Standard;
            input.customCaretColor = true;
            input.caretColor = CardStyle.BlackInk;

            return input;
        }

        void Begin()
        {
            overlay.SetActive(false);

            onStart?.Invoke(
                fields[0].text,
                fields[1].text,
                fields[2].text,
                fields[3].text);
        }

        public static void SaveNames(
            string south,
            string west,
            string north,
            string east)
        {
            PlayerPrefs.SetString(Keys[0], south);
            PlayerPrefs.SetString(Keys[1], west);
            PlayerPrefs.SetString(Keys[2], north);
            PlayerPrefs.SetString(Keys[3], east);
            PlayerPrefs.Save();
        }
    }
}
