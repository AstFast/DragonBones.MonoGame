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

        protected override void _OnClear()
        {
            base._OnClear();
            _texture = null;
            _sourceRect = Microsoft.Xna.Framework.Rectangle.Empty;
            _proxy = null;
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
        }

        protected override void _UpdateColor()
        {
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
                    _sourceRect = new Microsoft.Xna.Framework.Rectangle(
                        (int)region.x,
                        (int)region.y,
                        (int)region.width,
                        (int)region.height
                    );
                }
            }
            else
            {
                _texture = null;
                _sourceRect = Microsoft.Xna.Framework.Rectangle.Empty;
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
            visible = _parent != null && _parent.visible;
        }

        internal override void _UpdateBlendMode()
        {
        }

        public void Render(SpriteBatch spriteBatch)
        {
            if (!visible || _texture == null || _proxy == null)
            {
                System.Diagnostics.Debug.WriteLine($"Render skipped: visible={visible}, _texture={_texture != null}, _proxy={_proxy != null}");
                return;
            }

            var matrix = this.globalTransformMatrix;
            
            float worldX = matrix.tx + _proxy.Position.X;
            float worldY = matrix.ty + _proxy.Position.Y;
            
            float scaleX = (float)Math.Sqrt(matrix.a * matrix.a + matrix.b * matrix.b);
            float scaleY = (float)Math.Sqrt(matrix.c * matrix.c + matrix.d * matrix.d);
            
            float rotation = (float)Math.Atan2(matrix.b, matrix.a);

            float pivotX = _sourceRect.Width * 0.5f;
            float pivotY = _sourceRect.Height * 0.5f;

            var color = new Color(
                (byte)(this._colorTransform.redMultiplier * 255),
                (byte)(this._colorTransform.greenMultiplier * 255),
                (byte)(this._colorTransform.blueMultiplier * 255),
                (byte)(this._colorTransform.alphaMultiplier * 255)
            );

            spriteBatch.Draw(
                _texture,
                new Vector2(worldX, worldY),
                _sourceRect,
                color,
                rotation + _proxy.Rotation,
                new Vector2(pivotX, pivotY),
                new Vector2(scaleX * _proxy.Scale.X, scaleY * _proxy.Scale.Y),
                SpriteEffects.None,
                0
            );
        }
    }
}
