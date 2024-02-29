using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace PixPlays.Fishing.RandomGenerator
{
    public static class RNG
    {
        public static bool RTP_WillCatch(List<bool> attempts,int sampleAttempts,float successChance)
        {
            int totalAttempts = 0;
            int successCount = 0;
            int lastAttemptsCount = attempts.Count % sampleAttempts;
            for (int i = attempts.Count - 1; i >= attempts.Count - lastAttemptsCount; i--)
            {
                if (attempts[i])
                {
                    successCount++;
                }
                totalAttempts++;
            }
            int projectedSuccessfulAttempts = (int)((successChance / 100f) * sampleAttempts);
            int remainingAttempts = sampleAttempts - totalAttempts;
            float remainingSuccessfullAttempts = projectedSuccessfulAttempts - successCount;
            if (remainingSuccessfullAttempts == 0)
            {
                //Has to fail;
                return false;
            }
            else
            {
                if (remainingSuccessfullAttempts == remainingAttempts)
                {
                    //Has to succeed;
                    return true;
                }
                else
                {
                    //We are free to do random
                    return Random.Range(0f, 100f) <= successChance;
                }
            }
        }

        public static string RNG_ItemCought(Dictionary<string, float> items)
        {
            float maxCatchValue = 0;
            Dictionary<string, Vector2> catchValues = new();
            foreach (var i in items)
            {
                catchValues.Add(i.Key, new Vector2(maxCatchValue, maxCatchValue + i.Value));
                maxCatchValue += i.Value;
            }
            float catchValue = Random.Range(0, maxCatchValue);
            string key = null;
            foreach (var catchItem in catchValues)
            {
                if (catchItem.Value.x < catchValue && catchItem.Value.y >= catchValue)
                {
                    key = catchItem.Key;
                    break;
                }
            }
            return key;
        }
    }
}
