using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public override void Spawned()
    {
        DontDestroyOnLoad(gameObject);
    }
}