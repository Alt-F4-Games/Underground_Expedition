using System;
using UnityEngine;

namespace UI
{
    public enum InputMode
    {
        Game,
        UI
    }

    public static class InputManager
    {
        public static InputMode Mode { get; private set; } = InputMode.Game;

        public static bool IsGameMode => Mode == InputMode.Game;

        public static bool IsBlocked => _uiBlockCount > 0;

        public static event Action<InputMode> OnInputModeChanged;

        private static int _uiBlockCount;

        public static void PushUI()
        {
            _uiBlockCount++;

            if (Mode != InputMode.UI)
            {
                SetModeInternal(InputMode.UI);
            }
        }

        public static void PopUI()
        {
            _uiBlockCount--;

            if (_uiBlockCount < 0)
                _uiBlockCount = 0;

            if (_uiBlockCount == 0)
            {
                SetModeInternal(InputMode.Game);
            }
        }

        public static void ForceGameMode()
        {
            _uiBlockCount = 0;
            SetModeInternal(InputMode.Game);
        }

        public static void ForceUIMode()
        {
            _uiBlockCount = Mathf.Max(1, _uiBlockCount);
            SetModeInternal(InputMode.UI);
        }

        private static void SetModeInternal(InputMode mode)
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
    }
}