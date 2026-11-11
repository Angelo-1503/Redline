using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Redline.Settings;

namespace Redline.Menu
{
    /// <summary>Interface original do Redline, construída com gráficos próprios e sem prefabs de UI.</summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] Sprite backgroundArt;
        [SerializeField] Font menuFont;
        [SerializeField] AudioClip previewClip;
        [SerializeField] string firstLevel = "Fase01";

        static readonly Color Red = new Color32(213, 43, 59, 255);
        static readonly Color White = new Color32(241, 235, 223, 255);
        static readonly Color Muted = new Color32(156, 160, 164, 255);
        static readonly Color Panel = new Color32(12, 16, 22, 248);
        Canvas canvas;
        CanvasGroup canvasGroup;
        GameObject home, settings, settingsDim;
        SettingsSnapshot before, draft;
        readonly Button[,] keyButtons = new Button[7, 2];
        readonly Slider[] volumeSliders = new Slider[3];
        readonly Text[] volumeNumbers = new Text[3];
        Text keyHint, homeStatus;
        Button newGameButton, settingsButton, applyButton;
        StandaloneInputModule inputModule;
        AudioSource previewSource;
        int captureAction = -1, captureSlot;
        float captureReadyAt;
        bool loading;

        void OnEnable() { GameSettings.Changed += ApplyPreviewVolume; }
        void OnDisable() { GameSettings.Changed -= ApplyPreviewVolume; }
        void ApplyPreviewVolume()
        {
            if (previewSource != null) previewSource.volume = GameSettings.EffectsVolume * 0.9f;
        }
        void Start()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameSettings.Capture();
            if (menuFont == null) menuFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (EventSystem.current == null)
                new GameObject("MenuEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            inputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
            previewSource = gameObject.AddComponent<AudioSource>();
            previewSource.playOnAwake = false; previewSource.spatialBlend = 0f;
            BuildInterface();
            StartCoroutine(Reveal());
        }
        void Update()
        {
            if (loading) return;
            if (captureAction >= 0)
            {
                if (Time.unscaledTime < captureReadyAt) return;
                for (int i = 0; i < 7; i++)
                    if (Input.GetMouseButtonDown(i)) { CaptureKey((KeyCode)((int)KeyCode.Mouse0 + i)); break; }
            }
            else if (settings != null && settings.activeSelf && Input.GetKeyDown(KeyCode.Escape)) CancelSettings();
        }
        void OnGUI()
        {
            if (captureAction < 0 || Time.unscaledTime < captureReadyAt) return;
            Event e = Event.current;
            if (e.type != EventType.KeyDown || e.keyCode == KeyCode.None) return;
            CaptureKey(e.keyCode); e.Use();
        }
        void OnDestroy()
        {
            if (captureAction >= 0 && inputModule != null) inputModule.enabled = true;
            if (settings != null && settings.activeSelf && before != null) GameSettings.Apply(before, false);
        }
        void BuildInterface()
        {
            var obj = new GameObject("RedlineMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = obj.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 300;
            var scaler = obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGroup = obj.AddComponent<CanvasGroup>();
            Transform root = obj.transform;
            var backdrop = new GameObject("ArteDoRedline", typeof(RectTransform), typeof(Image)); backdrop.transform.SetParent(root, false);
            var art = backdrop.GetComponent<Image>(); art.sprite = backgroundArt; art.raycastTarget = false;
            Stretch(art.rectTransform);
            art.rectTransform.pivot = new Vector2(0.5f, 1f);
            if (backgroundArt != null)
            {
                var fit = backdrop.AddComponent<AspectRatioFitter>(); fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = backgroundArt.rect.width / backgroundArt.rect.height;
            }
            var gradient = new GameObject("SombraEditorial", typeof(RectTransform), typeof(MenuGradient)); gradient.transform.SetParent(root, false);
            Stretch(gradient.GetComponent<RectTransform>()); gradient.GetComponent<MenuGradient>().raycastTarget = false;
            var top = Label(root, "REDLINE  /  CAMPANHA", 17, Muted, 0, 0, 500, 32);
            top.rectTransform.anchorMin = top.rectTransform.anchorMax = new Vector2(1, 1);
            top.rectTransform.pivot = new Vector2(1, 1); top.rectTransform.anchoredPosition = new Vector2(-76, -48); top.alignment = TextAnchor.MiddleRight;
            home = new GameObject("Inicio", typeof(RectTransform)); home.transform.SetParent(root, false);
            var h = home.GetComponent<RectTransform>(); h.anchorMin = h.anchorMax = new Vector2(0, 0.5f); h.pivot = new Vector2(0, 0.5f);
            h.anchoredPosition = new Vector2(112, 0); h.sizeDelta = new Vector2(720, 860);
            Label(h, "UMA HISTÓRIA. SEIS BATALHAS.", 18, Muted, 4, 10, 650, 30);
            var logo = Label(h, "RED<color=#D52B3B>LINE</color>", 142, White, -6, 48, 740, 170); logo.fontStyle = FontStyle.BoldAndItalic;
            Label(h, "CADA RODADA DEIXA UMA MARCA.", 18, White, 4, 220, 650, 32);
            Fill(h, "AssinaturaVermelha", Red, 4, 278, 92, 4);
            Label(h, "A próxima rodada começa com você.", 23, Muted, 4, 302, 650, 38);
            newGameButton = MenuButton(h, "NovoJogo", "NOVO JOGO", 4, 378, 600, 88, true, StartNewGame, "01");
            settingsButton = MenuButton(h, "Configuracoes", "CONFIGURAÇÕES", 4, 484, 600, 78, false, OpenSettings, "02");
            MenuButton(h, "Sair", "SAIR DO JOGO", 4, 580, 600, 64, false, Application.Quit, "03");
            homeStatus = Label(h, "", 18, Muted, 4, 662, 650, 45);
            Fill(h, "LinhaRodape", new Color(1, 1, 1, 0.14f), 4, 766, 600, 1);
            Label(h, "MOUSE OU TECLADO  /  ENTER PARA SELECIONAR", 15, Muted, 4, 790, 700, 34);
            settingsDim = new GameObject("FundoConfiguracoes", typeof(RectTransform), typeof(Image)); settingsDim.transform.SetParent(root, false);
            Stretch(settingsDim.GetComponent<RectTransform>()); settingsDim.GetComponent<Image>().color = new Color(0, 0, 0, 0.58f);
            settings = new GameObject("PainelConfiguracoes", typeof(RectTransform), typeof(Image)); settings.transform.SetParent(root, false);
            var rect = settings.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = new Vector2(1560, 920);
            settings.GetComponent<Image>().color = Panel;
            Fill(rect, "FaixaSuperior", Red, 0, 0, 1560, 5);
            Label(rect, "CONFIGURAÇÕES", 46, White, 58, 34, 1000, 64).fontStyle = FontStyle.Bold;
            Label(rect, "SEU RITMO. SEU CONTROLE.", 17, Muted, 62, 104, 1000, 32);
            MenuButton(rect, "Fechar", "×", 1450, 34, 54, 54, false, CancelSettings);
            Fill(rect, "DivisoriaTopo", new Color(1, 1, 1, 0.14f), 60, 148, 1440, 1);
            Label(rect, "01  /  ÁUDIO", 20, Red, 60, 178, 440, 36).fontStyle = FontStyle.Bold;
            Fill(rect, "DivisoriaColunas", new Color(1, 1, 1, 0.10f), 560, 188, 1, 566);
            string[] volumeNames = { "Volume geral", "Música", "Efeitos e vozes" };
            for (int i = 0; i < 3; i++)
            {
                int index = i; float y = 244 + i * 102;
                Label(rect, volumeNames[i], 24, White, 60, y, 350, 38);
                volumeNumbers[i] = Label(rect, "100%", 20, Muted, 424, y, 78, 38); volumeNumbers[i].alignment = TextAnchor.MiddleRight;
                volumeSliders[i] = MakeSlider(rect, "Volume" + i, 60, y + 42, 440);
                volumeSliders[i].onValueChanged.AddListener(value => ChangeVolume(index, value));
            }
            MenuButton(rect, "Previa", "OUVIR PRÉVIA", 60, 590, 440, 56, false, PlayPreview);
            Label(rect, "Os volumes são testados na hora.\nAplique para guardar suas preferências.", 18, Muted, 60, 674, 445, 80);
            Label(rect, "02  /  CONTROLES", 20, Red, 620, 178, 840, 36).fontStyle = FontStyle.Bold;
            Label(rect, "AÇÃO", 15, Muted, 620, 234, 300, 26);
            Label(rect, "PRINCIPAL", 15, Muted, 950, 234, 240, 26);
            Label(rect, "ALTERNATIVA", 15, Muted, 1210, 234, 240, 26);
            for (int action = 0; action < 7; action++)
            {
                float y = 276 + action * 64;
                Label(rect, GameSettings.ActionNames[action], 20, White, 620, y, 316, 48);
                for (int slot = 0; slot < 2; slot++)
                {
                    int a = action, s = slot;
                    keyButtons[a, s] = MenuButton(rect, "Tecla_" + a + "_" + s, "", 950 + s * 260, y, 240, 48, false, () => BeginRebind((GameAction)a, s));
                }
            }
            keyHint = Label(rect, "Clique numa tecla para trocar. Esc cancela. Backspace limpa.", 17, Muted, 620, 744, 840, 42);
            Fill(rect, "DivisoriaRodape", new Color(1, 1, 1, 0.14f), 60, 806, 1440, 1);
            MenuButton(rect, "Restaurar", "RESTAURAR PADRÕES", 60, 836, 380, 54, false, RestoreDefaults);
            MenuButton(rect, "Cancelar", "CANCELAR", 870, 836, 240, 54, false, CancelSettings);
            applyButton = MenuButton(rect, "Aplicar", "APLICAR E VOLTAR", 1130, 836, 370, 54, true, ApplyAndClose);
            settings.SetActive(false); settingsDim.SetActive(false);
            EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
        }
        IEnumerator Reveal()
        {
            canvasGroup.alpha = 0f;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.55f) { canvasGroup.alpha = t; yield return null; }
            canvasGroup.alpha = 1f;
        }
        public void OpenSettings()
        {
            if (loading) return;
            before = GameSettings.Capture(); draft = before.Clone(); RefreshSettings();
            home.SetActive(false); settingsDim.SetActive(true); settings.SetActive(true);
            EventSystem.current.SetSelectedGameObject(volumeSliders[0].gameObject);
        }
        void RefreshSettings()
        {
            float[] volumes = { draft.Master, draft.Music, draft.Effects };
            for (int i = 0; i < 3; i++) { volumeSliders[i].SetValueWithoutNotify(volumes[i]); volumeNumbers[i].text = Mathf.RoundToInt(volumes[i] * 100) + "%"; }
            for (int a = 0; a < 7; a++) for (int s = 0; s < 2; s++)
                keyButtons[a, s].GetComponentInChildren<Text>().text = GameSettings.KeyLabel((s == 0 ? draft.Primary : draft.Secondary)[a]);
            keyHint.text = "Clique numa tecla para trocar. Esc cancela. Backspace limpa."; keyHint.color = Muted;
        }
        void ChangeVolume(int index, float value)
        {
            if (draft == null) return;
            if (index == 0) draft.Master = value; else if (index == 1) draft.Music = value; else draft.Effects = value;
            volumeNumbers[index].text = Mathf.RoundToInt(value * 100) + "%";
            GameSettings.PreviewAudio(draft.Master, draft.Music, draft.Effects);
        }
        public void RestoreDefaults()
        {
            EndCapture(); draft = new SettingsSnapshot(); RefreshSettings();
            GameSettings.PreviewAudio(draft.Master, draft.Music, draft.Effects);
            keyHint.text = "Padrões restaurados. Aplique para salvar.";
        }
        public void BeginRebind(GameAction action, int slot)
        {
            EndCapture(); captureAction = (int)action; captureSlot = slot; captureReadyAt = Time.unscaledTime + 0.2f;
            keyButtons[captureAction, slot].GetComponentInChildren<Text>().text = "PRESSIONE...";
            keyHint.text = "Pressione uma tecla ou botão do mouse. Esc cancela. Backspace limpa."; keyHint.color = White;
            EventSystem.current.SetSelectedGameObject(null);
            if (inputModule != null) inputModule.enabled = false;
        }
        public void CaptureKey(KeyCode key)
        {
            if (captureAction < 0) return;
            if (key == KeyCode.Escape) { EndCapture(); RefreshSettings(); return; }
            if (key == KeyCode.Backspace) key = KeyCode.None;
            if (!GameSettings.TryAssign(draft, (GameAction)captureAction, captureSlot, key, out string reason))
            { keyHint.text = reason; keyHint.color = Red; return; }
            EndCapture(); RefreshSettings(); keyHint.text = reason;
        }
        void EndCapture()
        {
            if (captureAction < 0) return;
            captureAction = -1;
            if (inputModule != null) inputModule.enabled = true;
        }
        public void ApplyAndClose()
        {
            EndCapture(); GameSettings.Apply(draft, true); CloseSettings(); homeStatus.text = "PREFERÊNCIAS SALVAS";
        }
        public void CancelSettings()
        {
            EndCapture(); if (before != null) GameSettings.Apply(before, false); CloseSettings();
        }
        void CloseSettings()
        {
            if (previewSource != null) previewSource.Stop();
            settings.SetActive(false); settingsDim.SetActive(false); home.SetActive(true);
            EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
        }
        public void PlayPreview()
        {
            if (previewClip == null) return;
            previewSource.Stop(); previewSource.clip = previewClip; ApplyPreviewVolume(); previewSource.Play();
        }
        public void StartNewGame()
        {
            if (loading) return;
            if (!Application.CanStreamedLevelBeLoaded(firstLevel)) { homeStatus.text = "A fase inicial não está disponível no build."; return; }
            loading = true; StartCoroutine(LoadGame());
        }
        IEnumerator LoadGame()
        {
            canvasGroup.interactable = false;
            var black = new GameObject("TransicaoNovoJogo", typeof(RectTransform), typeof(Image)); black.transform.SetParent(canvas.transform, false);
            var image = black.GetComponent<Image>(); Stretch(image.rectTransform); image.color = Color.clear;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.55f) { image.color = new Color(0, 0, 0, t); yield return null; }
            image.color = Color.black; Time.timeScale = 1f; AudioListener.pause = false;
            SceneManager.LoadScene(firstLevel);
        }
        Text Label(Transform parent, string content, int size, Color color, float x, float y, float width, float height)
        {
            var rect = At(parent, content.Length > 20 ? "Texto" : content, x, y, width, height);
            var text = rect.gameObject.AddComponent<Text>(); text.font = menuFont; text.fontSize = size; text.text = content; text.color = color;
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false; text.supportRichText = true;
            return text;
        }
        Image Fill(Transform parent, string name, Color color, float x, float y, float w, float h)
        {
            var image = At(parent, name, x, y, w, h).gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return image;
        }
        Button MenuButton(Transform parent, string name, string label, float x, float y, float width, float height, bool primary, UnityEngine.Events.UnityAction action, string number = null)
        {
            var rect = At(parent, name, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>(); image.color = Color.white;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.normalColor = primary ? Red : new Color(0.075f, 0.09f, 0.115f, 0.94f);
            colors.highlightedColor = primary ? new Color(0.96f, 0.22f, 0.29f) : new Color(0.20f, 0.22f, 0.26f, 0.99f);
            colors.selectedColor = colors.highlightedColor; colors.pressedColor = new Color(0.52f, 0.08f, 0.14f); colors.fadeDuration = 0.12f; button.colors = colors;
            var text = Label(rect, label, height >= 64 ? 27 : 18, White, number == null ? 18 : 76, 0, width - (number == null ? 36 : 128), height);
            text.fontStyle = FontStyle.Bold;
            if (number == null) text.alignment = TextAnchor.MiddleCenter;
            if (number != null)
            {
                Label(rect, number, 17, primary ? new Color(1, 1, 1, 0.7f) : Muted, 24, 0, 45, height);
                Label(rect, "›", 32, White, width - 46, 0, 32, height);
            }
            Fill(rect, "BordaInferior", primary ? new Color(1, 1, 1, 0.18f) : new Color(1, 1, 1, 0.12f), 0, height - 1, width, 1);
            button.onClick.AddListener(action); return button;
        }
        Slider MakeSlider(Transform parent, string name, float x, float y, float width)
        {
            var root = At(parent, name, x, y, width, 36);
            root.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.001f);
            var slider = root.gameObject.AddComponent<Slider>(); slider.minValue = 0; slider.maxValue = 1;
            Fill(root, "Trilho", new Color(1, 1, 1, 0.16f), 0, 15, width, 6);
            var area = At(root, "AreaPreenchimento", 0, 15, width, 6);
            var fill = At(area, "Preenchimento", 0, 0, width, 6); fill.gameObject.AddComponent<Image>().color = Red; Stretch(fill);
            var handleArea = At(root, "AreaControle", 0, 0, width, 36);
            var handle = At(handleArea, "Controle", 0, 3, 12, 30); var image = handle.gameObject.AddComponent<Image>(); image.color = White;
            handle.anchorMin = handle.anchorMax = handle.pivot = new Vector2(0.5f, 0.5f); handle.anchoredPosition = Vector2.zero;
            handle.sizeDelta = new Vector2(12f, -12f);
            slider.fillRect = fill; slider.handleRect = handle; slider.targetGraphic = image;
            var colors = slider.colors; colors.highlightedColor = White; colors.selectedColor = Red; slider.colors = colors;
            return slider;
        }
        static RectTransform At(Transform parent, string name, float x, float y, float width, float height)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height); return rect;
        }
        static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
    }
}
