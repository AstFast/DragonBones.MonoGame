using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using DragonBones;

namespace DragonBones.MonoGame
{
    public class MonoGameArmature : Armature, IArmatureProxy
    {
        private readonly Dictionary<string, List<ListenerDelegate<EventObject>>> _eventListeners =
            new Dictionary<string, List<ListenerDelegate<EventObject>>>();

        public SpriteBatch SpriteBatch { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;

        protected override void _OnClear()
        {
            base._OnClear();
            _eventListeners.Clear();
            SpriteBatch = null;
            Position = Vector2.Zero;
            Rotation = 0;
            Scale = Vector2.One;
        }

        public void Render()
        {
            foreach (var slot in GetSlots())
            {
                var monoGameSlot = slot as MonoGameSlot;
                if (monoGameSlot != null)
                {
                    monoGameSlot.Render(SpriteBatch);
                }
            }
        }

        #region IArmatureProxy 实现
        public Armature armature
        {
            get { return this; }
        }

        public Animation animation
        {
            get { return base.animation; }
        }

        public void DBInit(Armature armature)
        {
        }

        public void DBClear()
        {
        }

        public void DBUpdate()
        {
        }

        public void Dispose(bool disposeProxy)
        {
            SpriteBatch = null;
        }
        #endregion

        #region IEventDispatcher 实现
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
        #endregion
    }
}
