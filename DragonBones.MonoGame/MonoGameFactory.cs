using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using DragonBones;

namespace DragonBones.MonoGame
{
    public class MonoGameFactory : BaseFactory
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<Texture2D, Texture2D> _textureCache = new Dictionary<Texture2D, Texture2D>();

        public MonoGameFactory(GraphicsDevice graphicsDevice, DataParser dataParser = null) : base(dataParser)
        {
            _graphicsDevice = graphicsDevice;
        }

        protected override TextureAtlasData _BuildTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas)
        {
            if (textureAtlasData == null)
            {
                textureAtlasData = BaseObject.BorrowObject<MonoGameTextureAtlasData>();
            }
            
            if (textureAtlasData != null && textureAtlas is Texture2D)
            {
                var monoGameTextureAtlasData = textureAtlasData as MonoGameTextureAtlasData;
                if (monoGameTextureAtlasData != null)
                {
                    monoGameTextureAtlasData.texture = textureAtlas as Texture2D;
                }
            }
            
            return textureAtlasData;
        }

        protected override Armature _BuildArmature(BuildArmaturePackage dataPackage)
        {
            var armature = BaseObject.BorrowObject<MonoGameArmature>();
            armature.Init(dataPackage.armature, armature, armature, this._dragonBones);
            return armature;
        }

        protected override Slot _BuildSlot(BuildArmaturePackage dataPackage, SlotData slotData, Armature armature)
        {
            var slot = BaseObject.BorrowObject<MonoGameSlot>();
            // 使用slot本身作为rawDisplay和meshDisplay，因为我们不需要单独的显示对象
            slot.Init(slotData, armature, slot, slot);
            return slot;
        }

        public void Dispose()
        {
            foreach (var texture in _textureCache.Values)
            {
                texture.Dispose();
            }
            _textureCache.Clear();
        }
    }
}
