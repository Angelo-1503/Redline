using UnityEngine;

namespace Redline.Settings
{
    public enum Difficulty { Easy, Normal, Hard }

    /// <summary>Escolha da campanha atual; preservada entre fases e reinícios.</summary>
    public static class GameDifficulty
    {
        public static Difficulty Current { get; private set; } = Difficulty.Normal;
        public static string Label => Name(Current);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize() { Current = Difficulty.Normal; }

        public static void BeginCampaign(Difficulty difficulty)
        {
            Current = difficulty == Difficulty.Easy || difficulty == Difficulty.Hard ? difficulty : Difficulty.Normal;
        }
        public static string Name(Difficulty difficulty)
        {
            return difficulty == Difficulty.Easy ? "FÁCIL" : difficulty == Difficulty.Hard ? "DIFÍCIL" : "NORMAL";
        }
        public static int ScaleDamage(int amount, bool targetIsPlayer, bool targetIsEnemy)
        {
            if (amount <= 0 || Current == Difficulty.Normal || (!targetIsPlayer && !targetIsEnemy)) return amount;
            float multiplier = targetIsPlayer
                ? (Current == Difficulty.Easy ? 0.65f : 1.35f)
                : (Current == Difficulty.Easy ? 1.25f : 0.85f);
            return Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));
        }
    }
}
