using System;
using UnityEngine;

namespace UI
{
    public static class InputManager
    {
        public static InputMode Mode { get; private set; }
            = InputMode.Game;

        public static event Action<InputMode> OnInputModeChanged;

        public static void SetMode(InputMode mode)
        {
            if (Mode == mode)
                return;

            Mode = mode;

            Cursor.lockState =
                mode == InputMode.Game
                    ? CursorLockMode.Locked
                    : CursorLockMode.None;

            Cursor.visible =
                mode == InputMode.UI;

            OnInputModeChanged?.Invoke(mode);
        }

        public static bool IsGameMode()
        {
            return Mode == InputMode.Game;
        }
    }
}