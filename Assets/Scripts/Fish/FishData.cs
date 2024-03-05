using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Fish
{
    [System.Serializable]
    public class FishData
    {
        public string Id;
        public string Name;
        public FishRarity Rarity;
        [Range(0, 100)]
        public float CatchChance;
        [Range(0,100)]
        public float SpawnChance;
    }
}
