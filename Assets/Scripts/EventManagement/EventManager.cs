using System;
using System.Collections.Generic;
using UnityEditor;

namespace PixPlays.Framework.Events
{
    public class EventManager
    {
        private static Dictionary<Type, List<Delegate>> _eventDictionary = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> action)
        {
            if (_eventDictionary.TryGetValue(typeof(T), out List<Delegate> subscribers))
            {
                subscribers.Add(action);
            }
            else
            {
                List<Delegate> list = new List<Delegate>();
                list.Add(action);
                _eventDictionary.Add(typeof(T), list);
            }
        }

        public static void Fire<T>(T eventObject = default)
        {
            if(eventObject is Enum)
            {
                string uniqueId = eventObject.GetType().ToString() + "." + eventObject.ToString();
                Fire(uniqueId);
                return;
            }
            if (_eventDictionary.TryGetValue(typeof(T), out List<Delegate> subscribers))
            {
                List<Delegate> toRemove = new List<Delegate>();
                for (int i = 0; i < subscribers.Count; i++)
                {
                    if (subscribers[i].Target != null && !subscribers[i].Target.Equals(null))
                    {
                        Action<T> action = (Action<T>)subscribers[i];
                        action?.Invoke(eventObject);
                    }
                    else
                    {
                        toRemove.Add(subscribers[i]);
                    }
                }
                for (int i = 0; i < toRemove.Count; i++)
                {
                    subscribers.Remove(toRemove[i]);
                }
            }
        }

        public static Dictionary<string, List<Delegate>> _emptyEventsDictionary = new Dictionary<string, List<Delegate>>();

        public static void Subscribe<T>(T eventEnum, Action action) where T : Enum
        {
            string uniqueId=eventEnum.GetType().ToString()+"."+ eventEnum.ToString();
            Subscribe(uniqueId, action);
        }

        private static void Subscribe(string eventName, Action action)
        {
            if (_emptyEventsDictionary.TryGetValue(eventName, out List<Delegate> subscribers))
            {
                subscribers.Add(action);
            }
            else
            {
                List<Delegate> list = new List<Delegate>();
                list.Add(action);
                _emptyEventsDictionary.Add(eventName, list);
            }
        }

        private static void Fire(string eventName)
        {
            if (_emptyEventsDictionary.TryGetValue(eventName, out List<Delegate> subscribers))
            {
                List<Delegate> toRemove = new List<Delegate>();
                for (int i = 0; i < subscribers.Count; i++)
                {
                    if (subscribers[i].Target != null && !subscribers[i].Target.Equals(null))
                    {
                        Action action = (Action)subscribers[i];
                        action?.Invoke();
                    }
                    else
                    {
                        toRemove.Add(subscribers[i]);
                    }
                }
                for (int i = 0; i < toRemove.Count; i++)
                {
                    subscribers.Remove(toRemove[i]);
                }
            }
        }
    }
}