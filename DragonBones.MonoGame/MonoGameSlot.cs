using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using DragonBones;
using System;

namespace DragonBones.MonoGame
{
    public class MonoGameSlot : Slot
    {
        private Texture2D _texture;
        private Microsoft.Xna.Framework.Rectangle _sourceRect;
        private MonoGameArmature _proxy;

        // Cached render state from _UpdateColor / _UpdateBlendMode
        private Color _cachedColor = Color.White;
        private BlendMode _cachedBlendMode = BlendMode.Normal;
        private bool _textureRotated;

        protected override void _OnClear()
        {
            base._OnClear();
            _texture = null;
            _sourceRect = Microsoft.Xna.Framework.Rectangle.Empty;
            _proxy = null;
            _cachedColor = Color.White;
            _cachedBlendMode = BlendMode.Normal;
            _textureRotated = false;
        }

        protected override void _InitDisplay(object value, bool isRetain)
        {
        }

        protected override void _DisposeDisplay(object value, bool isRelease)
        {
        }

        protected override void _OnUpdateDisplay()
        {
            _proxy = _armature as MonoGameArmature;
        }

        protected override void _AddDisplay()
        {
        }

        protected override void _ReplaceDisplay(object value)
        {
        }

        protected override void _RemoveDisplay()
        {
        }

        protected override void _UpdateZOrder()
        {
            // Z-order sorting is handled by the Armature via _slots.Sort() in AdvanceTime().
            // The _zOrder value on each slot is already updated by Armature._SortZOrder().
        }

        protected override void _UpdateColor()
        {
            _cachedColor = new Color(
                (byte)(this._colorTransform.redMultiplier * 255),
                (byte)(this._colorTransform.greenMultiplier * 255),
                (byte)(this._colorTransform.blueMultiplier * 255),
                (byte)(this._colorTransform.alphaMultiplier * 255)
            );
        }

        protected override void _UpdateFrame()
        {
            if (_textureData != null)
            {
                var textureAtlasData = _textureData.parent as MonoGameTextureAtlasData;
                if (textureAtlasData != null && textureAtlasData.texture != null)
                {
                    _texture = textureAtlasData.texture;
                    var region = _textureData.region;
                    int w = (int)region.width;
                    int h = (int)region.height;
                    _textureRotated = _textureData.rotated && _textureData.frame == null;
                    if (_textureRotated)
                    {
                        // Swap width/height for rotated textures packed in atlas
                        int tmp = w;
                        w = h;
                        h = tmp;
                    }
                    _sourceRect = new Microsoft.Xna.Framework.Rectangle(
                        (int)region.x,
                        (int)region.y,
                        w,
                        h
                    );
                }
            }
            else
            {
                _texture = null;
                _sourceRect = Microsoft.Xna.Framework.Rectangle.Empty;
                _textureRotated = false;
            }
        }

        protected override void _UpdateMesh()
        {
        }

        protected override void _UpdateTransform()
        {
        }

        protected override void _IdentityTransform()
        {
        }

        internal override void _UpdateVisible()
        {
            _visible = _visible && _parent != null && _parent.visible;
        }

        internal override void _UpdateBlendMode()
        {
            _cachedBlendMode = this._blendMode;
            // Note: MonoGame SpriteBatch uses a single BlendState per Begin/End pair.
            // Per-slot blend modes require batching by blend mode (end/begin between groups)
            // or using a custom SpriteSortMode / custom shader.
            // For now, the blend mode is stored and available for external sorting/drawing.
        }

        public void Render(SpriteBatch spriteBatch)
        {
            if (!visible || _texture == null || _proxy == null)
            {
                return;
            }

            var matrix = this.globalTransformMatrix;

            float worldX = matrix.tx + _proxy.Position.X;
            float worldY = matrix.ty + _proxy.Position.Y;

            float scaleX = (float)Math.Sqrt(matrix.a * matrix.a + matrix.b * matrix.b);
            float scaleY = (float)Math.Sqrt(matrix.c * matrix.c + matrix.d * matrix.d);

            float rotation = (float)Math.Atan2(matrix.b, matrix.a);
            if (_textureRotated)
            {
                rotation += MathHelper.PiOver2;
            }

            // Use DragonBones-computed pivot (in texture-pixel space, pre-scaled).
            float pivotX = this._pivotX;
            float pivotY = this._pivotY;

            // Determine flip effects from the armature's flip flags.
            SpriteEffects effects = SpriteEffects.None;
            if (_armature.flipX) effects |= SpriteEffects.FlipHorizontally;
            if (_armature.flipY) effects |= SpriteEffects.FlipVertically;

            spriteBatch.Draw(
                _texture,
                new Vector2(worldX, worldY),
                _sourceRect,
                _cachedColor,
                rotation + _proxy.Rotation,
                new Vector2(pivotX, pivotY),
                new Vector2(scaleX * _proxy.Scale.X, scaleY * _proxy.Scale.Y),
                effects,
                0
            );
        }
    }
}
