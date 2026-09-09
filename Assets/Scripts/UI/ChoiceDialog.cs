using System;
using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class ChoiceDialog
    {
        static readonly Color PanelColor =
            new Color(0.015f, 0.115f, 0.055f, 0.985f);

        static readonly Color PanelBorder =
            new Color(0.72f, 0.54f, 0.18f, 1f);

        readonly GameObject overlay;
        readonly Text titleText;
        readonly Text messageText;
        readonly Text leftText;
        readonly Text rightText;
        Action leftAction;
        Action rightAction;

        public ChoiceDialog(Transform canvas)
        {
            overlay = new GameObject("ChoiceOverlay", typeof(RectTransform), typeof(Image));
            var overlayRect = (RectTransform)overlay.transform;
            overlayRect.SetParent(canvas, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var shade = overlay.GetComponent<Image>();
            shade.color = new Color(0f, 0f, 0f, 0.65f);
            shade.raycastTarget = true;

            var panelBorder = UiKit.Rect(
                "ChoicePanelBorder",
                overlayRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(742f, 330f));

            UiKit.RoundedImage(panelBorder, PanelBorder);

            var panel = UiKit.Rect(
                "ChoicePanel",
                panelBorder,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(730f, 318f));

            UiKit.RoundedImage(panel, PanelColor);

            titleText = UiKit.Label(
                "Title", panel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 88f), new Vector2(560f, 52f),
                "", 38, CardStyle.Cream, TextAnchor.MiddleCenter);
            titleText.fontStyle = FontStyle.Bold;

            var leftLine = UiKit.Rect(
                "TitleLineLeft",
                panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-220f, 88f),
                new Vector2(90f, 3f));

            UiKit.RoundedImage(leftLine, PanelBorder);

            var rightLine = UiKit.Rect(
                "TitleLineRight",
                panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(220f, 88f),
                new Vector2(90f, 3f));

            UiKit.RoundedImage(rightLine, PanelBorder);

            messageText = UiKit.Label(
                "Message", panel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 20f), new Vector2(630f, 88f),
                "", 25, CardStyle.Cream, TextAnchor.MiddleCenter);

            messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            messageText.verticalOverflow = VerticalWrapMode.Overflow;

            var leftRect = UiKit.Rect(
                "LeftChoice", panel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-165f, -96f), new Vector2(250f, 64f));
            var leftImage = UiKit.RoundedImage(leftRect, new Color(0.055f, 0.18f, 0.105f, 1f));
            var leftButton = UiKit.MakeButton(leftImage);
            leftText = UiKit.Fill("Label", leftRect, "", 28, CardStyle.Cream, TextAnchor.MiddleCenter);
            leftText.fontStyle = FontStyle.Bold;
            leftButton.onClick.AddListener(ChooseLeft);

            var rightRect = UiKit.Rect(
                "RightChoice", panel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(165f, -96f), new Vector2(250f, 64f));
            var rightImage = UiKit.RoundedImage(rightRect, new Color(0.055f, 0.18f, 0.105f, 1f));
            var rightButton = UiKit.MakeButton(rightImage);
            rightText = UiKit.Fill("Label", rightRect, "", 28, CardStyle.Cream, TextAnchor.MiddleCenter);
            rightText.fontStyle = FontStyle.Bold;
            rightButton.onClick.AddListener(ChooseRight);

            overlay.SetActive(false);
        }

        public void Show(
            string title,
            string message,
            string leftLabel,
            Action onLeft,
            string rightLabel,
            Action onRight)
        {
            titleText.text = title;
            messageText.text = message;
            leftText.text = leftLabel;
            rightText.text = rightLabel;
            leftAction = onLeft;
            rightAction = onRight;
            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();
        }

        void ChooseLeft()
        {
            overlay.SetActive(false);
            var callback = leftAction;
            ClearCallbacks();
            callback?.Invoke();
        }

        void ChooseRight()
        {
            overlay.SetActive(false);
            var callback = rightAction;
            ClearCallbacks();
            callback?.Invoke();
        }

        void ClearCallbacks()
        {
            leftAction = null;
            rightAction = null;
        }
    }
}
