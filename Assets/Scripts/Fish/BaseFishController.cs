using PixPlays.Fishing.GameManagement;
using PixPlays.Fishing.Movement;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.States;
using PixPlays.Fishing.World;
using PixPlays.Framework.Events;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.Fish
{
    public abstract class BaseFishController : MonoBehaviour
    {
        public ulong Id { get; private set; }
        [HideInInspector] public BasePlayerController PlayerOwner;
        public FishData FishData;
        public bool IsOwned => PlayerOwner != null;

        protected Dictionary<FishStates, EntityState> _states = new();
        protected float _originalMovementSpeed;
        protected MovementController _movementController;
        protected WaterArea _waterArea;

        private bool _isInitalized;

        public virtual void SetData(ulong id,FishTemplate fishTemplate,FishData fishData, WaterArea waterArea, Vector3 startLocation)
        {
            if (_isInitalized)
            {
                return;
            }
            Id= id;
            FishData = fishData;
            _waterArea = waterArea;
            _isInitalized = true;
        }

        public bool SetState(FishStates stateType)
        {
            if (_states.TryGetValue(stateType, out var state))
            {
                foreach (var i in _states)
                {
                    i.Value.Deactivate();
                }
                state.Activate();
                return true;
            }
            return false;
        }

        protected abstract void OnReturnToWater();

        protected void OnBiteHookFinish()
        {
            SetState(FishStates.HookedState);
            EventManager.Fire(new LiftHookEvent()
            {
                Success=true,
                FishId = Id,
                OwnerId = PlayerOwner.Id
            });
        }

        public void BiteHook(BasePlayerController playerController)
        {
            PlayerOwner = playerController;
            foreach (var i in _states)
            {
                i.Value.Deactivate();
            }
            _states[FishStates.BiteHook].Activate();
        }

        public abstract void ResetFish();
    }
}