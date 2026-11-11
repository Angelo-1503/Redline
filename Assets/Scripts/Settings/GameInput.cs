using UnityEngine;

namespace Redline.Settings
{
    public static class GameInput
    {
        public static bool Held(GameAction action)
        {
            var a = GameSettings.GetKey(action, 0); var b = GameSettings.GetKey(action, 1);
            return (a != KeyCode.None && Input.GetKey(a)) || (b != KeyCode.None && Input.GetKey(b));
        }
        public static bool Down(GameAction action)
        {
            var a = GameSettings.GetKey(action, 0); var b = GameSettings.GetKey(action, 1);
            return (a != KeyCode.None && Input.GetKeyDown(a)) || (b != KeyCode.None && Input.GetKeyDown(b));
        }
        public static float Horizontal => (Held(GameAction.Right) ? 1f : 0f) - (Held(GameAction.Left) ? 1f : 0f);
        public static float Vertical => (Held(GameAction.Up) ? 1f : 0f) - (Held(GameAction.Down) ? 1f : 0f);
    }
}
