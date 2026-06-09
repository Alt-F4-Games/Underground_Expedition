using FMODUnity;

namespace Audio
{
    [System.Serializable]
    public class AudioEntry
    {
        public AudioEvent audioEvent;

        [EventRef]
        public EventReference eventReference;
    }
}