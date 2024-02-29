using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Player
{
    [System.Serializable]
    public class PlayerData
    {
        public string Id;
        public string Name;
        public List<bool> Attempts=new();
        public Dictionary<string, int> FishCought=new();
    }
}
