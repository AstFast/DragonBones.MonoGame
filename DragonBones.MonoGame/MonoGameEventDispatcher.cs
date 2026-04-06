using System;
using System.Collections.Generic;
using DragonBones;

namespace DragonBones.MonoGame
{
    public class MonoGameEventDispatcher : IEventDispatcher<EventObject>
    {
        private readonly Dictionary<string, List<ListenerDelegate<EventObject>>> _eventListeners = new Dictionary<string, List<ListenerDelegate<EventObject>>>();

        public bool HasDBEventListener(string type)
        {
            return _eventListeners.ContainsKey(type);
        }

        public void DispatchDBEvent(string type, EventObject eventObject)
        {
            if (_eventListeners.TryGetValue(type, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    listener(type, eventObject);
                }
            }
        }

        public void AddDBEventListener(string type, ListenerDelegate<EventObject> listener)
        {
            if (!_eventListeners.TryGetValue(type, out var listeners))
            {
                listeners = new List<ListenerDelegate<EventObject>>();
                _eventListeners[type] = listeners;
            }
            
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        public void RemoveDBEventListener(string type, ListenerDelegate<EventObject> listener)
        {
            if (_eventListeners.TryGetValue(type, out var listeners))
            {
                listeners.Remove(listener);
                if (listeners.Count == 0)
                {
                    _eventListeners.Remove(type);
                }
            }
        }

        public void ClearDBEventListeners()
        {
            _eventListeners.Clear();
        }
    }
}
