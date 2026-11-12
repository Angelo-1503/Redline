using Redline.Core;
using Redline.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Redline.UI
{
    /// <summary>HUD do operador: retrato, vida numérica e barra contínua. Mantém as referências das cenas.</summary>
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Health targetHealth;
        [SerializeField] private Image fillImage;
        Canvas hudCanvas;
        Widget playerWidget, bossWidget;
        BossEnemy boss;
        Health bossHealth;
        bool initialized;
        GameObject legacyBar;

        void Start()
        {
            if (targetHealth == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) targetHealth = player.GetComponent<Health>();
            }
            if (targetHealth == null) { Debug.LogWarning("[HealthBarUI] Jogador sem Health."); return; }
            if (fillImage != null)
            {
                legacyBar = fillImage.transform.parent.gameObject;
                if (legacyBar != gameObject) legacyBar.SetActive(false);
                else fillImage.enabled = false;
            }
            var obj = new GameObject("HUD_Operador", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            hudCanvas = obj.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay; hudCanvas.sortingOrder = 10;
            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            playerWidget = new Widget(obj.transform, false, "VIDA");
            playerWidget.SetPortrait(Resources.Load<Texture2D>("UI/OperatorPortrait"));
            boss = FindFirstObjectByType<BossEnemy>(FindObjectsInactive.Include);
            if (boss != null)
            {
                bossHealth = boss.GetComponent<Health>();
                bossWidget = new Widget(obj.transform, true, BossName());
                var renderer = boss.GetComponentInChildren<SpriteRenderer>(true);
                if (renderer != null && renderer.sprite != null) bossWidget.SetPortrait(renderer.sprite);
                bossWidget.Root.gameObject.SetActive(false);
            }
            initialized = true;
            targetHealth.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
        void Update()
        {
            if (!initialized) return;
            // Insets in canvas coordinates keep both cards inside device safe areas.
            Rect safe = Screen.safeArea;
            float scale = Mathf.Max(0.001f, hudCanvas.scaleFactor);
            playerWidget.Root.anchoredPosition = new Vector2(28 + safe.xMin / scale, -28 - (Screen.height - safe.yMax) / scale);
            if (bossWidget != null)
            {
                bossWidget.Root.anchoredPosition = new Vector2(-28 - (Screen.width - safe.xMax) / scale, -28 - (Screen.height - safe.yMax) / scale);
                bool visible = boss != null && boss.isActiveAndEnabled && bossHealth != null && !bossHealth.IsDead;
                bossWidget.Root.gameObject.SetActive(visible);
                if (visible) bossWidget.SetHealth(bossHealth.CurrentHealth, bossHealth.MaxHealth);
            }
        }
        void OnEnable()
        {
            if (!initialized) return;
            hudCanvas.gameObject.SetActive(true);
            targetHealth.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
        void OnDisable()
        {
            if (targetHealth != null) targetHealth.OnHealthChanged -= HandleHealthChanged;
            if (hudCanvas != null) hudCanvas.gameObject.SetActive(false);
        }
        void OnDestroy()
        {
            if (targetHealth != null) targetHealth.OnHealthChanged -= HandleHealthChanged;
            if (hudCanvas != null) Destroy(hudCanvas.gameObject);
        }
        void HandleHealthChanged(int current, int max) { playerWidget?.SetHealth(current, max); }
        static string BossName()
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "Fase01": return "OLOF";
                case "Fase02": return "DEVICE";
                case "Fase03": return "S1MPLE";
                case "Fase04": return "ZYWOO";
                case "Fase05": return "DONK";
                case "Fase06": return "FalleN";
                default: return "CHEFÃO";
            }
        }

        sealed class Widget
        {
            public readonly RectTransform Root;
            readonly RectTransform fill;
            readonly Text number;
            readonly TacticalPlate frame, portraitFrame;
            readonly RawImage portrait;
            static readonly Color Ivory = new Color32(241, 235, 211, 255);
            static readonly Color Red = new Color32(204, 12, 48, 255);
            static readonly Color Steel = new Color32(100, 111, 134, 255);

            public Widget(Transform parent, bool right, string title)
            {
                Root = At(parent, "Cartao_" + title, 0, 0, 540, 116);
                Root.anchorMin = Root.anchorMax = Root.pivot = new Vector2(right ? 1 : 0, 1);
                Root.anchoredPosition = new Vector2(right ? -28 : 28, -28);
                Plate(Root, "Sombra", 4, 6, 536, 110, new Color(0, 0, 0, 0.65f));
                frame = Plate(Root, "Moldura", 78, 31, 458, 81, Steel);
                Plate(Root, "PlacaMarinho", 81, 34, 452, 75, new Color32(17, 24, 41, 255));
                Plate(Root, "Etiqueta", 100, 15, 270, 50, new Color32(17, 24, 41, 255));
                Box(Root, "Assinatura", 118, 31, 3, 22, Red);
                Label(Root, title, 23, 132, 26, 175, 34, Ivory);
                number = Label(Root, "", 23, 322, 26, 176, 34, Ivory);
                number.alignment = TextAnchor.MiddleRight;
                Box(Root, "BordaBarra", 116, 70, 384, 25, Ivory);
                Box(Root, "TrilhaVazia", 119, 73, 378, 19, new Color32(40, 44, 59, 255));
                var track = At(Root, "AreaVida", 119, 73, 378, 19);
                fill = At(track, "VidaVermelha", 0, 0, 378, 19);
                fill.gameObject.AddComponent<Image>().color = Red;
                fill.GetComponent<Image>().raycastTarget = false;
                Box(fill, "BrilhoSuperior", 0, 0, 0, 2, new Color32(247, 48, 75, 255)).rectTransform.anchorMax = new Vector2(1, 1);
                portraitFrame = Plate(Root, "MolduraRetrato", 0, 0, 112, 116, Steel);
                Plate(Root, "FundoRetrato", 3, 3, 106, 110, new Color32(12, 17, 30, 255));
                var clipping = At(Root, "RecorteRetrato", 8, 8, 96, 100);
                clipping.gameObject.AddComponent<RectMask2D>();
                var image = At(clipping, "Retrato", 0, 0, 96, 100);
                portrait = image.gameObject.AddComponent<RawImage>(); portrait.raycastTarget = false;
            }
            public void SetPortrait(Texture2D texture)
            {
                portrait.texture = texture;
                portrait.enabled = texture != null;
            }
            public void SetPortrait(Sprite sprite)
            {
                portrait.texture = sprite.texture;
                Rect r = sprite.rect;
                // Crop the existing boss sprite to head and upper torso instead of inventing a new likeness.
                portrait.uvRect = new Rect(r.x / sprite.texture.width, (r.y + r.height * 0.5f) / sprite.texture.height,
                    r.width / sprite.texture.width, r.height * 0.5f / sprite.texture.height);
            }
            public void SetHealth(int current, int max)
            {
                float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0;
                fill.localScale = new Vector3(ratio, 1, 1);
                number.text = Mathf.Max(0, current) + "/" + Mathf.Max(0, max);
                bool critical = ratio <= 0.25f;
                frame.color = portraitFrame.color = critical ? Red : Steel;
                number.color = critical ? new Color32(255, 45, 73, 255) : Ivory;
            }
            static RectTransform At(Transform parent, string name, float x, float y, float w, float h)
            {
                var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
                var r = obj.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
                r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
            }
            static TacticalPlate Plate(Transform p, string n, float x, float y, float w, float h, Color color)
            {
                var g = At(p, n, x, y, w, h).gameObject.AddComponent<TacticalPlate>();
                g.color = color; g.raycastTarget = false; return g;
            }
            static Image Box(Transform p, string n, float x, float y, float w, float h, Color color)
            {
                var g = At(p, n, x, y, w, h).gameObject.AddComponent<Image>();
                g.color = color; g.raycastTarget = false; return g;
            }
            static Text Label(Transform p, string value, int size, float x, float y, float w, float h, Color color)
            {
                var t = At(p, "Texto", x, y, w, h).gameObject.AddComponent<Text>();
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.fontSize = size; t.fontStyle = FontStyle.Bold; t.color = color; t.text = value;
                t.alignment = TextAnchor.MiddleLeft; t.raycastTarget = false; return t;
            }
        }
    }
}
