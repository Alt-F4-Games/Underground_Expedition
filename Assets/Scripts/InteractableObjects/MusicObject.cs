using UnityEngine;

public class MusicObject : MonoBehaviour, IInteractable
{
    [Header("Sound ID")]
    [SerializeField] private string musicID;

    public void Interact(PlayerInteraction player)
    {
        Debug.Log($"Reproduciendo música: {musicID}");
        SoundManager.Instance.Play(musicID);
    }

    public void Release()
    {
        // SoundManager.Instance.Stop(musicID);
    }
}