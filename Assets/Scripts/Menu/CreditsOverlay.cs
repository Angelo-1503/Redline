using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Redline.Menu
{
    /// <summary>Créditos compartilhados pelo menu e pelo encerramento da campanha.</summary>
    public sealed class CreditsOverlay : MonoBehaviour
    {
        Action onClose;
        bool closing;
        float readyAt;

        public static void Show(Font font, Action onClose, bool campaignComplete = false)
        {
            var root = new GameObject("CreditosRedline", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CreditsOverlay));
            var overlay = root.GetComponent<CreditsOverlay>();
            overlay.onClose = onClose;
            overlay.readyAt = Time.unscaledTime + 0.2f;
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            if (EventSystem.current == null)
                new GameObject("CreditsEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var bg = Box(root.transform, "Fundo", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.gameObject.AddComponent<Image>().color = new Color32(12, 16, 22, 255);
            var panel = Box(bg, "Conteudo", new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.92f), Vector2.zero, Vector2.zero);
            var red = new Color32(213, 43, 59, 255);
            var title = Label(panel, font, "REDLINE  /  CRÉDITOS", 46, red);
            title.anchorMin = new Vector2(0, 0.88f); title.anchorMax = Vector2.one;

            var viewport = Box(panel, "JanelaRolagem", new Vector2(0, 0.20f), new Vector2(1, 0.86f), Vector2.zero, Vector2.zero);
            viewport.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, 0.001f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            var content = Box(viewport, "TextoCreditos", new Vector2(0, 1), Vector2.one, Vector2.zero, new Vector2(0, 560));
            content.pivot = new Vector2(0.5f, 1);
            var text = content.gameObject.AddComponent<Text>();
            text.font = font; text.fontSize = 30; text.lineSpacing = 1.15f;
            text.alignment = TextAnchor.UpperCenter; text.color = new Color32(241, 235, 223, 255);
            text.raycastTarget = false; text.supportRichText = true;
            var data = Resources.Load<TextAsset>("RedlineCredits");
            text.text = data != null ? data.text : "REDLINE\nObrigado por jogar.";
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;

            var buttonRect = Box(panel, "Voltar", new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.15f), Vector2.zero, Vector2.zero);
            var image = buttonRect.gameObject.AddComponent<Image>(); image.color = red;
            var button = buttonRect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.7f, 0.7f);
            colors.selectedColor = colors.highlightedColor; button.colors = colors;
            Label(buttonRect, font, campaignComplete ? "VOLTAR AO MENU INICIAL" : "VOLTAR", 25, Color.white);
            button.onClick.AddListener(overlay.Close);
            EventSystem.current.SetSelectedGameObject(button.gameObject);
            Canvas.ForceUpdateCanvases(); scroll.verticalNormalizedPosition = 1f;
        }

        void Update()
        {
            if (Time.unscaledTime >= readyAt && Input.GetKeyDown(KeyCode.Escape)) Close();
        }
        void Close()
        {
            if (closing) return;
            closing = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
            onClose?.Invoke();
        }
        static RectTransform Label(Transform parent, Font font, string value, int size, Color color)
        {
            var rect = Box(parent, "Texto", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size;
            text.color = color; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            return rect;
        }
        static RectTransform Box(Transform parent, string name, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }
    }
}
