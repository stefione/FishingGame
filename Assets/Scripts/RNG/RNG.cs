using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace PixPlays.Fishing.RandomGenerator
{
    public static class RNG
    {
        public static bool GetSuccessAttempt(List<bool> attempts,int sampleAttempts,float successChance)
        {
            int totalAttempts = 0;
            int successCount = 0;
            if (attempts == null)
            {
                attempts = new();
            }
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

        public static T SelectRandomItem<T>(Dictionary<T, float> items)
        {
            float maxCatchValue = 0;
            Dictionary<T, Vector2> catchValues = new();
            foreach (var i in items)
            {
                catchValues.Add(i.Key, new Vector2(maxCatchValue, maxCatchValue + i.Value));
                maxCatchValue += i.Value;
            }
            float catchValue = Random.Range(0, maxCatchValue);
            T key = default;
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

        public static List<T> SelectRandomItems<T>(Dictionary<T, float> items,int count)
        {
            float maxCatchValue = 0;
            Dictionary<T, Vector2> catchValues = new();
            foreach (var i in items)
            {
                catchValues.Add(i.Key, new Vector2(maxCatchValue, maxCatchValue + i.Value));
                maxCatchValue += i.Value;
            }
            List<T> result = new();
            for (int i = 0; i < count; i++)
            {
                float catchValue = Random.Range(0, maxCatchValue);
                foreach (var catchItem in catchValues)
                {
                    if (catchItem.Value.x < catchValue && catchItem.Value.y >= catchValue)
                    {
                        result.Add(catchItem.Key);
                        break;
                    }
                }
            }
            return result;
        }
    }
}
