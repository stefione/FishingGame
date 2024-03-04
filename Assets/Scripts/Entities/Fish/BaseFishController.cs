using PixPlays.Fishing.Movement;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.World;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.Entities
{
    public class BaseFishController : NetworkBehaviour
    {
        public ulong NetworkId;
        public FishData FishData;
        public ClientPlayerController Owner;
        protected MovementController _movementController;
        protected WaterArea _waterArea;
        public void ResetFish()
        {
            Owner = null;
            RandomMove();
        }

        public virtual void RandomMove() { }
    }
}