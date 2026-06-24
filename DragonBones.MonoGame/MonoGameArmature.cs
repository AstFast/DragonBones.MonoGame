using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using DragonBones;
using MGBlendState = Microsoft.Xna.Framework.Graphics.BlendState;

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

        // Cached slot lists for grouped rendering
        private readonly List<MonoGameSlot> _renderSlots = new List<MonoGameSlot>();
        private readonly List<MonoGameSlot> _imageSlots = new List<MonoGameSlot>();
        private readonly List<MonoGameSlot> _meshSlots = new List<MonoGameSlot>();

        protected override void _OnClear()
        {
            base._OnClear();
            _eventListeners.Clear();
            _renderSlots.Clear();
            _imageSlots.Clear();
            _meshSlots.Clear();
            SpriteBatch = null;
            Position = Vector2.Zero;
            Rotation = 0;
            Scale = Vector2.One;
        }

        public void Render()
        {
            if (SpriteBatch == null)
                return;

            var slots = GetSlots();
            _renderSlots.Clear();
            _imageSlots.Clear();
            _meshSlots.Clear();
            foreach (var slot in slots)
            {
                var monoGameSlot = slot as MonoGameSlot;
                if (monoGameSlot == null || !monoGameSlot.visible)
                    continue;

                if (monoGameSlot.IsMeshDisplay)
                    _meshSlots.Add(monoGameSlot);
                else
                    _imageSlots.Add(monoGameSlot);
            }

            // Phase 1: Draw image slots grouped by blend mode (SpriteBatch)
            if (_imageSlots.Count > 0)
            {
                var sorted = new List<MonoGameSlot>(_imageSlots);
                sorted.Sort((a, b) => a.CachedBlendMode.CompareTo(b.CachedBlendMode));

                var currentBlendMode = (BlendMode)(-1);
                MGBlendState currentBlendState = null;
                var samplerState = SamplerState.PointClamp;

                foreach (var slot in sorted)
                {
                    if (slot.CachedBlendMode != currentBlendMode)
                    {
                        if (currentBlendMode != (BlendMode)(-1))
                        {
                            SpriteBatch.End();
                        }
                        currentBlendMode = slot.CachedBlendMode;
                        currentBlendState = GetBlendState(currentBlendMode);
                        SpriteBatch.Begin(
                            SpriteSortMode.Deferred,
                            currentBlendState,
                            samplerState,
                            DepthStencilState.None,
                            RasterizerState.CullNone,
                            null,
                            null
                        );
                    }

                    slot.Render(SpriteBatch);
                }

                if (currentBlendMode != (BlendMode)(-1))
                {
                    SpriteBatch.End();
                }
            }

            // Phase 2: Draw mesh slots directly (GraphicsDevice)
            foreach (var slot in _meshSlots)
            {
                slot.Render(SpriteBatch);
            }
        }

        private static MGBlendState GetBlendState(BlendMode mode)
        {
            switch (mode)
            {
                case BlendMode.Add:
                    return AdditiveBlend;
                case BlendMode.Normal:
                case BlendMode.Alpha:
                case BlendMode.Layer:
                default:
                    return AlphaBlend;
            }
        }

        private static readonly MGBlendState AlphaBlend = new MGBlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
        };

        private static readonly MGBlendState AdditiveBlend = new MGBlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One,
        };

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
