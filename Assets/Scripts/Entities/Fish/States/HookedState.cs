using PixPlays.Fishing.Entities;
using PixPlays.Fishing.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.States
{
    public class HookedState : EntityState
    {
        private BaseFishController _fishController;
        private MovementController _movementController;
        private float _lerp = 0;
        public HookedState(BaseFishController fishController, MovementController movementController)
        {
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
            if (_fishController.Owner == null)
            {
                Debug.LogError("Bitten hook is null");
                return;
            }
            if (_fishController.Owner.Hook.HookPosition != _movementController.GetPosition())
            {
                _movementController.Teleport(Vector3.Lerp(_movementController.GetPosition(), _fishController.Owner.Hook.HookPosition, _lerp));
                _lerp += deltaTime * 2;
                return;
            }
            _movementController.Teleport(_fishController.Owner.Hook.HookPosition);
        }
    }
}