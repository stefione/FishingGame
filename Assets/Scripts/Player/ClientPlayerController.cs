using PixPlays.Fishing.Entities;
using PixPlays.Fishing.GameManagement;
using PixPlays.Fishing.Hook;
using PixPlays.Fishing.World;
using PixPlays.Framework.Events;
using PixPlays.Framework.Network;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.Player
{
    public class TryCatchFishEvent
    {
        public ulong PlayerID;
    }
    public class PlayerControllerSpawnedEvent
    {
        public ClientPlayerController Controller;
    }
    public class ThrowHookEvent
    {
        public ulong PlayerID;
    }

    public class ClientPlayerController : BasePlayerController
    {
        private ulong _playerClientId;
        public HookController Hook;
        private BaseFishController _caughtFish;
        private bool _canThrow=true;
        private void Awake()
        {
            EventManager.Subscribe<LiftHookEvent>(x => ProcessLiftHookEvent(x));
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            EventManager.Fire(new PlayerControllerSpawnedEvent()
            {
                Controller = this
            });
        }
        private void ProcessLiftHookEvent(LiftHookEvent x)
        {
            if (x.Owner == this) 
            {
                LiftHook(x.Fish);
            }
        }

        public void SetData(ulong playerClientId ,PlayerData playerData, WaterArea waterArea)
        {
            _playerClientId = playerClientId;
            _playerData = playerData;
            Hook.SetData(waterArea);
            Hook.OnHookReachedMaxDepth += _hook_OnHookReachedMaxDepth;
            Hook.OnHookLifted += _hook_OnHookLifted;
        }

        private void _hook_OnHookLifted()
        {
            if (_caughtFish != null)
            {
                _caughtFish.ResetFish();
            }
            _canThrow = true;
        }

        private void _hook_OnHookReachedMaxDepth()
        {
            EventManager.Fire(new TryCatchFishEvent()
            {
                PlayerID = _playerClientId
            });
        }

        public void Update()
        {
            if (IsOwner && _canThrow && Input.GetKeyDown(KeyCode.Space))
            {
                EventManager.Fire(new ThrowHookEvent() { PlayerID = OwnerClientId });
            }
        }

        public void ThrowHook()
        {
            Hook.ThrowHook();
            _canThrow = false;
        }

        internal void LiftHook(BaseFishController fish)
        {
            _caughtFish= fish;
            Hook.LiftHook();
        }
    }
}