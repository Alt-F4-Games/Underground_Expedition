namespace Network.Environment.ToggleableObjects
{
    public interface IToggleable
    {
        void SetState(bool state);
        void Toggle();
        bool GetState();
    }
}