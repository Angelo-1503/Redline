using UnityEngine;

namespace Redline.Settings
{
    public static class GameInput
    {
        static bool KeyboardHeld(GameAction action)
        {
            var a = GameSettings.GetKey(action, 0); var b = GameSettings.GetKey(action, 1);
            return (a != KeyCode.None && Input.GetKey(a)) || (b != KeyCode.None && Input.GetKey(b));
        }
        public static bool Held(GameAction action)
        {
            if (Redline.UI.PauseMenu.BlocksGameplayInput) return false;
            return KeyboardHeld(action) || ControllerInput.Held(action);
        }
        public static bool Down(GameAction action)
        {
            if (Redline.UI.PauseMenu.BlocksGameplayInput) return false;
            var a = GameSettings.GetKey(action, 0); var b = GameSettings.GetKey(action, 1);
            return (a != KeyCode.None && Input.GetKeyDown(a)) || (b != KeyCode.None && Input.GetKeyDown(b)) || ControllerInput.Down(action);
        }
        public static float Horizontal
        {
            get
            {
                if (Redline.UI.PauseMenu.BlocksGameplayInput) return 0;
                float keyboard = (KeyboardHeld(GameAction.Right) ? 1 : 0) - (KeyboardHeld(GameAction.Left) ? 1 : 0);
                return keyboard != 0 ? keyboard : ControllerInput.Move.x;
            }
        }
        public static float Vertical
        {
            get
            {
                if (Redline.UI.PauseMenu.BlocksGameplayInput) return 0;
                float keyboard = (KeyboardHeld(GameAction.Up) ? 1 : 0) - (KeyboardHeld(GameAction.Down) ? 1 : 0);
                return keyboard != 0 ? keyboard : ControllerInput.Move.y;
            }
        }
        public static bool TryControllerAim(out Vector2 direction)
        {
            direction = Vector2.zero;
            return !Redline.UI.PauseMenu.BlocksGameplayInput && ControllerInput.TryAim(out direction);
        }
        public static bool MenuBackPressed => Input.GetKeyDown(KeyCode.Escape) || ControllerInput.BackPressed;
        public static bool PausePressed => Input.GetKeyDown(KeyCode.Escape) || ControllerInput.PausePressed;
    }
}
