using PixPlays.Fishing.Fish;
using PixPlays.Fishing.GameManagement;
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
        public Dictionary<string,FishDataHolder> FishDatas;
        public Dictionary<string, FishTemplate> FishTemplates;
        public int NumberOfFish;

        [Space(20)]
        public ClientGameManager ClientGameManagerTemplate;
        public ServerGameManager ServerGameManagerTemplate;

        [Space(20)]
        public ClientPlayerController ClientPlayerTemplate;
        public ServerPlayerController ServerPlayerTemplate;

        [Space(20)]
        public ClientFishController ClientFishController;
        public ServerFishController ServerFishController;
    }
}
