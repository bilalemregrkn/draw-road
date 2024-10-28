using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadSystem.EventBus
{
    public class EventBus : MonoBehaviour
    {
        private static EventBus _instance;

        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("EventBus");
                    _instance = obj.AddComponent<EventBus>();
                    DontDestroyOnLoad(obj); // Persist between scenes
                }

                return _instance;
            }
        }

        private readonly Dictionary<Type, List<Delegate>> _eventDictionary = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> subscriber) where T : struct
        {
            var eventType = typeof(T);
            if (!_eventDictionary.ContainsKey(eventType))
            {
                _eventDictionary[eventType] = new List<Delegate>();
            }

            _eventDictionary[eventType].Add(subscriber);
        }

        public void Unsubscribe<T>(Action<T> subscriber) where T : struct
        {
            var eventType = typeof(T);
            if (_eventDictionary.ContainsKey(eventType))
            {
                _eventDictionary[eventType].Remove(subscriber);
            }
        }

        public void Publish<T>(T eventData) where T : struct
        {
            var eventType = typeof(T);
            if (_eventDictionary.ContainsKey(eventType))
            {
                foreach (var action in _eventDictionary[eventType])
                {
                    (action as Action<T>)?.Invoke(eventData);
                }
            }
        }
    }
}