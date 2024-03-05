using PixPlays.Fishing.Fish;
using PixPlays.Fishing.Movement;
using PixPlays.Fishing.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Fish
{
    public class SyncedMoveState : EntityState
    {
        private ClientFishController _clientFishController;
        private MovementController _movementController;
        public SyncedMoveState(ClientFishController clientFishController,MovementController movementController)
        {
            _movementController=movementController;
            _clientFishController =clientFishController;
        }

        protected override void OnActivate()
        {
        }

        protected override void OnDeactivate()
        {
        }

        protected override void Update(float deltaTime)
        {
            _movementController.Teleport(
                Vector3.Lerp(_movementController.GetPosition(), 
                _clientFishController.TargetPos, 
                Time.deltaTime * _clientFishController.SyncLerpSpeed));
        }
    }
}
