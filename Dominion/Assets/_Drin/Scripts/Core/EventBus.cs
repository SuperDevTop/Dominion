using System.Collections.Generic;
using System;
using UnityEngine;

public class EventBus : MonoBehaviour
{
    public Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();
    private static EventBus instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        subscribers.Clear();
    }

    public static void Publish<T>(T eventToPublish)
    {
        var eventType = typeof(T);
        if (instance.subscribers.ContainsKey(eventType))
        {
            var delegates = instance.subscribers[eventType];
            foreach (var del in delegates)
            {
                if (del == null) continue;
                ((Action<T>)del)?.Invoke(eventToPublish);
            }
        }
    }

    public static void Subscribe<T>(Action<T> action)
    {
        var eventType = typeof(T);
        if (!instance.subscribers.ContainsKey(eventType))
        {
            instance.subscribers[eventType] = new List<Delegate>();
        }
        instance.subscribers[eventType].Add(action);
    }
}
