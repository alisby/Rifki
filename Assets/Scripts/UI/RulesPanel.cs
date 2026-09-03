using UnityEngine;
using UnityEngine.UI;

namespace King.UI
{
    public sealed class RulesPanel
    {
        readonly GameObject overlay;
        readonly ScrollRect scrollRect;
        float previousTimeScale = 1f;

        public RulesPanel(Transform canvas)
        {
            var toggleRect = UiKit.Rect(
                "RulesToggle",
                canvas,
                Vector2.one,
                Vector2.one,
                new Vector2(-24f, -188f),
                new Vector2(150f, 48f));

            var toggleImage = UiKit.RoundedImage(
                toggleRect,
                new Color(0.05f, 0.14f, 0.08f, 0.92f));

            var toggle = UiKit.MakeButton(toggleImage);

            UiKit.Fill(
                "Label",
                toggleRect,
                "Kurallar",
                24,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            toggle.onClick.AddListener(Show);

            var overlayGo = new GameObject(
                "RulesOverlay",
                typeof(RectTransform),
                typeof(Image));

            var overlayRect =
                (RectTransform)overlayGo.transform;

            overlayRect.SetParent(canvas, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var overlayImage =
                overlayGo.GetComponent<Image>();

            overlayImage.color =
                new Color(0f, 0f, 0f, 0.72f);

            overlayImage.raycastTarget = true;
            overlay = overlayGo;

            var panelGo = new GameObject(
                "RulesPanel",
                typeof(RectTransform));

            var panel =
                (RectTransform)panelGo.transform;

            panel.SetParent(overlayRect, false);
            panel.anchorMin = new Vector2(0.04f, 0.05f);
            panel.anchorMax = new Vector2(0.96f, 0.95f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            UiKit.RoundedImage(
                panel,
                new Color(0.035f, 0.12f, 0.07f, 0.995f));

            var title = UiKit.Label(
                "Title",
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -34f),
                new Vector2(760f, 54f),
                "Mevcut Kurallar",
                36,
                CardStyle.Gold,
                TextAnchor.MiddleCenter);

            title.fontStyle = FontStyle.Bold;

            var closeRect = UiKit.Rect(
                "Close",
                panel,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-32f, -30f),
                new Vector2(126f, 46f));

            var closeImage = UiKit.RoundedImage(
                closeRect,
                new Color(0.12f, 0.30f, 0.19f, 1f));

            var close = UiKit.MakeButton(closeImage);

            UiKit.Fill(
                "Label",
                closeRect,
                "Kapat",
                22,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            close.onClick.AddListener(Hide);

            var scrollAreaGo = new GameObject(
                "ScrollArea",
                typeof(RectTransform),
                typeof(ScrollRect));

            var scrollArea =
                (RectTransform)scrollAreaGo.transform;

            scrollArea.SetParent(panel, false);
            scrollArea.anchorMin = Vector2.zero;
            scrollArea.anchorMax = Vector2.one;
            scrollArea.offsetMin = new Vector2(34f, 28f);
            scrollArea.offsetMax = new Vector2(-34f, -94f);

            scrollRect =
                scrollAreaGo.GetComponent<ScrollRect>();

            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType =
                ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.12f;
            scrollRect.scrollSensitivity = 38f;

            var viewportGo = new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(Image),
                typeof(RectMask2D));

            var viewport =
                (RectTransform)viewportGo.transform;

            viewport.SetParent(scrollArea, false);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = new Vector2(-30f, 0f);

            var viewportImage =
                viewportGo.GetComponent<Image>();

            viewportImage.color =
                new Color(0f, 0f, 0f, 0.001f);

            viewportImage.raycastTarget = true;

            const string rules =
@"<size=27><color=#E4BD58><b>TEMEL OYNANIŞ</b></color></size>

• Oyun 4 kişiyle ve 52 kartla oynanır. Her oyuncuya 13 kart dağıtılır.
• Bir oyun toplam 20 elden oluşur.
• Her oyuncu toplam 5 kontrat söyler: 3 ceza oyunu ve 2 koz oyunu.
• İlk dört elde koz kontratı söylenemez.
• Kontrat söyleme sırası her elde bir sonraki oyuncuya geçer.

<size=27><color=#E4BD58><b>KART OYNAMA KURALLARI</b></color></size>

• Yere açılan renkten elde kart varsa aynı renkten oynamak zorunludur.
• Kozsuz oyunlarda açılan renkten kart yoksa uygun olan herhangi bir kart oynanabilir.
• Bir eli, koz kullanılmamışsa açılan rengin en yüksek kartı kazanır.
• Koz oynanmışsa en yüksek koz eli kazanır.

<size=27><color=#E4BD58><b>KOZ OYUNU</b></color></size>

• Açılan renkten elde kart yoksa ve elde koz varsa koz atmak zorunludur.
• El kozla açılmışsa ve masadaki en yüksek kozu geçebilecek koz varsa daha yüksek koz oynamak zorunludur.
• Başka bir renge kozla çakarken masadaki kozu yükseltmek zorunlu değildir.
• Açılan renkten de kozdan da yoksa herhangi bir kart oynanabilir.
• Koz oyununda alınan eller pozitif puan getirir.

<size=27><color=#E4BD58><b>CEZA KONTRATLARI</b></color></size>

<b>El Almaz</b>
Alınan her el ceza getirir.

<b>Kupa Almaz</b>
Alınan her kupa ceza getirir.
Kupa henüz açılmamışsa kupa ile çıkılmaz.
Açılan renkten elde kart yoksa ve elde kupa varsa kupa atmak zorunludur; böylece kupa açılmış olur.

<b>Kız Almaz</b>
Alınan her kız ceza getirir.
Dört oyuncunun her biri tam bir kız alırsa el iptal edilir ve aynı el yeniden oynanır.

<b>Erkek Almaz</b>
Vale ve papazlar ceza kartıdır.

<b>Rıfkı</b>
Kupa papazını alan oyuncu cezayı alır.

<b>Son İki</b>
Oyunun son iki elini alan oyuncular ceza alır.

<size=27><color=#E4BD58><b>KOZ ELİNİ BOZMA</b></color></size>

• Koz kontratında bir oyuncunun elinde Vale, Kız, Papaz veya As hiç yoksa oyuncu eli bozabilir.
• El bozulursa kartlar yeniden dağıtılır ve aynı koz kontratı yeniden oynanır.

<size=27><color=#E4BD58><b>KOZDA RIFKI İLANI</b></color></size>

• Koz söyleyen oyuncu el başlamadan önce Rıfkı ilan edebilir.
• Rıfkı ilan eden oyuncu 10 el alırsa tek başına çıkar ve oyun sona erer.
• Diğer üç oyuncu toplam 4 el aldığı anda Rıfkı ilan eden oyuncu başarısız olur, tek başına batar ve oyun sona erer.

<size=27><color=#E4BD58><b>PUANLAMA VE OYUN SONU</b></color></size>

• Ceza kontratlarında alınan ceza birimleri negatif puan getirir.
• Koz kontratında alınan eller pozitif puan getirir.
• Normal oyun 20 kontrat tamamlandığında sona erer.
• Toplam puanı en yüksek olan oyuncu sıralamada önde yer alır.

";

            var text = UiKit.Label(
                "RulesText",
                viewport,
                new Vector2(0f, 1f),
                new Vector2(0.5f, 1f),
                Vector2.zero,
                new Vector2(100f, 100f),
                rules,
                23,
                CardStyle.Cream,
                TextAnchor.UpperLeft);

            text.supportRichText = true;
            text.horizontalOverflow =
                HorizontalWrapMode.Wrap;
            text.verticalOverflow =
                VerticalWrapMode.Overflow;
            text.lineSpacing = 1.12f;

            var content =
                (RectTransform)text.transform;

            content.anchorMin =
                new Vector2(0f, 1f);
            content.anchorMax =
                new Vector2(1f, 1f);
            content.pivot =
                new Vector2(0.5f, 1f);
            content.anchoredPosition =
                Vector2.zero;
            content.sizeDelta =
                new Vector2(-24f, 0f);

            var fitter =
                text.gameObject.AddComponent<ContentSizeFitter>();

            fitter.horizontalFit =
                ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var scrollbarGo = new GameObject(
                "Scrollbar",
                typeof(RectTransform),
                typeof(Image),
                typeof(Scrollbar));

            var scrollbarRect =
                (RectTransform)scrollbarGo.transform;

            scrollbarRect.SetParent(scrollArea, false);
            scrollbarRect.anchorMin =
                new Vector2(1f, 0f);
            scrollbarRect.anchorMax =
                new Vector2(1f, 1f);
            scrollbarRect.pivot =
                new Vector2(1f, 0.5f);
            scrollbarRect.offsetMin =
                new Vector2(-18f, 0f);
            scrollbarRect.offsetMax =
                Vector2.zero;

            var track =
                scrollbarGo.GetComponent<Image>();

            track.color =
                new Color(0.02f, 0.08f, 0.045f, 0.95f);

            var handleGo = new GameObject(
                "Handle",
                typeof(RectTransform),
                typeof(Image));

            var handle =
                (RectTransform)handleGo.transform;

            handle.SetParent(scrollbarRect, false);
            handle.anchorMin = Vector2.zero;
            handle.anchorMax = Vector2.one;
            handle.offsetMin = new Vector2(3f, 3f);
            handle.offsetMax = new Vector2(-3f, -3f);

            var handleImage =
                handleGo.GetComponent<Image>();

            handleImage.color =
                new Color(0.62f, 0.52f, 0.25f, 1f);

            var scrollbar =
                scrollbarGo.GetComponent<Scrollbar>();

            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;
            scrollbar.direction =
                Scrollbar.Direction.BottomToTop;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility =
                ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.verticalScrollbarSpacing = 8f;

            overlay.SetActive(false);
        }

        void Show()
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        void Hide()
        {
            overlay.SetActive(false);
            Time.timeScale = previousTimeScale;
        }
    }
}
