using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using DragonBones;

namespace DragonBones.MonoGame
{
    public class MonoGameArmature : Armature, IArmatureProxy
    {
        public SpriteBatch SpriteBatch { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;

        protected override void _OnClear()
        {
            base._OnClear();
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
        }
        #endregion

        #region IEventDispatcher 实现
        public bool HasDBEventListener(string type)
        {
            return false;
        }

        public void DispatchDBEvent(string type, EventObject eventObject)
        {
        }

        public void AddDBEventListener(string type, ListenerDelegate<EventObject> listener)
        {
        }

        public void RemoveDBEventListener(string type, ListenerDelegate<EventObject> listener)
        {
        }

        public void ClearDBEventListeners()
        {
        }
        #endregion
    }
}
