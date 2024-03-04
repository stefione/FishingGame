using PixPlays.Fishing.Entities;
using PixPlays.Fishing.GameManagement;
using PixPlays.Fishing.Hook;
using PixPlays.Fishing.Player;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Configuration
{
    [CreateAssetMenu(menuName ="Data/GameConfig",fileName ="GameConfig")]
    public class GameConfiguration : SerializedScriptableObject
    {
        public int SampleAttempts;
        [Range(0, 100)]
        public float SuccessChance;
        public Dictionary<string,FishData> FishDatas;
        public Dictionary<string, FishController> FishTemplates;
        public int NumberOfFish;

        public ClientGameManager ClientGameManagerTemplate;
        public ServerGameManager ServerGameManagerTemplate;

        public ClientPlayerController PlayerTemplate;
        public ServerPlayerController ServerPlayerTemplate;
    }
}
