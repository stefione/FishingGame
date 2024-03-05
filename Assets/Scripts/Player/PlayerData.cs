using PixPlays.Fishing.Fish;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace PixPlays.Fishing.Player
{
    [System.Serializable]
    public class PlayerData
    {
        public string Name;
        public List<string> AttemptLog=new();
        public List<bool> AttemptLogSuccesses = new();
        public Dictionary<string, int> FishCought=new();

        public PlayerData(PlayerDataMessage data)
        {
            Name = data.Name;
            AttemptLogSuccesses = data.AttemptLogSuccesses.ToList();
            AttemptLog = data.AttemptLog;
            FishCought = new();
            if (data.FishCought != null)
                foreach (var i in data.FishCought)
                {
                    FishCought.Add(i.Id, i.Count);
                }
        }

        public void UpdateAttempt(bool isSuccess,FishData fishData)
        {
            if (isSuccess)
            {
                AttemptLogSuccesses.Add(true);
                AttemptLog.Add(fishData.Name);
                if (FishCought.ContainsKey(fishData.Id))
                {
                    FishCought[fishData.Id]++;
                }
                else
                {
                    FishCought.Add(fishData.Id, 1);
                }
            }
            else
            {
                AttemptLogSuccesses.Add(false);
                AttemptLog.Add("Missed");
            }

        }
    }
}
