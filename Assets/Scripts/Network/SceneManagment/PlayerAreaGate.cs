using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class PlayerAreaGate : NetworkBehaviour
{
    [SerializeField] private string sceneName;

    private HashSet<PlayerRef> _playersInside = new();

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        var no = other.GetComponentInParent<NetworkObject>();
        if (no == null) return;

        _playersInside.Add(no.InputAuthority);

        CheckAllPlayers();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        var no = other.GetComponentInParent<NetworkObject>();
        if (no == null) return;

        _playersInside.Remove(no.InputAuthority);
    }

    private void CheckAllPlayers()
    {
        int totalPlayers = Runner.ActivePlayers.Count();

        if (_playersInside.Count >= totalPlayers)
        {
            Debug.Log("[GATE] All players inside → changing scene");

            Runner.LoadScene(sceneName);
        }
    }
}