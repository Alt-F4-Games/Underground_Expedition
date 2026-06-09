namespace Audio
{
    public enum AudioEvent
    {
        None = 0,

        // UI
        ButtonClick,
        MenuOpen,
        MenuClose,

        // Combat
        GunShot,
        Explosion,
        EnemyDeath,

        // Character
        Footstep,
        Jump,
        Land,

        // Environment
        DoorOpen,
        DoorClose
    }
}