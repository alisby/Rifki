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
                "Oyun Kuralları",
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
@"<size=27><color=#E4BD58><b>OYUN DÜZENİ</b></color></size>

• Oyun 4 kişiyle ve toplam 20 oyun üzerinden oynanır. Her oyuncuya 13 kart dağıtılır.
• Her oyuncu 5 kez kontrat söyler: 3 ceza ve 2 koz.
• Altı ceza türünün her biri parti boyunca iki kez oynanır; toplam 12 ceza ve 8 koz oyunu vardır.
• İlk dört oyunda koz söylenemez.
• İlk kontratı Karo 2'nin bulunduğu oyuncu söyler. Kontrat sırası her oyundan sonra kontratçının sağındaki oyuncuya geçer.
• Bir oyuncu üç ceza kontratı hakkını kullandıktan sonra kalan iki çağrısında koz söylemek zorundadır.
• King ilanıyla parti erken sona ermezse 20 oyunun sonunda toplam puanı en yüksek oyuncu kazanır. En yüksek puanı birden fazla oyuncu paylaşırsa bu oyuncular birlikte kazanır.

<size=27><color=#E4BD58><b>TEMEL OYNANIŞ</b></color></size>

• Kontratı söyleyen oyuncu ilk kartı oynar. Her eli kazanan oyuncu sonraki ele başlar.
• Koz olmayan oyunlarda eli açılan rengin en yüksek kartı kazanır. Koz oyununda koz oynanmışsa en yüksek koz; koz oynanmamışsa açılan rengin en yüksek kartı eli kazanır. Kart sırası 2, 3, …, 10, Vale, Kız, Papaz, As şeklindedir.
• Açılan renkten elde kart varsa o renkten oynamak zorunludur.
• Ceza oyunlarında açılan renkten kart yoksa, elde o oyuna ait ceza kartı varsa ceza kartı atılmak zorundadır.
• Açılan renge uyarken eldeki ceza kartı masadaki daha yüksek kart nedeniyle artık eli alamayacak durumdaysa, o ceza kartı oynanmak zorundadır.
• Puanlanacak bütün ceza kartları oynandığında oyun 13 el tamamlanmadan sona erebilir.

<size=27><color=#E4BD58><b>CEZA OYUNLARI</b></color></size>

<b>El Almaz</b>
• Alınan her el −50 puandır.

<b>Kupa Almaz</b>
• Alınan her kupa −30 puandır.
• Kupa açılmadan kupayla ele başlanamaz; elde açılan renkten yoksa ve kupa varsa kupa atılır.
• Elde yalnızca kupa kaldığında kupayla başlanabilir.

<b>Kız Almaz</b>
• Alınan her Kız −100 puandır.
• Açılan renkten yoksa elde Kız varsa Kız atılır.
• Dört oyuncunun her biri birer Kız alırsa oyun iptal edilir; kontrat hakkı geri verilir ve aynı kontratçı yeniden oyun seçer.

<b>Erkek Almaz</b>
• Vale ve Papazlar erkektir. Alınan her erkek −60 puandır.
• Açılan renkten yoksa elde erkek varsa erkek atılır.

<b>Rıfkı</b>
• Kupa Papazı Rıfkı'dır ve −320 puandır.
• Kupa açılmadan kupayla ele başlanamaz.
• Açılan renkten yoksa elde Rıfkı varsa önce Rıfkı atılmak zorundadır; Rıfkı yoksa elde kupa varsa kupa atılır.
• Rıfkı alındığında oyun sona erer.
• Kupa grubunda yalnızca K♥ veya yalnızca K♥ + A♥ bulunan oyuncu, oyun başlamadan Rıfkı elini bozabilir. El bozulursa aynı Rıfkı kontratı yeniden dağıtılır.

<b>Son İki</b>
• İlk 11 el puan getirmez. Son iki elin her biri −180 puandır.

<size=27><color=#E4BD58><b>KOZ OYUNU</b></color></size>

• Alınan her el +50 puandır.
• Açılan renkten kart yoksa elde koz varsa koz atmak zorunludur.
• Ele kozla başlanmışsa ve masadaki en yüksek kozu geçebilecek koz varsa yükseltmek zorunludur.
• Başka bir renge çakarken masadaki kozu yükseltmek zorunlu değildir.
• Koz açılmadan normal olarak kozla ele başlanamaz. Elde yalnızca koz kaldığında kozla başlanabilir.
• İstisna olarak, kontratçının başlangıç elinde koz A-K-Q birlikteyse ilk ele bu üç kozdan biriyle başlayabilir. Kontratçı ilk eli başka renkle açarsa bu özel hak sona erer.
• Koz oyununda elinde hiçbir Vale, Kız, Papaz veya As bulunmayan oyuncu oyun başlamadan eli bozabilir. El bozulursa aynı koz kontratı yeniden dağıtılır.

<size=27><color=#E4BD58><b>KING İLANI</b></color></size>

• Koz kontratını söyleyen oyuncu oyun başlamadan King ilan edebilir.
• İlan eden oyuncu 10 el alırsa tek başına çıkar ve parti sona erer.
• Rakipler toplam 4 el aldığında ilan eden artık 10 ele ulaşamayacağı için King başarısız olur; ilan eden tek başına batar ve parti sona erer.

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
