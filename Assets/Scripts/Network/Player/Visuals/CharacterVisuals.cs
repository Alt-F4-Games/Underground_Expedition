using Fusion;
using UnityEngine;

public class CharacterVisuals : NetworkBehaviour
{
    [SerializeField] private GameObject firstPersonMesh;
    [SerializeField] private GameObject thirdPersonMesh;

    public override void Spawned()
    {
        bool isLocalPlayer = HasInputAuthority;

        firstPersonMesh.SetActive(isLocalPlayer);
        thirdPersonMesh.SetActive(!isLocalPlayer);
    }
}