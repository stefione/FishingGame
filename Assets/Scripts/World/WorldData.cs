using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.World
{
    public class WorldData : MonoBehaviour
    {
        public static WorldData Instance { get;private set; }

        public WaterArea WaterArea;
        public List<Transform> SpawnPoints;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

    }
}
