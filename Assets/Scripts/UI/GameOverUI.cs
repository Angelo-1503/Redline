using System.Collections;
using Redline.Core;
using Redline.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Redline.UI
{
    /// <summary>Tela de derrota original do Redline. Mantém as referências das seis cenas.</summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        Health playerHealth;
        Canvas canvas;
        CanvasGroup group;
        Text status;
        Button retryButton;
        bool showing, loading;
        bool previousAudioPause;
        float previousVolume, previousTimeScale;
        CursorLockMode previousLock;
        bool previousCursor;
        static readonly Color Red = new Color32(213, 43, 59, 255);
        static readonly Color Ivory = new Color32(241, 235, 223, 255);
        static readonly Color Muted = new Color32(163, 168, 179, 255);

        void Start()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            var player = GameObject.FindGameObjectWithTag("Player");
            playerHealth = player != null ? player.GetComponent<Health>() : null;
            if (playerHealth != null) playerHealth.OnDied += HandlePlayerDied;
            else Debug.LogWarning("[GameOverUI] Não encontrou o Health do jogador.");
        }
        void HandlePlayerDied()
        {
            if (showing) return;
            showing = true;
            previousTimeScale = Time.timeScale;
            previousAudioPause = AudioListener.pause;
            previousVolume = AudioListener.volume;
            previousCursor = Cursor.visible; previousLock = Cursor.lockState;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            BuildInterface();
            StartCoroutine(Reveal());
        }
        void BuildInterface()
        {
            var obj = new GameObject("RedlineGameOver", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = obj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 3000;
            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            group = obj.AddComponent<CanvasGroup>(); group.alpha = 0f;
            group.interactable = false; group.blocksRaycasts = true;
            var backdrop = At(obj.transform, "ArteDaDerrota", 0, 0, 0, 0);
            Stretch(backdrop);
            backdrop.pivot = new Vector2(0.5f, 0.5f);
            var art = backdrop.gameObject.AddComponent<RawImage>();
            var texture = Resources.Load<Texture2D>("UI/RedlineGameOverBackground");
            art.texture = texture;
            art.color = texture != null ? Color.white : new Color32(12, 16, 26, 255);
            if (texture != null)
            {
                var fitter = backdrop.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = (float)texture.width / texture.height;
            }
            var shade = Box(obj.transform, "SombraDeLeitura", new Color(0.02f, 0.025f, 0.045f, 0.25f), 0, 0, 0, 0);
            Stretch(shade.rectTransform); shade.raycastTarget = false;

            var safe = At(obj.transform, "AreaSegura", 0, 0, 0, 0);
            Rect area = Screen.safeArea;
            safe.anchorMin = new Vector2(area.xMin / Mathf.Max(1, Screen.width), area.yMin / Mathf.Max(1, Screen.height));
            safe.anchorMax = new Vector2(area.xMax / Mathf.Max(1, Screen.width), area.yMax / Mathf.Max(1, Screen.height));
            safe.offsetMin = safe.offsetMax = Vector2.zero;
            var panel = At(safe, "ConteudoDerrota", 0, 0, 780, 810);
            panel.anchorMin = panel.anchorMax = new Vector2(0.06f, 0.5f);
            panel.pivot = new Vector2(0, 0.5f); panel.anchoredPosition = Vector2.zero;
            Label(panel, "RED<color=#D52B3B>LINE</color>  /  FIM DA RODADA", 23, Muted, 0, 20, 750, 42);
            Box(panel, "TracoVermelho", Red, 0, 108, 86, 5);
            Label(panel, "GAME OVER", 108, Ivory, -6, 150, 780, 138).fontStyle = FontStyle.Bold;
            Label(panel, "A rodada acabou.\nSua história continua.", 30, Muted, 0, 300, 740, 100);
            Label(panel, PhaseLabel() + "   /   " + GameDifficulty.Label, 19, Ivory, 0, 421, 750, 42);
            retryButton = ActionButton(panel, "TentarNovamente", "TENTAR NOVAMENTE", "01", 490, true, Restart);
            ActionButton(panel, "MenuInicial", "VOLTAR AO MENU INICIAL", "02", 603, false, ReturnToMenu);
            status = Label(panel, "A fase reinicia na dificuldade escolhida.", 18, Muted, 0, 705, 750, 36);
            Label(panel, "MOUSE OU TECLADO  /  ENTER PARA SELECIONAR", 15, Muted, 0, 768, 750, 30);
            if (EventSystem.current == null)
                new GameObject("GameOverEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            EventSystem.current.SetSelectedGameObject(null);
        }
        IEnumerator Reveal()
        {
            const float duration = 0.7f;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime / duration)
            {
                group.alpha = Mathf.SmoothStep(0, 1, t);
                AudioListener.volume = previousVolume * (1f - t);
                yield return null;
            }
            // O breve fade deixa o som da morte terminar antes de suspender o áudio da fase.
            AudioListener.pause = true; AudioListener.volume = previousVolume;
            group.alpha = 1f;
            yield return null;
            group.interactable = true;
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }
        public void Restart() { BeginLoad(SceneManager.GetActiveScene().name); }
        public void ReturnToMenu() { BeginLoad("MenuInicial"); }
        void BeginLoad(string scene)
        {
            if (!showing || loading || group == null || !group.interactable) return;
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                status.text = "A cena não está disponível no build.";
                return;
            }
            loading = true; group.interactable = false;
            StartCoroutine(LoadScene(scene));
        }
        IEnumerator LoadScene(string scene)
        {
            var black = Box(canvas.transform, "Transicao", Color.clear, 0, 0, 0, 0);
            Stretch(black.rectTransform);
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.3f)
            { black.color = new Color(0, 0, 0, t); yield return null; }
            black.color = Color.black;
            Time.timeScale = 1f; AudioListener.pause = false; AudioListener.volume = previousVolume;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            SceneManager.LoadScene(scene);
        }
        void OnDestroy()
        {
            if (playerHealth != null) playerHealth.OnDied -= HandlePlayerDied;
            if (canvas != null) Destroy(canvas.gameObject);
            if (showing && !loading)
            {
                Time.timeScale = previousTimeScale;
                AudioListener.pause = previousAudioPause; AudioListener.volume = previousVolume;
                Cursor.lockState = previousLock; Cursor.visible = previousCursor;
            }
        }
        static string PhaseLabel()
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "Fase01": return "FASE 1 / DUST II";
                case "Fase02": return "FASE 2 / NUKE";
                case "Fase03": return "FASE 3 / MIRAGE";
                case "Fase04": return "FASE 4 / TRAIN";
                case "Fase05": return "FASE 5 / OVERPASS";
                case "Fase06": return "FASE FINAL / INFERNO";
                default: return "CAMPANHA";
            }
        }
        static Button ActionButton(Transform parent, string name, string value, string number, float y, bool primary, UnityEngine.Events.UnityAction action)
        {
            var rect = At(parent, name, 0, y, 660, 88);
            var plate = rect.gameObject.AddComponent<TacticalPlate>(); plate.color = Color.white;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = plate;
            var colors = button.colors;
            colors.normalColor = primary ? Red : new Color32(22, 30, 46, 250);
            colors.highlightedColor = primary ? new Color32(246, 52, 71, 255) : new Color32(55, 66, 86, 255);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color32(132, 16, 35, 255); colors.fadeDuration = 0.12f;
            button.colors = colors;
            Label(rect, number, 18, new Color(1, 1, 1, 0.65f), 28, 0, 48, 88);
            Label(rect, value, 25, Ivory, 88, 0, 522, 88).fontStyle = FontStyle.Bold;
            Label(rect, "›", 34, Ivory, 612, 0, 40, 88);
            Box(rect, "LinhaInferior", new Color(1, 1, 1, 0.15f), 14, 85, 632, 1);
            button.onClick.AddListener(action); return button;
        }
        static RectTransform At(Transform parent, string name, float x, float y, float width, float height)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height); return rect;
        }
        static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        static Image Box(Transform parent, string name, Color color, float x, float y, float width, float height)
        {
            var image = At(parent, name, x, y, width, height).gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = false; return image;
        }
        static Text Label(Transform parent, string value, int size, Color color, float x, float y, float width, float height)
        {
            var text = At(parent, "Texto", x, y, width, height).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size;
            text.text = value; text.color = color; text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = true; text.raycastTarget = false; return text;
        }
    }
}
