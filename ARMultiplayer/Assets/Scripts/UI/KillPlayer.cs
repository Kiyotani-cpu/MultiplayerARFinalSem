using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class KillPlayer : MonoBehaviour
{
    [SerializeField] private Button killPlayerButton;
    public static event Action<ulong> OnKillPlayer;

    void Start()
    {
        killPlayerButton.onClick.AddListener(OnKillPlayerButtonClicked);
    }

    private void OnKillPlayerButtonClicked()
    {
        // Ensure that the player ID is valid and the action is taken only on the server
        if (NetworkManager.Singleton.IsServer)
        {
            OnKillPlayer?.Invoke(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            Debug.LogWarning("Only the server can initiate a player kill.");
        }
    }
}
