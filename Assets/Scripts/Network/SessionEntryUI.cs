using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class SessionEntryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _roomNameText;
        [SerializeField] private TMP_Text _playerCountText;
        [SerializeField] private Button _joinButton;

        /// <summary>
        /// Visually configures the entry with session data and assigns the join action.
        /// </summary>
        public void Setup(SessionInfo session, Action onJoinSelected)
        {
            // Set the display name of the room
            _roomNameText.text = session.Name;
            
            // Display current player count vs capacity
            _playerCountText.text = $"{session.PlayerCount} / {session.MaxPlayers}";
            
            // Clear existing listeners to prevent multiple triggers and assign the join callback
            _joinButton.onClick.RemoveAllListeners();
            _joinButton.onClick.AddListener(() => onJoinSelected());
            
            // Disable interaction if the room is at full capacity
            if (session.PlayerCount >= session.MaxPlayers)
            {
                _joinButton.interactable = false;
                _playerCountText.color = Color.red;
            }
        }
    }
}