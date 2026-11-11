using System;
using UnityEngine;

namespace Redline.Settings
{
    public enum GameAction { Left, Right, Up, Down, Jump, Shoot, Drop }

    public sealed class SettingsSnapshot
    {
        public float Master = 1f, Music = 1f, Effects = 1f;
        public KeyCode[] Primary = { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.Space, KeyCode.Mouse0, KeyCode.C };
        public KeyCode[] Secondary = { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.None, KeyCode.LeftControl, KeyCode.None };
        public SettingsSnapshot Clone() => new SettingsSnapshot {
            Master = Master, Music = Music, Effects = Effects,
            Primary = (KeyCode[])Primary.Clone(), Secondary = (KeyCode[])Secondary.Clone()
        };
    }

    /// <summary>Preferências locais persistidas separadamente do progresso da campanha.</summary>
    public static class GameSettings
    {
        const string Prefix = "Redline.Settings.";
        static SettingsSnapshot current;
        public static event Action Changed;
        public static float MusicVolume { get { EnsureLoaded(); return current.Music; } }
        public static float EffectsVolume { get { EnsureLoaded(); return current.Effects; } }
        public static readonly string[] ActionNames = { "Mover para a esquerda", "Mover para a direita", "Mirar para cima", "Mirar para baixo", "Pular", "Atirar", "Descer da plataforma" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize() { current = null; EnsureLoaded(); AudioListener.pause = false; }

        static float ReadVolume(string key)
        {
            float value = PlayerPrefs.GetFloat(Prefix + key, 1f);
            return float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Clamp01(value);
        }
        static void EnsureLoaded()
        {
            if (current != null) return;
            current = new SettingsSnapshot { Master = ReadVolume("Master"), Music = ReadVolume("Music"), Effects = ReadVolume("Effects") };
            var saved = current.Clone();
            bool valid = true;
            var used = new System.Collections.Generic.HashSet<KeyCode>();
            for (int i = 0; i < saved.Primary.Length; i++)
            {
                for (int slot = 0; slot < 2; slot++)
                {
                    var keys = slot == 0 ? saved.Primary : saved.Secondary;
                    keys[i] = (KeyCode)PlayerPrefs.GetInt(Prefix + "Key." + i + "." + slot, (int)keys[i]);
                    var key = keys[i];
                    if (!Enum.IsDefined(typeof(KeyCode), key) || key == KeyCode.Escape || key == KeyCode.Backspace
                        || (int)key >= (int)KeyCode.JoystickButton0 || (key != KeyCode.None && !used.Add(key))) valid = false;
                }
                if (saved.Primary[i] == KeyCode.None && saved.Secondary[i] == KeyCode.None) valid = false;
            }
            if (valid) { current.Primary = saved.Primary; current.Secondary = saved.Secondary; }
            AudioListener.volume = current.Master;
        }
        public static SettingsSnapshot Capture() { EnsureLoaded(); return current.Clone(); }
        public static KeyCode GetKey(GameAction action, int slot)
        {
            EnsureLoaded(); return (slot == 0 ? current.Primary : current.Secondary)[(int)action];
        }
        public static void PreviewAudio(float master, float music, float effects)
        {
            EnsureLoaded(); current.Master = Mathf.Clamp01(master); current.Music = Mathf.Clamp01(music); current.Effects = Mathf.Clamp01(effects);
            AudioListener.volume = current.Master; Changed?.Invoke();
        }
        public static void Apply(SettingsSnapshot snapshot, bool save)
        {
            current = snapshot.Clone();
            PreviewAudio(current.Master, current.Music, current.Effects);
            if (!save) return;
            PlayerPrefs.SetFloat(Prefix + "Master", current.Master);
            PlayerPrefs.SetFloat(Prefix + "Music", current.Music);
            PlayerPrefs.SetFloat(Prefix + "Effects", current.Effects);
            for (int i = 0; i < current.Primary.Length; i++)
            {
                PlayerPrefs.SetInt(Prefix + "Key." + i + ".0", (int)current.Primary[i]);
                PlayerPrefs.SetInt(Prefix + "Key." + i + ".1", (int)current.Secondary[i]);
            }
            PlayerPrefs.Save();
        }
        public static bool TryAssign(SettingsSnapshot draft, GameAction action, int slot, KeyCode key, out string reason)
        {
            int index = (int)action;
            if (key == KeyCode.Escape || key == KeyCode.Backspace || (int)key >= (int)KeyCode.JoystickButton0)
            { reason = "Escolha outra tecla. Esc cancela e Backspace limpa."; return false; }
            var own = slot == 0 ? draft.Primary : draft.Secondary;
            var other = slot == 0 ? draft.Secondary : draft.Primary;
            if (key == KeyCode.None && other[index] == KeyCode.None)
            { reason = "Esta ação precisa de pelo menos uma tecla."; return false; }
            if (key != KeyCode.None)
                for (int i = 0; i < draft.Primary.Length; i++)
                    for (int s = 0; s < 2; s++)
                        if (!(i == index && s == slot) && (s == 0 ? draft.Primary[i] : draft.Secondary[i]) == key)
                        { reason = "Essa tecla já está em “" + ActionNames[i] + "”."; return false; }
            own[index] = key; reason = "Tecla atualizada. Aplique para salvar."; return true;
        }
        public static string KeyLabel(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.None: return "—";
                case KeyCode.Space: return "ESPAÇO";
                case KeyCode.LeftArrow: return "SETA ESQ.";
                case KeyCode.RightArrow: return "SETA DIR.";
                case KeyCode.UpArrow: return "SETA CIMA";
                case KeyCode.DownArrow: return "SETA BAIXO";
                case KeyCode.LeftControl: return "CTRL ESQ.";
                case KeyCode.RightControl: return "CTRL DIR.";
                case KeyCode.LeftShift: return "SHIFT ESQ.";
                case KeyCode.RightShift: return "SHIFT DIR.";
                case KeyCode.LeftAlt: return "ALT ESQ.";
                case KeyCode.RightAlt: return "ALT DIR.";
                case KeyCode.Mouse0: return "MOUSE 1";
                case KeyCode.Mouse1: return "MOUSE 2";
                case KeyCode.Mouse2: return "MOUSE 3";
                case KeyCode.Return: return "ENTER";
                case KeyCode.KeypadEnter: return "ENTER NUM.";
                default: return key.ToString().ToUpperInvariant();
            }
        }
    }
}
