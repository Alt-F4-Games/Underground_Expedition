using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class PlayerAreaGate : NetworkBehaviour
{
    [Header("Scene")]
    [SerializeField] private string sceneName;

    [Header("Input")]
    [SerializeField] private KeyCode triggerKey = KeyCode.E;

    private HashSet<PlayerRef> _playersInside = new();

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // ============================================================
    // TRIGGERS (solo server)
    // ============================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        var no = other.GetComponentInParent<NetworkObject>();
        if (no == null) return;

        _playersInside.Add(no.InputAuthority);

        Debug.Log($"[GATE] Enter ({_playersInside.Count}/{Runner.ActivePlayers.Count()})");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        var no = other.GetComponentInParent<NetworkObject>();
        if (no == null) return;

        _playersInside.Remove(no.InputAuthority);

        Debug.Log($"[GATE] Exit ({_playersInside.Count}/{Runner.ActivePlayers.Count()})");
    }

    // ============================================================
    // INPUT (solo host local)
    // ============================================================

    private void Update()
    {
        if (Runner == null || !Runner.IsRunning) return;

        if (Runner.IsServer && Input.GetKeyDown(triggerKey))
        {
            RPC_RequestSceneChange();
        }
    }

    // ============================================================
    // RPC (VALIDACIÓN EN SERVER)
    // ============================================================

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestSceneChange()
    {
        int totalPlayers = Runner.ActivePlayers.Count();

        if (_playersInside.Count < totalPlayers)
        {
            Debug.Log($"[GATE] Not all players inside ({_playersInside.Count}/{totalPlayers})");
            return;
        }

        Debug.Log("[GATE] All players ready → changing scene");

        Runner.LoadScene(sceneName);
    }
}