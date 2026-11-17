using UnityEngine;
using UnityEngine.InputSystem;

namespace Redline.Settings
{
    /// <summary>Controles reconhecidos pelo Input System: botões por posição, sem índices de fabricante.</summary>
    public static class ControllerInput
    {
        const float Deadzone = 0.25f;
        static Gamepad Pad => Gamepad.current != null && Gamepad.current.added && Gamepad.current.enabled ? Gamepad.current : null;
        public static bool Connected => Pad != null;
        static float Axis(float value) => Mathf.Abs(value) >= Deadzone ? value : 0f;
        public static Vector2 Move
        {
            get
            {
                var pad = Pad; if (pad == null) return Vector2.zero;
                Vector2 dpad = pad.dpad.ReadValue();
                if (dpad.sqrMagnitude > 0.1f)
                    return new Vector2(Mathf.Abs(dpad.x) > 0.1f ? Mathf.Sign(dpad.x) : 0f, Mathf.Abs(dpad.y) > 0.1f ? Mathf.Sign(dpad.y) : 0f);
                Vector2 stick = pad.leftStick.ReadValue();
                return new Vector2(Axis(stick.x), Axis(stick.y));
            }
        }
        public static bool TryAim(out Vector2 direction)
        {
            var pad = Pad; direction = Vector2.zero;
            if (pad == null) return false;
            Vector2 stick = pad.rightStick.ReadValue();
            if (stick.magnitude < 0.4f) return false;
            float angle = Mathf.Round(Mathf.Atan2(stick.y, stick.x) / (Mathf.PI / 4f)) * (Mathf.PI / 4f);
            direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            return true;
        }
        public static bool Held(GameAction action)
        {
            var pad = Pad; if (pad == null) return false;
            switch (action)
            {
                case GameAction.Left: return Move.x < -0.5f;
                case GameAction.Right: return Move.x > 0.5f;
                case GameAction.Up: return Move.y > 0.5f;
                case GameAction.Down: return Move.y < -0.5f;
                case GameAction.Jump: return pad.buttonSouth.isPressed;
                case GameAction.Shoot: return pad.rightTrigger.isPressed || pad.buttonWest.isPressed;
                case GameAction.Drop: return pad.buttonEast.isPressed;
                default: return false;
            }
        }
        public static bool Down(GameAction action)
        {
            var pad = Pad; if (pad == null) return false;
            switch (action)
            {
                case GameAction.Left: return pad.dpad.left.wasPressedThisFrame || pad.leftStick.left.wasPressedThisFrame;
                case GameAction.Right: return pad.dpad.right.wasPressedThisFrame || pad.leftStick.right.wasPressedThisFrame;
                case GameAction.Up: return pad.dpad.up.wasPressedThisFrame || pad.leftStick.up.wasPressedThisFrame;
                case GameAction.Down: return pad.dpad.down.wasPressedThisFrame || pad.leftStick.down.wasPressedThisFrame;
                case GameAction.Jump: return pad.buttonSouth.wasPressedThisFrame;
                case GameAction.Shoot: return pad.rightTrigger.wasPressedThisFrame || pad.buttonWest.wasPressedThisFrame;
                case GameAction.Drop: return pad.buttonEast.wasPressedThisFrame;
                default: return false;
            }
        }
        public static bool SubmitPressed => Pad != null && Pad.buttonSouth.wasPressedThisFrame;
        public static bool BackPressed => Pad != null && Pad.buttonEast.wasPressedThisFrame;
        public static bool PausePressed => Pad != null && Pad.startButton.wasPressedThisFrame;
        public static float Scroll
        {
            get
            {
                var pad = Pad; if (pad == null) return 0;
                float right = Axis(pad.rightStick.ReadValue().y);
                return Mathf.Abs(right) > 0 ? right : Move.y;
            }
        }
    }
}
