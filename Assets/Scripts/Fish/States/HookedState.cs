using PixPlays.Fishing.Fish;
using PixPlays.Fishing.Movement;
using PixPlays.Fishing.States;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Fish
{
    public class HookedState : EntityState
    {
        private BaseFishController _fishController;
        private MovementController _movementController;
        private float _lerp = 0;
        private Action _onCancel;
        public HookedState(BaseFishController fishController, MovementController movementController,Action onCancel)
        {
            _onCancel = onCancel;
            _fishController = fishController;
            _movementController = movementController;
        }

        protected override void OnActivate()
        {
            _lerp = 0;
        }

        protected override void OnDeactivate()
        {
        }

        protected override void Update(float deltaTime)
        {
            if (_fishController.PlayerOwner == null)
            {
                Debug.LogError("Bitten hook is null");
                _onCancel?.Invoke();
                Deactivate();
                return;
            }
            if (_fishController.PlayerOwner.Hook.HookPosition != _movementController.GetPosition())
            {
                _movementController.Teleport(Vector3.Lerp(_movementController.GetPosition(), _fishController.PlayerOwner.Hook.HookPosition, _lerp));
                _lerp += deltaTime * 2;
                return;
            }
            _movementController.Teleport(_fishController.PlayerOwner.Hook.HookPosition);
        }
    }
}