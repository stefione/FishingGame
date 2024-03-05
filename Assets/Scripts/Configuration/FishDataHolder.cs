using PixPlays.Fishing.Fish;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Configuration
{
    [CreateAssetMenu(fileName ="FishData",menuName ="Data/FishData")]
    public class FishDataHolder : ScriptableObject
    {
        public FishData Data;
    }
}
