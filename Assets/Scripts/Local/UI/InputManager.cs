using System;
namespace UI
{
    public class InputManager
    {
        public static InputMode Mode { get; private set; } = InputMode.Game;

        public static event Action<InputMode> OnInputModeChanged;

        public static void SetMode(InputMode newMode)
        {
            if (Mode == newMode) return;

            Mode = newMode;
            OnInputModeChanged?.Invoke(newMode);
        }

        public static bool IsGameMode()
        {
            return Mode == InputMode.Game;
        }
    }
}