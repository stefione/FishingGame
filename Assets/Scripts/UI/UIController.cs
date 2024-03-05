using Assets.HeroEditor.Common.Scripts.Common;
using PixPlays.Fishing.GameManagement;
using PixPlays.Fishing.Player;
using PixPlays.Framework.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixPlays.Fishing.UI
{
    public class PlayerDataUpdated
    {
        public ulong ClientId;
        public PlayerData playerData;
    }

    public class UIController : MonoBehaviour
    {
        [SerializeField] List<PlayerDataUIDisplay> _playerDisplays;
        private Dictionary<ulong, PlayerDataUIDisplay> _displays=new();

        void Awake()
        {
            foreach (var i in _playerDisplays)
            {
                i.gameObject.SetActive(false);
            }
            EventManager.Subscribe<PlayerDataUpdated>(x => ProcessPlayerDataUpdated(x));
            EventManager.Subscribe<PlayerRegisteredEvent>(x => ProcessPlayerRegisteredEvent(x));
            EventManager.Subscribe<PlayerDisconnectedEvent>(x => ProcessPlayerDisconnectedEvent(x));
        }

        private void ProcessPlayerDisconnectedEvent(PlayerDisconnectedEvent x)
        {
            if(_displays.TryGetValue(x.ClientId,out var display))
            {
                display.gameObject.SetActive(false);
            }
            _displays.Remove(x.ClientId);
        }

        private void ProcessPlayerRegisteredEvent(PlayerRegisteredEvent x)
        {
            if (_playerDisplays.Count<=x.SpawnPointIndex)
            {
                Debug.LogError("Not enought displays for characters");
                return;
            }
            PlayerDataUIDisplay display= _playerDisplays[x.SpawnPointIndex];
            _displays.Add(x.ClientId, display);
            display.gameObject.SetActive(true);
            display.UpdateData(x.PlayerData);
        }

        private void ProcessPlayerDataUpdated(PlayerDataUpdated x)
        {
            if(_displays.TryGetValue(x.ClientId,out var display))
            {
                display.UpdateData(x.playerData);
            }
        }
    }
}
