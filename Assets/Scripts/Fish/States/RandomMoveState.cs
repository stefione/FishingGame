using PixPlays.Fishing.Fish;
using PixPlays.Fishing.Movement;
using PixPlays.Fishing.States;
using PixPlays.Fishing.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace PixPlays.Fishing.Fish
{
    public class RandomMoveState : EntityState
    {
        private BaseFishController _fishController;
        private MovementController _movementController;
        private WaterArea _waterArea;

        public RandomMoveState(BaseFishController fishController,MovementController movementController, WaterArea waterArea)
        {
            _fishController = fishController;
            _movementController = movementController;
            _waterArea = waterArea;
        }

        protected override void OnActivate()
        {
            _movementController.OnDestinationReached += _movementController_OnDestinationReached;
            SetRandomDestination();
        }

        protected override void OnDeactivate()
        {
            _movementController.OnDestinationReached -= _movementController_OnDestinationReached;
        }
        protected override void Update(float deltaTime)
        {
        }

        private void _movementController_OnDestinationReached(Vector3 arg1, Vector3 arg2)
        {
            SetRandomDestination();
        }

        private void SetRandomDestination()
        {
            float x = Random.Range(_waterArea.BottomLeft.position.x, _waterArea.TopRight.position.x);
            float y = Random.Range(_waterArea.BottomLeft.position.y, _waterArea.TopRight.position.y);
            _movementController.MoveTo(new Vector3(x, y, 0));
        }

  
    }
}
