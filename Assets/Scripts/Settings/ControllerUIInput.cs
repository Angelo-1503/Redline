using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Redline.Settings
{
    /// <summary>Unifica teclado, mouse e Gamepad no EventSystem existente, sem processar joystick duas vezes.</summary>
    public sealed class ControllerUIInput : BaseInput
    {
        StandaloneInputModule module;
        public static void Ensure(EventSystem system)
        {
            if (system == null) return;
            var module = system.GetComponent<StandaloneInputModule>();
            if (module == null) module = system.gameObject.AddComponent<StandaloneInputModule>();
            var bridge = system.GetComponent<ControllerUIInput>();
            if (bridge == null) bridge = system.gameObject.AddComponent<ControllerUIInput>();
            bridge.module = module; module.inputOverride = bridge;
        }
        public override float GetAxisRaw(string axisName)
        {
            if (axisName == module.horizontalAxis)
            {
                float keyboard = (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) ? 1 : 0)
                    - (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) ? 1 : 0);
                return keyboard != 0 ? keyboard : ControllerInput.Move.x;
            }
            if (axisName == module.verticalAxis)
            {
                float keyboard = (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) ? 1 : 0)
                    - (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) ? 1 : 0);
                return keyboard != 0 ? keyboard : ControllerInput.Move.y;
            }
            return base.GetAxisRaw(axisName);
        }
        public override bool GetButtonDown(string buttonName)
        {
            if (buttonName == module.horizontalAxis)
                return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)
                    || ControllerInput.Down(GameAction.Left) || ControllerInput.Down(GameAction.Right);
            if (buttonName == module.verticalAxis)
                return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)
                    || ControllerInput.Down(GameAction.Up) || ControllerInput.Down(GameAction.Down);
            if (buttonName == module.submitButton)
                return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || ControllerInput.SubmitPressed;
            if (buttonName == module.cancelButton) return GameInput.MenuBackPressed;
            return base.GetButtonDown(buttonName);
        }
        void Update()
        {
            if (module == null || !module.isActiveAndEnabled || !ControllerInput.Connected) return;
            var system = GetComponent<EventSystem>();
            if (system.currentSelectedGameObject != null || (ControllerInput.Move.sqrMagnitude < 0.1f && !ControllerInput.SubmitPressed)) return;
            // A click on empty space must not strand subsequent controller navigation.
            foreach (var selectable in Selectable.allSelectablesArray)
                if (selectable != null && selectable.isActiveAndEnabled && selectable.IsInteractable())
                { system.SetSelectedGameObject(selectable.gameObject); break; }
        }
    }
}
