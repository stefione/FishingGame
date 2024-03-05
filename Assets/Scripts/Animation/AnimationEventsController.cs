using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Animation
{
    public class AnimationEventsController : MonoBehaviour
    {
        public delegate void AnimationEventHandler();

        private Dictionary<string, AnimationEventHandler> Events=new();

        private void SendEvent(string eventName)
        {
            if(Events.TryGetValue(eventName,out var value)) 
            {
                value?.Invoke();
            }
        }
        public void Subscribe(string eventName,AnimationEventHandler handler) 
        { 
            if(Events.TryGetValue(eventName,out var value))
            {
                value += handler;
            }
            else
            {
                Events.Add(eventName, handler);
            }
        }
    }
}
