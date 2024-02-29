using PixPlays.Fishing.Entities;
using PixPlays.Fishing.Hook;
using PixPlays.Fishing.World;
using PixPlays.Framework.Events;
using System;
using System.Collections;
using UnityEngine;
namespace PixPlays.Fishing.Player
{
    public class TryCatchFishEvent
    {
        public string PlayerID;
    }

    public class ClientPlayerController : BasePlayerController
    {
        public HookController Hook;
        private FishController _caughtFish;
        private bool _canThrow=true;
        private void Awake()
        {
            EventManager.Subscribe<LiftHookEvent>(x => ProcessLiftHookEvent(x));
        }

        private void ProcessLiftHookEvent(LiftHookEvent x)
        {
            if (x.Owner == this) 
            {
                LiftHook(x.Fish);
            }
        }

        public void SetData(PlayerData playerData, WaterArea waterArea)
        {
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
                PlayerID = _playerData.Id
            });
        }

        public void Update()
        {
            if (_canThrow && Input.GetKeyDown(KeyCode.Space))
            {
                ThrowHook();
            }
        }

        private void ThrowHook()
        {
            Hook.ThrowHook();
            _canThrow = false;
        }

        internal void LiftHook(FishController fish)
        {
            _caughtFish= fish;
            Hook.LiftHook();
        }
    }
}