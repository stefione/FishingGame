using PixPlays.Fishing.Movement;
using PixPlays.Fishing.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.States
{
    public class OutOfWaterState : EntityState
    {
        private MovementController _movementController;
        private float _movementSpeed;
        private Action _onReturnToWater;
        private WaterArea _waterArea;
        public OutOfWaterState(MovementController movementController,WaterArea waterArea,Action onReturnToWater)
        {
            _movementController = movementController;
            _movementSpeed = _movementController.MovementSpeed;
            _waterArea = waterArea;
            _onReturnToWater = onReturnToWater;
        }

        protected override void OnActivate()
        {
            _movementController.MovementSpeed= _movementSpeed*3f;
            Vector3 direction = Vector3.right;
            if (Vector3.Dot(_movementController.GetForward(), direction) < 0)
            {
                direction = -Vector3.right;
            }
            Vector3 pos = _movementController.GetPosition() + direction * 4f;
            pos.y = _waterArea.TopRight.position.y - 2f;
            _movementController.MoveTo(pos);
        }

        protected override void OnDeactivate()
        {
            _movementController.MovementSpeed = _movementSpeed;
        }

        protected override void Update(float deltaTime)
        {
            if (_movementController.GetPosition().y < _waterArea.TopRight.position.y - 1f)
            {
                _onReturnToWater?.Invoke();
            }
        }
    }
}
