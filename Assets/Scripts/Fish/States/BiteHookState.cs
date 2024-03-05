using PixPlays.Fishing.Fish;
using PixPlays.Fishing.Movement;
using PixPlays.Fishing.States;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Fish
{
    public class BiteHookState : EntityState
    {
        private MovementController _movementController;
        private BaseFishController _fishController;
        private Action _onFinish;
        private bool _hookBitten;
        public BiteHookState(MovementController movementController, BaseFishController fishController,Action onFinish)
        {
            _hookBitten = false;
            _fishController = fishController;
            _movementController = movementController;
            _onFinish= onFinish;
        }

        private void _movementController_OnDestinationReached(Vector3 arg1, Vector3 arg2)
        {
            _hookBitten = true;
            _onFinish?.Invoke();
        }

        protected override void OnActivate()
        {
            _hookBitten = false;
            _movementController.OnDestinationReached += _movementController_OnDestinationReached;
        }

        protected override void OnDeactivate()
        {
            _movementController.OnDestinationReached -= _movementController_OnDestinationReached;
        }

        protected override void Update(float deltaTime)
        {
            if (!_hookBitten)
            {
                if (_fishController.PlayerOwner == null)
                {
                    Debug.LogError("Bitten hook is null");
                    return;
                }
                _movementController.MoveTo(_fishController.PlayerOwner.Hook.HookPosition);
            }
        }
    }
}