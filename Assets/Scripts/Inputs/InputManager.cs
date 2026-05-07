using System;
using UnityEngine;

namespace UI
{
    public static class InputManager
    {
        public static InputMode Mode { get; private set; } = InputMode.Game;

        public static event Action<InputMode> OnInputModeChanged;

        public static void SetMode(InputMode newMode)
        {
            if (Mode == newMode)
                return;

            Mode = newMode;

            ApplyCursorState(newMode);

            OnInputModeChanged?.Invoke(newMode);
        }

        public static bool IsGameMode()
        {
            return Mode == InputMode.Game;
        }

        private static void ApplyCursorState(InputMode mode)
        {
            bool isUI = mode == InputMode.UI;

            Cursor.lockState = isUI
                ? CursorLockMode.None
                : CursorLockMode.Locked;

            Cursor.visible = isUI;
        }
    }
}